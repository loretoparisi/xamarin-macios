﻿using System;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Xamarin.MacDev;
using Xamarin.MacDev.Tasks;

namespace Xamarin.iOS.Tasks
{
	public class ArchiveTaskBase : Xamarin.MacDev.Tasks.ArchiveTaskBase
	{
		public ITaskItem[] AppExtensionReferences { get; set; }

		static bool IsWatchAppExtension (ITaskItem appex, PDictionary plist, out string watchAppBundleDir)
		{
			PString expectedBundleIdentifier, bundleIdentifier, extensionPoint;
			PDictionary extension, attributes;

			watchAppBundleDir = null;

			if (!plist.TryGetValue ("NSExtension", out extension))
				return false;

			if (!extension.TryGetValue ("NSExtensionPointIdentifier", out extensionPoint))
				return false;

			if (extensionPoint.Value != "com.apple.watchkit")
				return false;

			// Okay, we've found the WatchKit App Extension...
			if (!extension.TryGetValue ("NSExtensionAttributes", out attributes))
				return false;

			if (!attributes.TryGetValue ("WKAppBundleIdentifier", out expectedBundleIdentifier))
				return false;

			var pwd = PathUtils.ResolveSymbolicLink (Environment.CurrentDirectory);

			// Scan the *.app subdirectories to find the WatchApp bundle...
			foreach (var bundle in Directory.GetDirectories (appex.ItemSpec, "*.app")) {
				if (!File.Exists (Path.Combine (bundle, "Info.plist")))
					continue;

				plist = PDictionary.FromFile (Path.Combine (bundle, "Info.plist"));

				if (!plist.TryGetValue ("CFBundleIdentifier", out bundleIdentifier))
					continue;

				if (bundleIdentifier.Value != expectedBundleIdentifier.Value)
					continue;

				watchAppBundleDir = PathUtils.AbsoluteToRelative (pwd, PathUtils.ResolveSymbolicLink (bundle));

				return true;
			}

			return false;
		}

		void ArchiveAppExtension (ITaskItem appex, string archiveDir)
		{
			var plist = PDictionary.FromFile (Path.Combine (appex.ItemSpec, "Info.plist"));
			string watchAppBundleDir;

			if (IsWatchAppExtension (appex, plist, out watchAppBundleDir)) {
				var wk = Path.Combine (watchAppBundleDir, "_WatchKitStub", "WK");
				var supportDir = Path.Combine (archiveDir, "WatchKitSupport");

				if (File.Exists (wk) && !Directory.Exists (supportDir)) {
					Directory.CreateDirectory (supportDir);
					File.Copy (wk, Path.Combine (supportDir, "WK"), true);
				}
			}

			var dsymDir = appex.ItemSpec + ".dSYM";
			var destDir = Path.Combine (archiveDir, "dSYMs", Path.GetFileName (dsymDir));

			Ditto (dsymDir, destDir);
		}

		void AddIconPaths (PArray icons, PArray iconFiles, string productsDir)
		{
			foreach (var icon in iconFiles.Cast<PString> ().Where (p => p.Value != null)) {
				var path = string.Format ("Applications/{0}/{1}", Path.GetFileName (AppBundleDir.ItemSpec), icon.Value);
				bool addDefault = true;

				if (path.EndsWith (".png", StringComparison.Ordinal)) {
					icons.Add (new PString (path));
					continue;
				}

				if (File.Exists (Path.Combine (productsDir, path + "@3x.png"))) {
					icons.Add (new PString (path + "@3x.png"));
					addDefault = false;
				}

				if (File.Exists (Path.Combine (productsDir, path + "@2x.png"))) {
					icons.Add (new PString (path + "@2x.png"));
					addDefault = false;
				}

				if (addDefault || File.Exists (Path.Combine (productsDir, path + ".png")))
					icons.Add (new PString (path + ".png"));
			}
		}

		public override bool Execute ()
		{
			Log.LogTaskName ("Archive");
			Log.LogTaskProperty ("AppBundleDir", AppBundleDir);
			Log.LogTaskProperty ("AppExtensionReferences", AppExtensionReferences);
			Log.LogTaskProperty ("ITunesSourceFiles", ITunesSourceFiles);
			Log.LogTaskProperty ("OutputPath", OutputPath);
			Log.LogTaskProperty ("ProjectName", ProjectName);
			Log.LogTaskProperty ("SigningKey", SigningKey);
			Log.LogTaskProperty ("SolutionPath", SolutionPath);

			var archiveDir = CreateArchiveDirectory ();

			try {
				var plist = PDictionary.FromFile (Path.Combine (AppBundleDir.ItemSpec, "Info.plist"));
				var productsDir = Path.Combine (archiveDir, "Products");

				// Archive the OnDemandResources...
				var resourcesDestDir = Path.Combine (productsDir, "OnDemandResources");
				var resourcesSrcDir = Path.Combine (OutputPath, "OnDemandResources");

				if (Directory.Exists (resourcesSrcDir))
					Ditto (resourcesSrcDir, resourcesDestDir);

				// Archive the Applications...
				var appDestDir = Path.Combine (productsDir, "Applications", Path.GetFileName (AppBundleDir.ItemSpec));
				Ditto (AppBundleDir.ItemSpec, appDestDir);

				// Archive the dSYMs...
				var dsymsDestDir = Path.Combine (archiveDir, "dSYMs", Path.GetFileName (DSYMDir));
				Ditto (DSYMDir, dsymsDestDir);

				// Archive the Bitcode symbol maps
				var bcSymbolMaps = Directory.GetFiles (Path.GetDirectoryName (DSYMDir), "*.bcsymbolmap");
				if (bcSymbolMaps.Length > 0) {
					var bcSymbolMapsDir = Path.Combine (archiveDir, "BCSymbolMaps");

					Directory.CreateDirectory (bcSymbolMapsDir);

					for (int i = 0; i < bcSymbolMaps.Length; i++)
						File.Copy (bcSymbolMaps[i], Path.Combine (bcSymbolMapsDir, Path.GetFileName (bcSymbolMaps[i])));
				}

				if (AppExtensionReferences != null) {
					// Archive the dSYMs for each of the referenced App Extensions as well...
					for (int i = 0; i < AppExtensionReferences.Length; i++)
						ArchiveAppExtension (AppExtensionReferences[i], archiveDir);
				}

				if (ITunesSourceFiles != null) {
					// Archive the iTunesMetadata.plist and iTunesArtwork files...
					var iTunesMetadataDir = Path.Combine (archiveDir, "iTunesMetadata", Path.GetFileName (AppBundleDir.ItemSpec));
					for (int i = 0; i < ITunesSourceFiles.Length; i++) {
						var archivedMetaFile = Path.Combine (iTunesMetadataDir, Path.GetFileName (ITunesSourceFiles[i].ItemSpec));

						Directory.CreateDirectory (iTunesMetadataDir);
						File.Copy (ITunesSourceFiles[i].ItemSpec, archivedMetaFile, true);
					}
				}

				// Generate an archive Info.plist
				var arInfo = new PDictionary ();
				// FIXME: figure out this value
				//arInfo.Add ("AppStoreFileSize", new PNumber (65535));

				var props = new PDictionary ();
				props.Add ("ApplicationPath", new PString (string.Format ("Applications/{0}", Path.GetFileName (AppBundleDir.ItemSpec))));
				props.Add ("CFBundleIdentifier", new PString (plist.GetCFBundleIdentifier ()));
				if (plist.GetCFBundleShortVersionString () != null)
					props.Add ("CFBundleShortVersionString", new PString (plist.GetCFBundleShortVersionString ()));
				else if (plist.GetCFBundleVersion () != null)
					props.Add ("CFBundleShortVersionString", new PString (plist.GetCFBundleVersion ()));

				var iconFiles = plist.GetCFBundleIconFiles ();
				var iconDict = plist.GetCFBundleIcons ();
				var icons = new PArray ();

				if (iconFiles != null)
					AddIconPaths (icons, iconFiles, Path.Combine (archiveDir, "Products"));

				if (iconDict != null) {
					var primary = iconDict.Get<PDictionary> (ManifestKeys.CFBundlePrimaryIcon);
					if (primary != null && (iconFiles = primary.GetCFBundleIconFiles ()) != null)
						AddIconPaths (icons, iconFiles, Path.Combine (archiveDir, "Products"));
				}

				if (icons.Count > 0)
					props.Add ("IconPaths", icons);

				props.Add ("SigningIdentity", new PString (SigningKey));

				arInfo.Add ("ApplicationProperties", props);
				arInfo.Add ("ArchiveVersion", new PNumber (2));
				arInfo.Add ("CreationDate", new PDate (Now.ToUniversalTime ()));
				arInfo.Add ("Name", new PString (plist.GetCFBundleName () ?? plist.GetCFBundleDisplayName ()));
				arInfo.Add ("SchemeName", new PString (ProjectName));

				if (!string.IsNullOrEmpty (SolutionPath)) {
					arInfo.Add ("SolutionName", new PString (Path.GetFileNameWithoutExtension (SolutionPath)));
					arInfo.Add ("SolutionPath", new PString (SolutionPath));
				}

				arInfo.Save (Path.Combine (archiveDir, "Info.plist"));

				ArchiveDir = archiveDir;
			} catch (Exception ex) {
				Log.LogErrorFromException (ex);
				Directory.Delete (archiveDir, true);
			}

			return !Log.HasLoggedErrors;
		}
	}
}
