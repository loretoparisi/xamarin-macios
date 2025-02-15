using System;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.Tests
{
	class Configuration
	{
		public const string XI_ProductName = "MonoTouch";
		public const string XM_ProductName = "Xamarin.Mac";

		static Dictionary<string,string> XS = new Dictionary<string, string> () {
			{ "MONO_GAC_PREFIX", "/Applications/Xamarin Studio.app/Contents/MacOS" },
			{ "PATH", "/Applications/Xamarin Studio.app/Contents/MacOS" },
			{ "DYLD_FALLBACK_LIBRARY_PATH", "/Library/Frameworks/Mono.framework/Versions/Current/lib:/lib:/usr/lib:/Applications/Xamarin Studio.app/Contents/MacOS" },
			{ "PKG_CONFIG_PATH", "/Applications/Xamarin Studio.app/Contents/MacOS" }
		};

		static string mt_root;
		static string ios_destdir;
		public static string mt_src_root;
		public static string sdk_version;
		public static string watchos_sdk_version;
		public static string tvos_sdk_version;
		public static string xcode_root;
		public static string XcodeVersion;
		public static string xcode5_root;
		public static string xcode6_root;
		public static string xcode72_root;
#if MONOMAC
		public static string mac_xcode_root;
#endif
		public static Dictionary<string, string> make_config = new Dictionary<string, string> ();

		public static bool include_ios;
		public static bool include_mac;
		public static bool include_tvos;
		public static bool include_watchos;

		// This is /Library/Frameworks/Xamarin.iOS.framework/Versions/Current if running
		// against a system XI, otherwise it's the <git checkout>/_ios-build/Library/Frameworks/Xamarin.iOS.framework/Versions/Current directory.
		public static string MonoTouchRootDirectory {
			get {
				return mt_root;
			}
		}

		static string FindConfigFile (string name)
		{
			var dir = Environment.CurrentDirectory;
			while (dir != "/") {
				var file = Path.Combine (dir, name);
				if (File.Exists (file))
					return file;
				dir = Path.GetDirectoryName (dir);
			}
			return null;
		}

		static void ParseConfigFiles ()
		{
			ParseConfigFile (FindConfigFile ("test.config"));
			ParseConfigFile (FindConfigFile ("Make.config.local"));
			ParseConfigFile (FindConfigFile ("Make.config"));
		}

		static void ParseConfigFile (string file)
		{
			if (string.IsNullOrEmpty (file))
				return;

			foreach (var line in File.ReadAllLines (file)) {
				var eq = line.IndexOf ('=');
				if (eq == -1)
					continue;
				var key = line.Substring (0, eq);
				if (!make_config.ContainsKey (key))
					make_config [key] = line.Substring (eq + 1);
			}
		}

		static string GetVariable (string variable, string @default)
		{
			var result = Environment.GetEnvironmentVariable (variable);
			if (string.IsNullOrEmpty (result))
				make_config.TryGetValue (variable, out result);
			if (string.IsNullOrEmpty (result))
				result = @default;
			return result;
		}

		static Configuration ()
		{
			ParseConfigFiles ();

			mt_root = GetVariable ("MONOTOUCH_PREFIX", "/Library/Frameworks/Xamarin.iOS.framework/Versions/Current");
			ios_destdir = GetVariable ("IOS_DESTDIR", null);
			sdk_version = GetVariable ("IOS_SDK_VERSION", "8.0");
			watchos_sdk_version = GetVariable ("WATCH_SDK_VERSION", "2.0");
			tvos_sdk_version = GetVariable ("TVOS_SDK_VERSION", "9.0");
			xcode_root = GetVariable ("XCODE_DEVELOPER_ROOT", "/Applications/Xcode.app/Contents/Developer");
			xcode5_root = GetVariable ("XCODE5_DEVELOPER_ROOT", "/Applications/Xcode511.app/Contents/Developer");
			xcode6_root = GetVariable ("XCODE6_DEVELOPER_ROOT", "/Applications/Xcode601.app/Contents/Developer");
			xcode72_root = GetVariable ("XCODE72_DEVELOPER_ROOT", "/Applications/Xcode72.app/Contents/Developer");
			include_ios = !string.IsNullOrEmpty (GetVariable ("INCLUDE_IOS", ""));
			include_mac = !string.IsNullOrEmpty (GetVariable ("INCLUDE_MAC", ""));
			include_tvos = !string.IsNullOrEmpty (GetVariable ("INCLUDE_TVOS", ""));
			include_watchos = !string.IsNullOrEmpty (GetVariable ("INCLUDE_WATCH", ""));

			var version_plist = Path.Combine (xcode_root, "..", "version.plist");
			if (File.Exists (version_plist)) {
				var settings = new System.Xml.XmlReaderSettings ();
				settings.DtdProcessing = System.Xml.DtdProcessing.Ignore;
				var doc = new System.Xml.XmlDocument ();
				using (var fs = new FileStream (version_plist, FileMode.Open, FileAccess.Read)) {
					using (var reader = System.Xml.XmlReader.Create (fs, settings)) {
						doc.Load (reader);
						XcodeVersion = doc.DocumentElement.SelectSingleNode ("//dict/key[text()='CFBundleShortVersionString']/following-sibling::string[1]/text()").Value;
					}
				}
			}
#if MONOMAC
			mac_xcode_root = xcode_root;
#endif

			if (!Directory.Exists (mt_root) && !File.Exists (mt_root) && string.IsNullOrEmpty (ios_destdir))
				mt_root = "/Developer/MonoTouch";

			if (Directory.Exists (Path.Combine (mt_root, "usr")))
				mt_root = Path.Combine (mt_root, "usr");

			if (!string.IsNullOrEmpty (ios_destdir))
				mt_root = Path.Combine (ios_destdir, mt_root.Substring (1));

			Console.WriteLine ("Test configuration:");
			Console.WriteLine ("  MONOTOUCH_PREFIX={0}", mt_root);
			Console.WriteLine ("  IOS_DESTDIR={0}", ios_destdir);
			Console.WriteLine ("  SDK_VERSION={0}", sdk_version);
			Console.WriteLine ("  XCODE_ROOT={0}", xcode_root);
			Console.WriteLine ("  XCODE5_ROOT={0}", xcode5_root);
			Console.WriteLine ("  XCODE6_ROOT={0} Exists={1}", xcode6_root, Directory.Exists (xcode6_root));
#if MONOMAC
			Console.WriteLine ("  MAC_XCODE_ROOT={0}", mac_xcode_root);
#endif
			Console.WriteLine ("  INCLUDE_IOS={0}", include_ios);
			Console.WriteLine ("  INCLUDE_MAC={0}", include_mac);
			Console.WriteLine ("  INCLUDE_TVOS={0}", include_tvos);
			Console.WriteLine ("  INCLUDE_WATCHOS={0}", include_watchos);
		}

		public static string MaccorePath {
			get {
				var dir = Environment.CurrentDirectory;
				var path = Path.Combine (dir, "maccore");
				while (!Directory.Exists (path) && path.Length > 3) {
					dir = Path.GetDirectoryName (dir);
					path = Path.Combine (dir, "maccore");
				}
				if (!Directory.Exists (path))
					throw new Exception ("Could not find the maccore repo");
				return path;
			}
		}
			
		public static string SourceRoot {
			get {
				// might need tweaking.
				if (mt_src_root == null)
#if MONOMAC
					mt_src_root = Path.GetFullPath (Path.Combine (Environment.CurrentDirectory, "../../../.."));
#else
					mt_src_root = Path.GetFullPath (Path.Combine (Environment.CurrentDirectory, "../../../../.."));
#endif
				return mt_src_root;
			}
		}

		public static Dictionary<string,string> MonoDevelopLike {
			get {
				return XS;
			}
		}

		public static string MonoTouchDll {
			get {
				return Path.Combine (mt_root, "lib", "mono", "2.1", "monotouch.dll");
			}
		}

		public static string XamarinIOSDll {
			get {
				return Path.Combine (mt_root, "lib", "mono", "Xamarin.iOS", "Xamarin.iOS.dll");
			}
		}

		public static string XamarinWatchOSDll {
			get {
				return Path.Combine (mt_root, "lib", "mono", "Xamarin.WatchOS", "Xamarin.WatchOS.dll");
			}
		}

		public static string XamarinTVOSDll {
			get {
				return Path.Combine (mt_root, "lib", "mono", "Xamarin.TVOS", "Xamarin.TVOS.dll");
			}
		}

		public static string SdkBinDir {
			get {
#if MONOMAC
				return BinDirXM;
#else
				return BinDirXI;
#endif
			}
		}

		public static string BinDirXI {
			get {
				return Path.Combine (MaccorePath, "_ios-build", "Library", "Frameworks", "Xamarin.iOS.framework", "Versions", "Current", "bin");
			}
		}

		public static string BinDirXM {
			get {
				return Path.Combine (MaccorePath, "_mac-build", "Library", "Frameworks", "Xamarin.Mac.framework", "Versions", "Current", "bin");
			}
		}

		public static string SmcsPath {
			get {
				return Path.Combine (SdkBinDir, "smcs");
			}
		}

		public static string BtouchPath {
			get {
				return Path.Combine (SdkBinDir, "btouch");
			}
		}

		public static string MmpPath {
			get {
				return Path.Combine (BinDirXM, "mmp");
			}
		}

		public static string MtouchPath {
			get {
				return Path.Combine (BinDirXI, "mtouch");
			}
		}
	}
}
