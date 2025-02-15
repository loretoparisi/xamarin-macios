
/*
 * Copyright 2011-2014 Xamarin Inc. All rights reserved.
 * Copyright 2010 Novell Inc.
 *
 * Authors:
 *   Sebastien Pouliot <sebastien@xamarin.com>
 *   Aaron Bockover <abock@xamarin.com>
 *   Rolf Bjarne Kvinge <rolf@xamarin.com>
 *   Geoff Norton <gnorton@novell.com>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;

using Mono.Cecil;
using Mono.Linker;
using Mono.Options;
using Mono.Tuner;
using MonoMac.Tuner;
using Xamarin.Utils;
using Xamarin.Linker;

namespace Xamarin.Bundler {
	public enum RegistrarMode {
		Default,
		Dynamic,
		Static,
		IL,
	}

	public static partial class Driver {
		internal static Application App = new Application ();
		static Target BuildTarget = new Target (App);
		static List<string> references = new List<string> ();
		static List<string> resources = new List<string> ();
		static List<string> resolved_assemblies = new List<string> ();
		static List<string> ignored_assemblies = new List<string> ();
		static List<string> native_references = new List<string> ();
		static List<string> native_libraries_copied_in = new List<string> ();

		static string output_dir;
		static string app_name;
		static bool show_help = false;
		static bool show_version;
		static bool generate_plist;
		static RegistrarMode registrar = RegistrarMode.Default;
		static bool no_executable;
		static bool embed_mono = true;
		static bool? profiling = false;
		static bool? thread_check = null;
		static string link_flags = null;

		static bool arch_set = false;
		static string arch = "i386";
		static Version minos = new Version (10, 7);
		static Version sdk_version;
		static string contents_dir;
		static string frameworks_dir;
		static string macos_dir;
		static string resources_dir;
		static string mmp_dir;
		
		static string mono_dir;
		static string sdk_root;
		static string custom_bundle_name;

		static string tls_provider;
		static string http_message_provider;

		static string BundleName { get { return custom_bundle_name != null ? custom_bundle_name : "MonoBundle"; } }

		static string icon;
		static string certificate_name;
		static int verbose = 0;
		public static bool Force;

		// This must be kept in sync with the system launcher's minimum mono version (in launcher/launcher-system.m)
		static Version MinimumMonoVersion = new Version (4, 2, 0);
		const string pkg_config = "/Library/Frameworks/Mono.framework/Commands/pkg-config";

		static HashSet<string> xammac_reference_assemblies = new HashSet<string> {
			"Xamarin.Mac.dll",
			"Xamarin.Mac.CFNetwork.dll",
			"OpenTK.dll"
		};

		static void ShowHelp (OptionSet os) {
			Console.WriteLine ("mmp - Xamarin.Mac Packer");
			Console.WriteLine ("Copyright 2010 Novell Inc.");
			Console.WriteLine ("Copyright 2011-2016 Xamarin Inc.");
			Console.WriteLine ("Usage: mmp [options] application-exe");
			os.WriteOptionDescriptions (Console.Out);
		}

		public static bool IsUnifiedFullXamMacFramework { get; private set; }
		public static bool IsUnifiedFullSystemFramework { get; private set; }
		public static bool IsUnifiedMobile { get; private set; }
		public static bool IsUnified { get { return IsUnifiedFullSystemFramework || IsUnifiedMobile || IsUnifiedFullXamMacFramework; } }
		public static bool IsClassic { get { return !IsUnified; } }

		public static bool Is64Bit { 
			get {
				if (IsUnified && !arch_set)
					return true;

				return arch == "x86_64";
			}
		}

		public static Version SDKVersion { get { return sdk_version; } }

		public static Version MinOSVersion { get { return minos; } }

		static int watch_level;
		static Stopwatch watch;
		
		static void Watch (string msg, int level)
		{
			if (watch != null && (watch_level > level))
				Console.WriteLine ("{0}: {1} ms", msg, watch.ElapsedMilliseconds);
		}

		public static bool EnableDebug {
			get { return App.EnableDebug; }
		}

		public static int Main (string [] args)
		{
			try {
				Console.OutputEncoding = new UTF8Encoding (false, false);
				Main2 (args);
			}
			catch (Exception e) {
				ErrorHelper.Show (e);
			}
			finally {
				Watch ("Total time", 0);
			}
			return 0;
		}

		static void Main2 (string [] args)
		{
			var os = new OptionSet () {
				{ "h|?|help", "Displays the help", v => show_help = true },
				{ "version", "Output version information and exit.", v => show_version = true },
				{ "f|force", "Forces the recompilation of code, regardless of timestamps", v=> Force = true },
				{ "cache=", "Specify the directory where temporary build files will be cached", v => Cache.Location = v },
				{ "a|assembly=", "Add an assembly to be processed", v => references.Add (v) },
				{ "r|resource=", "Add a resource to be included", v => resources.Add (v) },
				{ "o|output=", "Specify the output path", v => output_dir = v },
				{ "n|name=", "Specify the application name", v => app_name = v },
				{ "d|debug", "Build a debug bundle", v => App.EnableDebug = true },
				{ "s|sgen:", "Use the SGen Garbage Collector",
					v => {
						if (!ParseBool (v, "sgen")) 
							ErrorHelper.Warning (43, "The Boehm garbage collector is not supported. The SGen garbage collector has been selected instead.");
					},
					true // do not show the option anymore
				},
				{ "boehm:", "Enable the Boehm garbage collector", 
					v => {
						if (ParseBool (v, "boehm"))
							ErrorHelper.Warning (43, "The Boehm garbage collector is not supported. The SGen garbage collector has been selected instead."); }, 
					true // do not show the option anymore
				},
				{ "new-refcount:", "Enable new refcounting logic",
					v => {
						if (!ParseBool (v, "new-refcount"))
							ErrorHelper.Warning (80, "Disabling the new refcount logic is deprecated.");
					},
					true // do not show this option anymore
				},
				{ "nolink", "Do not link the assemblies", v => App.LinkMode = LinkMode.None },
				{ "mapinject", "Inject a fast method map [deprecated]", v => { ErrorHelper.Show (new MonoMacException (16, false, "The option '{0}' has been deprecated.", "--mapinject")); } },
				{ "minos=", "Minimum supported version of Mac OS X", 
					v => {
						try {
							minos = Version.Parse (v);
						} catch (Exception ex) {
							ErrorHelper.Error (26, ex, "Could not parse the command line argument '{0}': {1}", "-minos", ex.Message);
						}
					}
				},
				{ "linksdkonly", "Link only the SDK assemblies", v => App.LinkMode = LinkMode.SDKOnly },
				{ "linkskip=", "Skip linking of the specified assembly", v => App.LinkSkipped.Add (v) },
				{ "i18n=", "List of i18n assemblies to copy to the output directory, separated by commas (none,all,cjk,mideast,other,rare,west)", v => App.I18n = LinkerOptions.ParseI18nAssemblies (v) },
				{ "c|certificate=", "The Code Signing certificate for the application", v => { certificate_name = v; }},
				{ "p", "Generate a plist for the application", v => { generate_plist = true; }},
				{ "v|verbose", "Verbose output", v => { verbose++; }},
				{ "q", "Quiet", v => verbose-- },
				{ "i|icon=", "Use the specified file as the bundle icon", v => { icon = v; }},
				{ "xml=", "Provide an extra XML definition file to the linker", v => App.Definitions.Add (v) },
				{ "time", v => watch_level++ },
				{ "sdkroot=", "Specify the location of Apple SDKs", v => sdk_root = v },
				{ "arch=", "Specify the architecture ('i386' or 'x86_64') of the native runtime (default to 'i386')", v => { arch = v; arch_set = true; } },
				{ "profile=", "(Obsoleted in favor of --target-framework) Specify the .NET profile to use (defaults to '" + Xamarin.Utils.TargetFramework.Default + "')", v => SetTargetFramework (v) },
				{ "target-framework=", "Specify the .NET target framework to use (defaults to '" + Xamarin.Utils.TargetFramework.Default + "')", v => SetTargetFramework (v) },
				{ "force-thread-check", "Keep UI thread checks inside (even release) builds", v => { thread_check = true; }},
				{ "disable-thread-check", "Remove UI thread checks inside (even debug) builds", v => { thread_check = false; }},
				{ "registrar:", "Specify the registrar to use (dynamic [default], IL or static)", v =>
					{
						switch (v) {
						case "static":
							registrar = RegistrarMode.Static;
							break;
						case "dynamic":
							registrar = RegistrarMode.Dynamic;
							break;
						case "il":
							registrar = RegistrarMode.IL;
							break;
						case "default":
							registrar = RegistrarMode.Default;
							break;
						default:
							throw new MonoMacException (20, true, "The valid options for '{0}' are '{1}'.", "--registrar", "il, dynamic, static or default");
						}
					}, true /* hidden for now */
				},
				{ "sdk=", "Specifies the SDK version to compile against (version, for example \"10.9\")",
					v => {
						try {
							sdk_version = Version.Parse (v);
						} catch (Exception ex) {
							ErrorHelper.Error (26, ex, "Could not parse the command line argument '{0}': {1}", "-sdk", ex.Message);
						}
					}
				},
				{ "no-root-assembly", "Specifies that mmp will not process a root assembly. This is if the app needs to be packaged with a different directory structure than what mmp supports.", v => no_executable = true },
				{ "embed-mono:", "Specifies whether the app will embed the Mono runtime, or if it will use the system Mono found at runtime (default: true).", v => {
						embed_mono = ParseBool (v, "embed-mono");
					}
				},
				{ "link_flags=", "Specifies additional arguments to the native linker.",
					v => { link_flags = v; }
				},
				{ "ignore-native-library=", "Add a native library to be ignored during assembly scanning and packaging",
					v => ignored_assemblies.Add (v)
				},
				{ "native-reference=", "Add a native (static, dynamic, or framework) library to be included in the bundle. Can be specified multiple times.",
					v => {
						native_references.Add (v);
						if (v.EndsWith (".framework", true, CultureInfo.InvariantCulture))
							App.Frameworks.Add (v);
					}
				},
				{ "profiling:", "Enable profiling", v => profiling = ParseBool (v, "profiling") },
				{ "custom_bundle_name=", "Specify a custom name for the MonoBundle folder.", v => custom_bundle_name = v, true }, // Hidden hack for "universal binaries"
				{ "tls-provider=", "Specify the default TLS provider", v => { tls_provider = v; }},
				{ "http-message-handler=", "Specify the default HTTP Message Handler", v => { http_message_provider = v; }},
			};

			IList<string> unprocessed;
			try {
				unprocessed = os.Parse (args);
			}
			catch (MonoMacException) {
				throw;
			}
			catch (Exception e) {
				throw new MonoMacException (10, true, "Could not parse the command line arguments: {0}", e.Message);
			}

			App.RuntimeOptions = RuntimeOptions.Create (http_message_provider, tls_provider);

			ErrorHelper.Verbosity = verbose;

			if (watch_level > 0) {
				watch = new Stopwatch ();
				watch.Start ();
			}

			if (show_help || (args.Length == 0)) {
				ShowHelp (os);
				return;
			} else if (show_version) {
				Console.Write ("mmp {0}.{1}", Constants.Version, Constants.Revision);
				Console.WriteLine ();
				return;
			}

			bool force45From40UnifiedSystemFull = false;

			if (!targetFramework.HasValue)
				targetFramework = TargetFramework.Default;

			if (TargetFramework.Identifier == TargetFramework.Xamarin_Mac_2_0.Identifier) {
				IsUnifiedMobile = true;
			} else {
				foreach (var asm in references) {
					if (asm.EndsWith ("reference/full/Xamarin.Mac.dll")) {
						IsUnifiedFullSystemFramework = true;
						force45From40UnifiedSystemFull = targetFramework == TargetFramework.Net_4_0;
						break;
					}
					if (asm.EndsWith ("mono/4.5/Xamarin.Mac.dll")) {
						IsUnifiedFullXamMacFramework = true;
						break;
					}
				}
			}

			if (IsUnifiedFullXamMacFramework) {
				if (TargetFramework.Identifier != TargetFramework.Net_4_5.Identifier)
					throw new MonoMacException (1405, true, "useFullXamMacFramework must always target framework .NET 4.5, not '{0}' which is invalid.", userTargetFramework);
			}
			if (IsUnifiedFullSystemFramework)
			{
				if (force45From40UnifiedSystemFull) {
					Console.WriteLine ("Xamarin.Mac Unified Full System profile requires .NET 4.5, not .NET 4.0.");
					FixReferences (x => x.Contains ("lib/mono/4.0"), x => x.Replace("lib/mono/4.0", "lib/mono/4.5"));
					targetFramework = TargetFramework.Net_4_5;
				}

			}

			if (IsUnifiedFullSystemFramework || IsClassic) {
				// With newer Mono builds, the system assemblies passed to us by msbuild are
				// no longer safe to copy into the bundle. They are stripped "fake" BCL
				// copies. So we redirect to the "real" ones. Thanks TargetFrameworkDirectories :(
				Regex monoAPIRegex = new Regex("lib/mono/.*-api/", RegexOptions.IgnoreCase);
				Regex monoAPIFacadesRegex = new Regex("lib/mono/.*-api/Facades/", RegexOptions.IgnoreCase);
				FixReferences (x => monoAPIRegex.IsMatch (x) && !monoAPIFacadesRegex.IsMatch (x), x => x.Replace(monoAPIRegex.Match(x).Value, "lib/mono/4.5/"));
			}

			if (targetFramework == TargetFramework.Empty)
				throw new MonoMacException (1404, true, "Target framework '{0}' is invalid.", userTargetFramework);

			// sanity check as this should never happen: we start out by not setting any
			// Unified/Classic properties, and only IsUnifiedMobile if we are are on the
			// XM framework. If we are not, we set IsUnifiedFull to true iff we detect
			// an explicit reference to the full unified Xamarin.Mac assembly; that is
			// only one of IsUnifiedMobile or IsUnifiedFull should ever be true. IsUnified
			// is true if one of IsUnifiedMobile or IsUnifiedFull is true; IsClassic is
			// implied if IsUnified is not true;
			int IsUnifiedCount = IsUnifiedMobile ? 1 : 0;
			if (IsUnifiedFullSystemFramework)
				IsUnifiedCount++;
			if (IsUnifiedFullXamMacFramework)
				IsUnifiedCount++;
			if (IsUnified == IsClassic || (IsUnified && IsUnifiedCount != 1))
				throw new Exception ("IsClassic/IsUnified/IsUnifiedMobile/IsUnifiedFullSystemFramework/IsUnifiedFullXamMacFramework logic regression");

			if ((IsUnifiedFullSystemFramework || IsUnifiedFullXamMacFramework) && (App.LinkMode != LinkMode.None))
				throw new MonoMacException (2007, true,
					"Xamarin.Mac Unified API against a full .NET framework does not support linking. Pass the -nolink flag.");

			if (!IsUnifiedMobile && tls_provider != null)
				throw new MonoMacException (2011, true, "Selecting a TLS Provider is only supported in the Unified Mobile profile");

			Log ("Xamarin.Mac {0}{1}", Constants.Version, verbose > 0 ? "." + Constants.Revision : string.Empty);

			if (verbose > 0)
				Console.WriteLine ("Selected target framework: {0}; API: {1}", targetFramework, IsClassic ? "Classic" : "Unified");

			try {
				Pack (unprocessed);
			} finally {
				if (Cache.IsCacheTemporary) {
					// If we used a temporary directory we created ourselves for the cache
					// (in which case it's more a temporary location where we store the 
					// temporary build products than a cache), it will not be used again,
					// so just delete it.
					try {
						Directory.Delete (Cache.Location, true);
					} catch {
						// Don't care.
					}
				} else {
					// Write the cache data as the last step, so there is no half-done/incomplete (but yet detected as valid) cache.
					Cache.ValidateCache ();
				}
			}

			Log ("bundling complete");
		}

		static void FixReferences (Func<string, bool> match, Func<string, string> fix)
		{
			var assembliesToFix = references.Where (x => match(x)).ToList ();
			references = references.Except (assembliesToFix).ToList ();
			var fixedAssemblies = assembliesToFix.Select (x => fix(x));
			references.AddRange (fixedAssemblies);
		}

		static bool ParseBool (string value, string name)
		{
			if (string.IsNullOrEmpty (value))
				return true;

			switch (value.ToLowerInvariant ()) {
			case "1":
			case "yes":
			case "true":
			case "enable":
				return true;
			case "0":
			case "no":
			case "false":
			case "disable":
				return false;
			default:
				try {
					return bool.Parse (value);
				} catch (Exception ex) {
					throw ErrorHelper.CreateError (26, ex, "Could not parse the command line argument '-{0}:{1}': {2}", name, value, ex.Message);
				}
			}
		}

		static void SetSDKVersion ()
		{
			if (sdk_version != null)
				return;

			if (string.IsNullOrEmpty (DeveloperDirectory))
				return;

			var sdks = new List<Version> ();
			var sdkdir = Path.Combine (DeveloperDirectory, "Platforms", "MacOSX.platform", "Developer", "SDKs");
			foreach (var sdkpath in Directory.GetDirectories (sdkdir)) {
				var sdk = Path.GetFileName (sdkpath);
				if (sdk.StartsWith ("MacOSX") && sdk.EndsWith (".sdk")) {
					Version sdkVersion;
					if (Version.TryParse (sdk.Substring (6, sdk.Length - 10), out sdkVersion))
						sdks.Add (sdkVersion);
				}
			}
			if (sdks.Count > 0) {
				sdks.Sort ();
				// select the highest.
				sdk_version = sdks [sdks.Count - 1];
			}
		}

		public static Frameworks Frameworks { get { return Frameworks.MacFrameworks; } }

		static void CheckForUnknownCommandLineArguments (IList<Exception> exceptions, IList<string> arguments)
		{
			for (int i = arguments.Count - 1; i >= 0; i--) {
				if (arguments [i].StartsWith ("-")) {
					exceptions.Add (ErrorHelper.CreateError (18, "Unknown command line argument: '{0}'", arguments [i]));
					arguments.RemoveAt (i);
				}
			}
		}

		static void Pack (IList<string> unprocessed)
		{
			string fx_dir = null;
			string root_assembly = null;
			var native_libs = new Dictionary<string, List<MethodDefinition>> ();
			HashSet<string> internalSymbols = new HashSet<string> ();

			if (registrar == RegistrarMode.Default)
				registrar = RegistrarMode.Dynamic;

			if (no_executable) {
				if (unprocessed.Count != 0) {
					var exceptions = new List<Exception> ();

					CheckForUnknownCommandLineArguments (exceptions, unprocessed);

					exceptions.Add (new MonoMacException (50, true, "You cannot provide a root assembly if --no-root-assembly is passed, found {0} assemblies: '{1}'", unprocessed.Count, string.Join ("', '", unprocessed.ToArray ())));

					throw new AggregateException (exceptions);
				}

				if (string.IsNullOrEmpty (output_dir))
					throw new MonoMacException (51, true, "An output directory (--output) is required if --no-root-assembly is passed.");

				if (string.IsNullOrEmpty (app_name))
					app_name = Path.GetFileNameWithoutExtension (output_dir);
			} else {
				if (unprocessed.Count != 1) {
					var exceptions = new List<Exception> ();

					CheckForUnknownCommandLineArguments (exceptions, unprocessed);

					if (unprocessed.Count > 1) {
						exceptions.Add (ErrorHelper.CreateError (8, "You should provide one root assembly only, found {0} assemblies: '{1}'", unprocessed.Count, string.Join ("', '", unprocessed.ToArray ())));
					} else if (unprocessed.Count == 0) {
						exceptions.Add (ErrorHelper.CreateError (17, "You should provide a root assembly."));
					}

					throw new AggregateException (exceptions);
				}

				root_assembly = unprocessed [0];
				if (!File.Exists (root_assembly))
					throw new MonoMacException (7, true, "The root assembly '{0}' does not exist", root_assembly);

				string root_wo_ext = Path.GetFileNameWithoutExtension (root_assembly);
				if (Profile.IsSdkAssembly (root_wo_ext) || Profile.IsProductAssembly (root_wo_ext))
					throw new MonoMacException (3, true, "Application name '{0}.exe' conflicts with an SDK or product assembly (.dll) name.", root_wo_ext);

				if (references.Exists (a => Path.GetFileNameWithoutExtension (a).Equals (root_wo_ext)))
					throw new MonoMacException (23, true, "Application name '{0}.exe' conflicts with another user assembly.", root_wo_ext);

				string monoFrameworkDirectory = TargetFramework.MonoFrameworkDirectory;
				if (IsUnifiedFullSystemFramework || IsClassic)
					monoFrameworkDirectory = "4.5";

				fx_dir = Path.Combine (MonoDirectory, "lib", "mono", monoFrameworkDirectory);

				if (!Directory.Exists (fx_dir))
					throw new MonoMacException (1403, true, "{0} {1} could not be found. Target framework '{2}' is unusable to package the application.", "Directory", fx_dir, userTargetFramework);

				references.Add (root_assembly);
				BuildTarget.Resolver.CommandLineAssemblies = references;

				if (string.IsNullOrEmpty (app_name))
					app_name = root_wo_ext;
			
				if (string.IsNullOrEmpty (output_dir))
					output_dir = Environment.CurrentDirectory;
			}

			CreateDirectoriesIfNeeded ();
			Watch ("Setup", 1);

			if (!no_executable) {
				BuildTarget.Resolver.FrameworkDirectory = fx_dir;
				BuildTarget.Resolver.RootDirectory = Path.GetDirectoryName (Path.GetFullPath (root_assembly));
				GatherAssemblies ();
				CheckReferences ();

				if (!resolved_assemblies.Exists (f => Path.GetExtension (f).ToLower () == ".exe"))
					throw new MonoMacException (79, true, "No executable was copied into the app bundle.  Please contact 'support@xamarin.com'", "");

				// i18n must be dealed outside linking too (e.g. bug 11448)
				if (App.LinkMode == LinkMode.None)
					CopyI18nAssemblies (App.I18n);

				CopyAssemblies ();
				Watch ("Copy Assemblies", 1);
			}

			CopyResources ();
			Watch ("Copy Resources", 1);

			CopyConfiguration ();
			Watch ("Copy Configuration", 1);

			ExtractNativeLinkInfo ();

			if (!no_executable) {
				foreach (var nr in native_references) {
					if (!native_libs.ContainsKey (nr))
						native_libs.Add (nr, null);
				}

				// warn if we ask to remove thread checks but the linker is not enabled
				if (App.LinkMode == LinkMode.None && thread_check.HasValue && !thread_check.Value)
					ErrorHelper.Warning (2003, "Option '{0}' will be ignored since linking is disabled", "-disable-thread-check");
				
				var linked_native_libs = Link ();
				foreach (var kvp in linked_native_libs) {
					List<MethodDefinition> methods;
					if (native_libs.TryGetValue (kvp.Key, out methods))
						methods.AddRange (kvp.Value);
					else
						native_libs.Add (kvp.Key, kvp.Value);
				}
				internalSymbols.UnionWith (BuildTarget.LinkContext.RequiredSymbols.Keys);
				Watch (string.Format ("Linking (mode: '{0}')", App.LinkMode), 1);
			}


			CopyDependencies (native_libs);
			Watch ("Copy Dependencies", 1);

			// MDK check
			var ret = Compile (internalSymbols);
			Watch ("Compile", 1);
			if (ret != 0) {
				if (ret == 1)
					throw new MonoMacException (5109, true, "Native linking failed with error code 1.  Check build log for details.");
				if (ret == 69)
					throw new MonoMacException (5308, true, "Xcode license agreement may not have been accepted.  Please launch Xcode.");
				// if not then the compilation really failed
				throw new MonoMacException (5103, true, String.Format ("Failed to compile. Error code - {0}. Please file a bug report at http://bugzilla.xamarin.com", ret));
			}
			
			if (generate_plist)
				GeneratePList ();

			if (App.LinkMode != LinkMode.All && App.RuntimeOptions != null)
				App.RuntimeOptions.Write (App.AppDirectory);

			if (!string.IsNullOrEmpty (certificate_name)) {
				CodeSign ();
				Watch ("Code Sign", 1);
			}
		}

		static void ExtractNativeLinkInfo ()
		{
			var exceptions = new List<Exception> ();

			BuildTarget.ExtractNativeLinkInfo (exceptions);

			if (exceptions.Count > 0)
				throw new AggregateException (exceptions);

			Watch ("Extracted native link info", 1);
		}

		static string FindSystemXcode ()
		{
			var output = new StringBuilder ();
			if (RunCommand ("xcode-select", "-p", output: output) != 0) {
				ErrorHelper.Warning (59, "Could not find the currently selected Xcode on the system: {0}", output.ToString ());
				return null;
			}
			return output.ToString ().Trim ();
		}

		static string DeveloperDirectory {
			get {
				if (sdk_root == null)
					sdk_root = LocateXcode ();

				var plist_path = Path.Combine (Path.GetDirectoryName (sdk_root), "version.plist");
				if (xcode_version == null) {
					if (File.Exists (plist_path)) {
						bool nextElement = false;
						XmlReaderSettings settings = new XmlReaderSettings ();
						settings.DtdProcessing = DtdProcessing.Ignore;
						using (XmlReader reader = XmlReader.Create (plist_path, settings)) {
							while (reader.Read()) {
								// We want the element after CFBundleShortVersionString
								if (reader.NodeType == XmlNodeType.Element) {
									if (reader.Name == "key") {
										if (reader.ReadElementContentAsString() == "CFBundleShortVersionString")
											nextElement = true;
									}
									if (nextElement && reader.Name == "string") {
										nextElement = false;
										xcode_version = new Version (reader.ReadElementContentAsString());
									}
								}
							}
						}
					} else {
						throw ErrorHelper.CreateError (58, "The Xcode.app '{0}' is invalid (the file '{1}' does not exist).", Path.GetDirectoryName (Path.GetDirectoryName (sdk_root)), plist_path);
					}

				} 
				return sdk_root;
			}
		}

		static string LocateXcode ()
		{
			// DEVELOPER_DIR overrides `xcrun` so it should have priority
			string user_developer_directory = Environment.GetEnvironmentVariable ("DEVELOPER_DIR");
			if (!String.IsNullOrEmpty (user_developer_directory))
				return user_developer_directory;

			// Next let's respect xcode-select -p if it exists
			string systemXCodePath = FindSystemXcode ();
			if (!String.IsNullOrEmpty (systemXCodePath)) {
				if (!Directory.Exists (systemXCodePath)) {
					ErrorHelper.Warning (60, "Could not find the currently selected Xcode on the system. 'xcode-select --print-path' returned '{0}', but that directory does not exist.", systemXCodePath);
				}
				else {
					return systemXCodePath;
				}
			}

			// Now the fallback locaions we uses to use (for backwards compat)
			const string Xcode43Default = "/Applications/Xcode.app/Contents/Developer";
			const string XcrunMavericks = "/Library/Developer/CommandLineTools";

			if (Directory.Exists (Xcode43Default))
				return Xcode43Default;

			if (Directory.Exists (XcrunMavericks))
				return XcrunMavericks;

			// And now we give up, but don't throw like mtouch, because we don't want to change behavior (this sometimes worked it appears)
			ErrorHelper.Warning (56, "Cannot find Xcode in any of our default locations. Please install Xcode, or pass a custom path using --sdkroot=<path>.");
			return string.Empty;
		}

		static string MonoDirectory {
			get {
				if (mono_dir == null) {
					if (IsUnifiedFullXamMacFramework || IsUnifiedMobile) {
						mono_dir = GetXamMacPrefix ();
					} else {
						var dir = new StringBuilder ();
						RunCommand (pkg_config, "--variable=prefix mono-2", null, dir);
						mono_dir = Path.GetFullPath (dir.ToString ().Replace (Environment.NewLine, String.Empty));
					}
				}
				return mono_dir;
			}
		}

		static void GeneratePList () {
			var sr = new StreamReader (typeof (Driver).Assembly.GetManifestResourceStream ("Info.plist.tmpl"));
			var all = sr.ReadToEnd ();
			var icon_str = (icon != null) ? "\t<key>CFBundleIconFile</key>\n\t<string>" + icon + "</string>\n\t" : "";

			using (var sw = new StreamWriter (Path.Combine (contents_dir, "Info.plist"))){
				sw.WriteLine (
					all.Replace ("@BUNDLEDISPLAYNAME@", app_name).
					Replace ("@EXECUTABLE@", app_name).
					Replace ("@BUNDLEID@", string.Format ("org.mono.bundler.{0}", app_name)).  
					Replace ("@BUNDLEICON@", icon_str).
					Replace ("@BUNDLENAME@", app_name).
					Replace ("@ASSEMBLY@", references.Where (e => Path.GetExtension (e) == ".exe").FirstOrDefault ()));
			}
		}	


		// the 'codesign' is provided with OSX, not with Xcode (no need to use xcrun)
		// note: by default the monodevelop addin does the signing (not mmp)
		static void CodeSign () {
			RunCommand ("codesign", String.Format ("-v -s \"{0}\" \"{1}\"", certificate_name, App.AppDirectory));
		}
		
		public static string Quote (string f)
		{
			if (f.IndexOf (' ') == -1)
				return f;
			
			var s = new StringBuilder ();
			
			s.Append ('"');
			foreach (var c in f){
				if (c == '\'' || c == '"' || c == '\\')
					s.Append ('\\');
				
				s.Append (c);
			}
			s.Append ('"');
			
			return s.ToString ();
		}

		[DllImport (Constants.libSystemLibrary)]
		static extern int symlink (string path1, string path2);

		[DllImport (Constants.libSystemLibrary)]
		static extern IntPtr realpath (string path, IntPtr buffer);

		static string GetRealPath (string path)
		{
			if (path.StartsWith ("@executable_path/"))
				path = Path.Combine (mmp_dir, path.Substring (17));
			if (path.StartsWith ("@rpath/"))
				path = Path.Combine (mmp_dir, path.Substring (7));

			const int PATHMAX = 1024 + 1;
			IntPtr buffer = IntPtr.Zero;
			try {
				buffer = Marshal.AllocHGlobal (PATHMAX);

				var result = realpath (path, buffer);

				if (result == IntPtr.Zero)
					return path;
				else
					return Marshal.PtrToStringAuto (buffer);
			}
			finally {
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal (buffer);
			}
		}

		[DllImport ("/usr/lib/system/libdyld.dylib")]
		static extern int _NSGetExecutablePath (byte[] buffer, ref uint bufsize);

		static string GetXamMacPrefix ()
		{
			var path = System.Reflection.Assembly.GetExecutingAssembly ().Location;

			var envFrameworkPath = Environment.GetEnvironmentVariable ("XAMMAC_FRAMEWORK_PATH");
			if (!String.IsNullOrEmpty (envFrameworkPath) && Directory.Exists (envFrameworkPath))
				return envFrameworkPath;

			path = GetRealPath (path);
			return Path.GetDirectoryName (Path.GetDirectoryName (Path.GetDirectoryName (path)));
		}

		public static string DriverBinDirectory {
			get {
				return MonoMacBinDirectory;
			}
		}

		public static string MonoMacBinDirectory {
			get {
				return Path.Combine (GetXamMacPrefix (), "bin");
			}
		}

		public static bool IsUptodate (string source, string target)
		{
			return Application.IsUptodate (source, target);
		}

		public static void Log (string format, params object[] args)
		{
			Log (0, format, args);
		}

		public static void Log (int min_verbosity, string format, params object[] args)
		{
			if (min_verbosity > verbose)
				return;

			Console.WriteLine (format, args);
		}

		static string GenerateMain ()
		{
			var sb = new StringBuilder ();
			using (var sw = new StringWriter (sb)) {
				sw.WriteLine ("#include <xamarin/xamarin.h>");
				sw.WriteLine ("#import <AppKit/NSAlert.h>");
				sw.WriteLine ("#import <Foundation/NSDate.h>"); // 10.7 wants this even if not needed on 10.9
				sw.WriteLine ();
				sw.WriteLine ();
				sw.WriteLine ();
				sw.WriteLine ("extern \"C\" int xammac_setup ()");
				sw.WriteLine ("{");
				if (custom_bundle_name != null) {
					sw.WriteLine ("extern NSString* xamarin_custom_bundle_name;");
					sw.WriteLine ("\txamarin_custom_bundle_name = @\"" + custom_bundle_name + "\";");
				}
				sw.WriteLine ("\txamarin_use_il_registrar = {0};", registrar == RegistrarMode.IL ? "true" : "false");
				sw.WriteLine ();
				if (Driver.registrar == RegistrarMode.Static)
					sw.WriteLine ("\txamarin_create_classes ();");

				sw.WriteLine ("\treturn 0;");
				sw.WriteLine ("}");
				sw.WriteLine ();
			}

			return sb.ToString ();
		}

		static void HandleFramework (StringBuilder args, string framework, bool weak)
		{
			string name = Path.GetFileName (framework);
			if (name.Contains ('.'))
				name = name.Remove (name.IndexOf("."));
			string path = Path.GetDirectoryName (framework);
			if (!string.IsNullOrEmpty (path))
				args.Append ("-F ").Append (Quote (path)).Append (' ');
			args.Append (weak ? "-weak_framework " : "-framework ").Append (Quote (name)).Append (' ');

			if (!framework.EndsWith (".framework"))
				return;

			// TODO - There is a chunk of code in mtouch that calls Xamarin.MachO.IsDynamicFramework and doesn't cpoy if framework of static libs
			// But IsDynamicFramework is not on XM yet

			CreateDirectoryIfNeeded (frameworks_dir);
			Application.UpdateDirectory (framework, frameworks_dir);
		}

		static int Compile (IEnumerable<string> internalSymbols)
		{
			int ret = 1;

			string cflags;
			string libdir;
			StringBuilder cflagsb = new StringBuilder ();
			StringBuilder libdirb = new StringBuilder ();
			StringBuilder mono_version = new StringBuilder ();

			string mainSource = GenerateMain ();
			string registrarPath = null;

			SetSDKVersion ();
			if (registrar == RegistrarMode.Static) {
				var code = XamCore.Registrar.StaticRegistrar.Generate (App, BuildTarget.Resolver.ResolverCache.Values, Is64Bit, BuildTarget.LinkContext);
				registrarPath = Path.Combine (Cache.Location, "registrar.m");
				File.WriteAllText (registrarPath, code);

				var platform_assembly = BuildTarget.Resolver.ResolverCache.First ((v) => v.Value.Name.Name == XamCore.Registrar.Registrar.PlatformAssembly).Value;
				Frameworks.Gather (platform_assembly, BuildTarget.Frameworks, BuildTarget.WeakFrameworks);
			}

			try {
				string [] env = null;
				if (IsUnified && !IsUnifiedFullSystemFramework)
					env = new [] { "PKG_CONFIG_PATH", Path.Combine (GetXamMacPrefix (), "lib", "pkgconfig") };

				RunCommand (pkg_config, "--cflags mono-2", env, cflagsb);
				RunCommand (pkg_config, "--variable=libdir mono-2", env, libdirb);
				RunCommand (pkg_config, "--modversion mono-2", env, mono_version);
			} catch (Win32Exception e) {
				throw new MonoMacException (5301, true, e, "pkg-config could not be found. Please install the Mono.framework from http://mono-project.com/Downloads");
			}

			Version mono_ver;
			if (Version.TryParse (mono_version.ToString ().TrimEnd (), out mono_ver) && mono_ver < MinimumMonoVersion)
				throw new MonoMacException (1, true, "This version of Xamarin.Mac requires Mono {0} (the current Mono version is {1}). Please update the Mono.framework from http://mono-project.com/Downloads", 
					MinimumMonoVersion, mono_version.ToString ().TrimEnd ());
			
			cflags = cflagsb.ToString ().Replace (Environment.NewLine, String.Empty);
			libdir = libdirb.ToString ().Replace (Environment.NewLine, String.Empty);

			var libmain = embed_mono ? "libxammac" : "libxammac-system";
			var libxammac = Path.Combine (GetXamMacPrefix (), "lib", libmain + (App.EnableDebug ? "-debug" : "") + ".a");

			if (!File.Exists (libxammac))
				throw new MonoMacException (5203, true, "Can't find {0}, likely because of a corrupted Xamarin.Mac installation. Please reinstall Xamarin.Mac.", libxammac);

			switch (arch) {
			case "i386":
				break;
			case "x86_64":
				if (IsClassic)
					throw new MonoMacException (5204, true, "Invalid architecture. x86_64 is only supported with the mobile profile.");
				break;
			default:
				throw new MonoMacException (5205, true, "Invalid architecture '{0}'. Valid architectures are i386 and x86_64 (when --profile=mobile).", arch);
			}

			if (IsUnified && !arch_set)
				arch = "x86_64";

			try {
				var args = new StringBuilder ();
				if (App.EnableDebug)
					args.Append ("-g ");
				args.Append ("-mmacosx-version-min=").Append (minos.ToString ()).Append (' ');
				args.Append ("-arch ").Append (arch).Append (' ');
				foreach (var assembly in BuildTarget.Assemblies) {
					if (assembly.LinkWith != null) {
						foreach (var linkWith in assembly.LinkWith) {
							if (verbose > 1)
								Console.WriteLine ("Found LinkWith on {0} for {1}", assembly.FileName, linkWith);
							if (linkWith.EndsWith (".dylib")) {
								// Link against the version copied into MonoBudle, since we install_name_tool'd it already
								string libName = Path.GetFileName (linkWith);
								string finalLibPath = Path.Combine (mmp_dir, libName);
								args.Append (Quote (finalLibPath)).Append (' ');
							}
							else {
								args.Append (Quote (linkWith)).Append (' ');
							}
						}
						args.Append ("-ObjC").Append (' ');
					}
					if (assembly.LinkerFlags != null)
						foreach (var linkFlag in assembly.LinkerFlags)
							args.Append (linkFlag).Append (' ');
					if (assembly.Frameworks != null)
						foreach (var f in assembly.Frameworks)
							HandleFramework (args, f, false);
					if (assembly.WeakFrameworks != null)
						foreach (var f in assembly.WeakFrameworks)
							HandleFramework (args, f, true);
				}

				foreach (var framework in App.Frameworks)
					HandleFramework (args, framework, false);

				foreach (var lib in native_libraries_copied_in)
				{
					// Link against the version copied into MonoBudle, since we install_name_tool'd it already
					string libName = Path.GetFileName (lib);
					string finalLibPath = Path.Combine (mmp_dir, libName);
					args.Append (Quote (finalLibPath)).Append (' ');
				}

				foreach (var f in BuildTarget.Frameworks)
					args.Append ("-framework ").Append (f).Append (' ');
				foreach (var f in BuildTarget.WeakFrameworks)
					args.Append ("-weak_framework ").Append (f).Append (' ');
				Driver.WriteIfDifferent (Path.Combine (Cache.Location, "exported-symbols-list"), string.Join ("\n", internalSymbols.Select ((symbol) => "_" + symbol).ToArray ()));
				foreach (var symbol in internalSymbols)
					args.Append ("-u _").Append (symbol).Append (' ');

				bool linkWithRequiresForceLoad = BuildTarget.Assemblies.Any (x => x.ForceLoad);
				if (no_executable || linkWithRequiresForceLoad)
					args.Append ("-force_load "); // make sure nothing is stripped away if we don't have a root assembly, since anything can end up being used.
				args.Append (Quote (libxammac)).Append (' ');
				args.Append ("-o ").Append (Quote (Path.Combine (macos_dir, app_name))).Append (' ');
				args.Append (cflags).Append (' ');
				if (embed_mono) {
					var libmono = "libmonosgen-2.0.a";
					var lib = Path.Combine (libdir, libmono);

					if (!File.Exists (Path.Combine (lib)))
						throw new MonoMacException (5202, true, "Mono.framework MDK is missing. Please install the MDK for your Mono.framework version from http://mono-project.com/Downloads");

					args.Append (Quote (lib)).Append (' ');

					if (profiling.HasValue && profiling.Value) {
						args.Append (Quote (Path.Combine (libdir, "libmono-profiler-log.a"))).Append (' ');
						args.Append ("-u _mono_profiler_startup_log -lz ");
					}
				}
				args.Append ("-framework AppKit -liconv -x objective-c++ ");
				args.Append ("-I").Append (Quote (Path.Combine (GetXamMacPrefix (), "include"))).Append (' ');
				if (registrarPath != null)
					args.Append (registrarPath).Append (' ');
				args.Append ("-fno-caret-diagnostics -fno-diagnostics-fixit-info ");
				if (link_flags != null)
					args.Append (link_flags + " ");
				if (!string.IsNullOrEmpty (DeveloperDirectory))
					args.Append ("-isysroot ").Append (Quote (Path.Combine (DeveloperDirectory, "Platforms", "MacOSX.platform", "Developer", "SDKs", "MacOSX" + sdk_version + ".sdk"))).Append (' ');

				var main = Path.Combine (Cache.Location, "main.m");
				File.WriteAllText (main, mainSource);
				args.Append (Quote (main));

				ret = XcodeRun ("clang", args.ToString (), null);
			} catch (Win32Exception e) {
				throw new MonoMacException (5103, true, e, "Failed to compile the file '{0}'. Please file a bug report at http://bugzilla.xamarin.com", "driver");
			}
			
			return ret;
		}

		static int XcodeRun (string command, string args, StringBuilder output = null)
		{
			string [] env = DeveloperDirectory != string.Empty ? new string [] { "DEVELOPER_DIR", DeveloperDirectory } : null;
			int ret = RunCommand ("xcrun", String.Concat ("-sdk macosx ", command, " ", args), env, output);
			if (ret != 0 && verbose > 1) {
				StringBuilder debug = new StringBuilder ();
				RunCommand ("xcrun", String.Concat ("--find ", command), env, debug);
				Console.WriteLine ("failed using `{0}` from: {1}", command, debug);
			}
			return ret;
		}

		// check that we have a reference to XamMac.dll and not to MonoMac.dll. Check various DRM license checks
		static void CheckReferences ()
		{
			List<Exception> exceptions = new List<Exception> ();
			var cache = BuildTarget.Resolver.ToResolverCache ();
			var incompatibleReferences = new List<string> ();
			var haveValidReference = false;

			foreach (string entry in cache.Keys) {
				switch (entry) {
				case "Xamarin.Mac":
					if (IsUnified)
						haveValidReference = true;
					else
						incompatibleReferences.Add (entry);
					break;
				case "XamMac":
					if (IsClassic)
						haveValidReference = true;
					else
						incompatibleReferences.Add (entry);
					break;
				case "MonoMac":
					incompatibleReferences.Add (entry);
					break;
				}
			}

			if (!haveValidReference)
				exceptions.Add (new MonoMacException (1401, true,
					"The required '{0}' assembly is missing from the references",
					IsUnified ? "Xamarin.Mac.dll" : "XamMac.dll"));

			foreach (var refName in incompatibleReferences)
				exceptions.Add (new MonoMacException (1402, true,
					"The assembly '{0}' is not compatible with this tool or profile",
					refName + ".dll"));

			if (exceptions.Count > 0)
				throw new AggregateException (exceptions);
		}

		static IDictionary<string,List<MethodDefinition>> Link ()
		{
			var cache = BuildTarget.Resolver.ToResolverCache ();
			var resolver = cache != null
				? new Mono.Linker.AssemblyResolver (cache)
				: new Mono.Linker.AssemblyResolver ();

			resolver.AddSearchDirectory (BuildTarget.Resolver.RootDirectory);
			resolver.AddSearchDirectory (BuildTarget.Resolver.FrameworkDirectory);

			var options = new LinkerOptions {
				MainAssembly = BuildTarget.Resolver.GetAssembly (references [references.Count - 1]),
				OutputDirectory = mmp_dir,
				LinkSymbols = App.EnableDebug,
				LinkMode = App.LinkMode,
				Resolver = resolver,
				SkippedAssemblies = App.LinkSkipped,
				I18nAssemblies = App.I18n,
				ExtraDefinitions = App.Definitions,
				TargetFramework = TargetFramework,
				Architecture = arch,
				RuntimeOptions = App.RuntimeOptions,
				// by default we keep the code to ensure we're executing on the UI thread (for UI code) for debug builds
				// but this can be overridden to either (a) remove it from debug builds or (b) keep it in release builds
				EnsureUIThread = thread_check.HasValue ? thread_check.Value : App.EnableDebug,
			};

			Mono.Linker.LinkContext context;
			MonoMac.Tuner.Linker.Process (options, out context, out resolved_assemblies);
			BuildTarget.LinkContext = (context as MonoMacLinkContext);
			return BuildTarget.LinkContext.PInvokeModules;
		}

		static void ProcessDllImports (Dictionary<string, List<MethodDefinition>> pinvoke_modules, HashSet<string> internalSymbols)
		{
			foreach (string assembly_name in resolved_assemblies) {
				AssemblyDefinition assembly = BuildTarget.Resolver.GetAssembly (assembly_name);
				foreach (ModuleDefinition md in assembly.Modules) {
					if (md.HasTypes) {
						foreach (TypeDefinition type in md.Types) {
							if (type.HasMethods) {
								foreach (MethodDefinition method in type.Methods) {
									if ((method != null) && !method.HasBody && method.IsPInvokeImpl) {
										// this happens for c++ assemblies (ref #11448)
										if (method.PInvokeInfo == null)
											continue;
										string module = method.PInvokeInfo.Module.Name;

										if (!String.IsNullOrEmpty (module)) {
											List<MethodDefinition> methods;
											if (!pinvoke_modules.TryGetValue (module, out methods))
												pinvoke_modules.Add (module, methods = new List<MethodDefinition> ());
											methods.Add (method);
										}
										if (module == "__Internal")
											internalSymbols.Add (method.PInvokeInfo.EntryPoint);
									}
								}
							}
						}
					}
				}
			}
		}

		static void CopyDependencies (IDictionary<string, List<MethodDefinition>> libraries)
		{
			// Process LinkWith first so we don't have unnecessary warnings
			foreach (var assembly in BuildTarget.Assemblies.Where (a => a.LinkWith != null)) {
				foreach (var linkWith in assembly.LinkWith.Where (l => l.EndsWith (".dylib"))) {
					string libName = Path.GetFileName (linkWith);
					string finalLibPath = Path.Combine (mmp_dir, libName);
					Application.UpdateFile (linkWith, finalLibPath);
					XcodeRun ("install_name_tool -id", string.Format ("{0} {1}", Quote("@executable_path/../" + BundleName + "/" + libName), finalLibPath));
					native_libraries_copied_in.Add (libName);
				}
			}

			var processed = new HashSet<string> ();
			foreach (var kvp in libraries.Where (l => !native_libraries_copied_in.Contains (Path.GetFileName (l.Key))))
				ProcessNativeLibrary (processed, kvp.Key, kvp.Value);

			// .dylibs might refers to specific paths (e.g. inside Mono framework directory) and/or
			// refer to a symlink (pointing to a more specific version of the library)
			StringBuilder sb = new StringBuilder ();
			foreach (string library in Directory.GetFiles (mmp_dir, "*.dylib")) {
				foreach (string lib in Xamarin.MachO.GetNativeDependencies (library)) {
					if (lib.StartsWith ("/Library/Frameworks/Mono.framework/Versions/", StringComparison.Ordinal)) {
						string libname = Path.GetFileName (lib);
						string real_lib = GetRealPath (lib);
						string real_libname	= Path.GetFileName (real_lib);
						// if a symlink was specified then re-create it inside the .app
						if (libname != real_libname)
							CreateSymLink (mmp_dir, real_libname, libname);
						sb.Append (" -change ").Append (lib).Append (" @executable_path/../" + BundleName + "/").Append (libname);
					}
				}
				// if required update the paths inside the .dylib that was copied
				if (sb.Length > 0) {
					sb.Append (' ').Append (Quote (library));
					XcodeRun ("install_name_tool", sb.ToString ());
					sb.Clear ();
				}
			}
		}

		static string GetLibraryShortenedName (string name)
		{
			// GetFileNameWithoutExtension only removes one extension, e.g. libx.so.2 won't work
			int start = name.StartsWith ("lib", StringComparison.Ordinal) ? 3 : 0;
			int end = name.Length;
			if (name.EndsWith (".dylib", StringComparison.Ordinal))
				end -= 6;
			else if (name.EndsWith (".so", StringComparison.Ordinal))
				end -= 3;
			else if (name.EndsWith (".dll", StringComparison.OrdinalIgnoreCase))
				end -= 4;
			return name.Substring (start, end - start);
		}

		static bool ShouldSkipNativeLibrary (string fileName)
		{
			string shortendName = GetLibraryShortenedName (fileName);

			// well known libraries we do not bundle or warn about
			switch (shortendName.ToLowerInvariant ()) {
			case "xammac":	// we have a p/invoke to this library in Runtime.mac.cs, for users that don't bundle with mmp.
			case "__internal":	// mono runtime
			case "kernel32":	// windows specific
			case "gdi32":		// windows specific
			case "ole32":		// windows specific
			case "user32":		// windows specific
			case "advapi32":	// windows specific
			case "crypt32":		// windows specific
			case "msvcrt":		// windows specific
			case "iphlpapi":	// windows specific
			case "winmm":		// windows specific
			case "winspool":	// windows specific
			case "c":		// system provided
			case "objc":		// system provided
			case "system":		// system provided, libSystem.dylib -> CommonCrypto
			case "x11":		// msvcrt pulled in
			case "winspool.drv":	// msvcrt pulled in
			case "cups":		// msvcrt pulled in
			case "fam.so.0":	// msvcrt pulled in
			case "gamin-1.so.0":	// msvcrt pulled in
			case "asound.so.2":	// msvcrt pulled in
			case "oleaut32": // referenced by System.Runtime.InteropServices.Marshal._[S|G]etErrorInfo
				return true;
			}
			// Shutup the warning until we decide on bug: 36478
			if (shortendName.ToLowerInvariant () == "intl" && IsUnifiedFullXamMacFramework)
				return true;
			return false;
		}

		static void ProcessNativeLibrary (HashSet<string> processed, string library, List<MethodDefinition> used_by_methods)
		{
			// We do not bundle system libraries, ever
			if (library.StartsWith ("/usr/lib/", StringComparison.Ordinal) || library.StartsWith ("/System/Library/", StringComparison.Ordinal))
				return;

			// If we've been required to ignore this library, skip it
			if (ignored_assemblies.Contains (library))
				return;

			// If we're as passed in framework, ignore
			if (App.Frameworks.Contains (library))
				return;

			// We need to check both the name and the shortened name, since we might get passed:
			// full path - /foo/bar/libFoo.dylib
			// just name - libFoo
			string name = Path.GetFileName (library);
			string libName = "lib" + GetLibraryShortenedName (name) + ".dylib";

			// There is a list of libraries we always skip, honor that
			if (ShouldSkipNativeLibrary (name))
				return;

			string src = null;
			// If we've been passed in a full path and it is there, let's just go with that
			if (File.Exists (library))
				src = library;

			// Now let's check inside mono/lib
			string monoDirPath = Path.Combine (MonoDirectory, "lib", libName);
			if (src == null && File.Exists (monoDirPath))
				src = monoDirPath;

			// Now let's check in path with our libName
			string path = Path.GetDirectoryName (library);
			if (src == null && !String.IsNullOrEmpty (path)) {
				string pathWithLibName = Path.Combine (path, name);
				if (File.Exists (pathWithLibName))
					src = pathWithLibName;
			}

			// If we can't find it at this point, scream
			if (src == null) {
				ErrorHelper.Show (new MonoMacException (2006, false, "Native library '{0}' was referenced but could not be found.", name));
				if (used_by_methods != null && used_by_methods.Count > 0) {
					const int referencedByLimit = 25;
					bool limitReferencedByWarnings = used_by_methods.Count > referencedByLimit && verbose < 4;
					foreach (var m in limitReferencedByWarnings ? used_by_methods.Take (referencedByLimit) : used_by_methods) {
						ErrorHelper.Warning (2009, "  Referenced by {0}.{1}", m.DeclaringType.FullName, m.Name);
					}
					if (limitReferencedByWarnings)
						ErrorHelper.Warning (2012, " Only first {0} of {1} \"Referenced by\" warnings shown.", referencedByLimit, used_by_methods.Count);
				}
				return;
			}
			string real_src = GetRealPath (src);

			string dest = Path.Combine (mmp_dir, Path.GetFileName (real_src));
			if (verbose > 1)
				Console.WriteLine ("Native library '{0}' copied to application bundle.", Path.GetFileName (real_src));

			// FIXME: should we strip extra architectures (e.g. x64) ? 
			// that could break the library signature and cause issues on the appstore :(
			if (GetRealPath (dest) == real_src) {
				Console.WriteLine ("Dependency {0} was already at destination, skipping.", Path.GetFileName (real_src));
			}
			else {
				File.Copy (real_src, dest, true);
			}

			bool isStaticLib = real_src.EndsWith (".a");
			if (native_references.Contains (real_src)) {
				if (!isStaticLib)
					XcodeRun ("install_name_tool -id", string.Format ("{0} {1}", Quote("@executable_path/../" + BundleName + "/" + name), dest));
				native_libraries_copied_in.Add (name);
			}

			// if a symlink was used then it might still be needed at runtime
			if (src != real_src)
				CreateSymLink (mmp_dir, real_src, src);

			// We add both the resolved location and the initial request.
			// @executable_path subtitution and other resolving can have these be different
			// and we'll loop forever otherwise
			processed.Add (real_src);
			processed.Add (library);

			// process native dependencies
			if (!isStaticLib) {
				foreach (string dependency in Xamarin.MachO.GetNativeDependencies (real_src)) {
					string lib = GetRealPath (dependency);
					if (!processed.Contains (lib))
						ProcessNativeLibrary (processed, lib, null);
				}
			}
		}

		static void CreateSymLink (string directory, string real, string link)
		{
			string cd = Environment.CurrentDirectory;
			Environment.CurrentDirectory = directory;
			symlink (Path.GetFileName (real), "./" + Path.GetFileName (link));
			Environment.CurrentDirectory = cd;
		}

		/* Currently we clobber any existing files, perhaps we should error and have a -force flag */
		static void CreateDirectoriesIfNeeded () {
			App.AppDirectory = Path.Combine (output_dir, string.Format ("{0}.app", app_name));
			contents_dir = Path.Combine (App.AppDirectory, "Contents");
			macos_dir = Path.Combine (contents_dir, "MacOS");
			frameworks_dir = Path.Combine (contents_dir, "Frameworks");
			resources_dir = Path.Combine (contents_dir, "Resources");
			mmp_dir = Path.Combine (contents_dir, BundleName);

			CreateDirectoryIfNeeded (App.AppDirectory);
			CreateDirectoryIfNeeded (contents_dir);
			CreateDirectoryIfNeeded (macos_dir);
			CreateDirectoryIfNeeded (resources_dir);
			CreateDirectoryIfNeeded (mmp_dir);
		}

		static void CreateDirectoryIfNeeded (string dir) {
			if (!Directory.Exists (dir))
				Directory.CreateDirectory (dir);
		}

		static void CopyConfiguration () {
			if (IsUnifiedMobile) {
				CopyResourceFile ("config_mobile", "config");
			}
			else {
				if (IsUnifiedFullXamMacFramework) {
					CopyResourceFile ("machine.4_5.config", "machine.config");
				}
				else {
					string machine_config = Path.Combine (MonoDirectory, "etc", "mono", "4.5", "machine.config");

					if (!File.Exists (machine_config))
						throw new MonoMacException (1403, true, "{0} '{1}' could not be found. Target framework {2} is unusable to package the application.", "File", machine_config, userTargetFramework);

					File.Copy (machine_config, Path.Combine (mmp_dir, "machine.config"), true);
				}

				CopyResourceFile ("config", "config");
			}
		}

		static void CopyResourceFile (string streamName, string outputFileName) {
			var sr = new StreamReader (typeof (Driver).Assembly.GetManifestResourceStream (streamName));
			var all = sr.ReadToEnd ();
			var config = Path.Combine (mmp_dir, outputFileName);
			using (var sw = new StreamWriter (config)) {
				sw.WriteLine (all);
			}
		}

		static void CopyI18nAssemblies (I18nAssemblies i18n)
		{
			if (i18n == I18nAssemblies.None)
				return;

			string fx_dir = BuildTarget.Resolver.FrameworkDirectory;
			// always needed (if any is specified)
			resolved_assemblies.Add (Path.Combine (fx_dir, "I18N.dll"));
			// optionally needed
			if ((i18n & I18nAssemblies.CJK) != 0)
				resolved_assemblies.Add (Path.Combine (fx_dir, "I18N.CJK.dll"));
			if ((i18n & I18nAssemblies.MidEast) != 0)
				resolved_assemblies.Add (Path.Combine (fx_dir, "I18N.MidEast.dll"));
			if ((i18n & I18nAssemblies.Other) != 0)
				resolved_assemblies.Add (Path.Combine (fx_dir, "I18N.Other.dll"));
			if ((i18n & I18nAssemblies.Rare) != 0)
				resolved_assemblies.Add (Path.Combine (fx_dir, "I18N.Rare.dll"));
			if ((i18n & I18nAssemblies.West) != 0)
				resolved_assemblies.Add (Path.Combine (fx_dir, "I18N.West.dll"));
		}

		static void CopyAssemblies () {
			foreach (string asm in resolved_assemblies) {
				var mdbfile = string.Format ("{0}.mdb", asm);
				var configfile = string.Format ("{0}.config", asm);
				string filename = Path.GetFileName (asm);

				File.Copy (asm, Path.Combine (mmp_dir, filename), true);
				if (verbose > 0)
					Console.WriteLine ("Added assembly {0}", asm);

				if (App.EnableDebug && File.Exists (mdbfile))
					File.Copy (mdbfile, Path.Combine (mmp_dir, Path.GetFileName (mdbfile)), true);
				if (File.Exists (configfile))
					File.Copy (configfile, Path.Combine (mmp_dir, Path.GetFileName (configfile)), true);
			}
		}

		static void CopyResources () {
			foreach (string res in resources) {
				File.Copy (res, Path.Combine (resources_dir, Path.GetFileName (res)), true);
			}
		}

		static void GatherAssemblies () {
			foreach (string asm in references) {
				var assembly = BuildTarget.Resolver.AddAssembly (SwapOutReferenceAssembly (asm));
				if (assembly == null)
					ErrorHelper.Warning (1501, "Can not resolve reference: {0}", asm);
				ProcessAssemblyReferences (assembly);
			}
			if (BuildTarget.Resolver.Exceptions.Count > 0)
				throw new AggregateException (BuildTarget.Resolver.Exceptions);
		}

		static void ProcessAssemblyReferences (AssemblyDefinition assembly) {
			if (assembly == null)
				return;

			var fqname = GetRealPath (assembly.MainModule.FullyQualifiedName);

			if (resolved_assemblies.Contains (fqname))
				return;

			BuildTarget.Assemblies.Add (new Assembly (BuildTarget, assembly));

			resolved_assemblies.Add (fqname);

			foreach (AssemblyNameReference reference in assembly.MainModule.AssemblyReferences) {
				var reference_assembly = BuildTarget.Resolver.Resolve (SwapOutReferenceAssembly (reference.FullName));
				if (verbose > 1)
					Console.WriteLine ("Adding dependency {0} required by {1}", reference.Name, assembly.MainModule.Name);
				ProcessAssemblyReferences (reference_assembly);
			}
		}

		static string SwapOutReferenceAssembly (string assembly)
		{
			// Inject the correct Xamarin.Mac.dll - the one in the framework
			// directory is a reference assembly only (stripped of IL, containing
			// only API/metadata) and the correct one based on the target
			// architecture needs to replace it
			string fileName = Path.GetFileName (assembly);

			if (assembly.Contains ("OpenTK.dll") && IsUnifiedFullXamMacFramework)
				return assembly;
			if (IsUnified &&
				xammac_reference_assemblies.Contains (fileName)) {
				switch (arch) {
				case "i386":
				case "x86_64":
					return Path.Combine (GetXamMacPrefix (), "lib", arch, (IsUnifiedFullSystemFramework || IsUnifiedFullXamMacFramework) ? "full" : "mobile", fileName);
				default:
					throw new MonoMacException (5205, true, "Invalid architecture '{0}'. " +
						"Valid architectures are i386 and x86_64 (when --profile=mobile).", arch);
				}
			}
			return assembly;
		}
	}
}
