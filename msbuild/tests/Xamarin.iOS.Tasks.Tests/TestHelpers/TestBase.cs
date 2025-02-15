using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Utilities;
using NUnit.Framework;
using Xamarin.MacDev;

namespace Xamarin.iOS.Tasks
{
	public abstract class TestBase
	{
		protected static class TargetName
		{
			public static string Build = "Build";
			public static string Clean = "Clean";
			public static string CollectBundleResources = "_CollectBundleResources";
			public static string CompileImageAssets = "_CompileImageAssets";
			public static string CompileInterfaceDefinitions = "_CompileInterfaceDefinitions";
			public static string CopyResourcesToBundle = "_CopyResourcesToBundle";
			public static string DetectAppManifest = "_DetectAppManifest";
			public static string GenerateBundleName = "_GenerateBundleName";
			public static string PackLibraryResources = "_PackLibraryResources";
			public static string ResolveReferences = "ResolveReferences";
		}

		public string [] ExpectedAppFiles = { };
		public string [] UnexpectedAppFiles = { "monotouch.dll" };

		public string[] CoreAppFiles {
			get {
				var xi = new string [] {
					"Xamarin.iOS.dll",
					"Xamarin.iOS.dll.mdb",
					"mscorlib.dll",
					"mscorlib.dll.mdb",
				};
				var xw = new string [] {
					"Xamarin.WatchOS.dll",
					"Xamarin.WatchOS.dll.mdb",
					"mscorlib.dll",
					"mscorlib.dll.mdb",
				};
				var xt = new string [] {
					"Xamarin.TVOS.dll",
					"Xamarin.TVOS.dll.mdb",
					"mscorlib.dll",
					"mscorlib.dll.mdb",
				};

				if (TargetFrameworkIdentifier == "Xamarin.WatchOS") {
					return xw;
				} else if (TargetFrameworkIdentifier == "Xamarin.TVOS") {
					return xt;
				} else {
					return xi;
				}
			}
		}

		public Logger Logger {
			get; set;
		}

		public TestEngine Engine {
			get; private set;
		}


		public Project LibraryProject {
			get; private set;
		}

		public Project MonoTouchProject {
			get; private set;
		}

		public string LibraryProjectBinPath;
		public string LibraryProjectObjPath;
		public string LibraryProjectPath;
		public string LibraryProjectCSProjPath;

		public string MonoTouchProjectBinPath;
		public string MonoTouchProjectObjPath;
		public string MonoTouchProjectPath;
		public string MonoTouchProjectCSProjPath;
		public string AppBundlePath;

		public string TempDir {
			get; set;
		}

		public ProjectPaths SetupProjectPaths (string projectName, string baseDir = "../", bool includePlatform = true, string platform = "iPhoneSimulator", string config = "Debug")
		{
			var projectPath = Path.Combine(baseDir, projectName);

			var binPath = includePlatform ? Path.Combine (projectPath, "bin", platform, config) : Path.Combine (projectPath, "bin", config);
			var objPath = includePlatform ? Path.Combine (projectPath, "obj", platform, config) : Path.Combine (projectPath, "obj", config);

			return new ProjectPaths {
				ProjectPath = projectPath,
				ProjectBinPath = binPath,
				ProjectObjPath = objPath,
				ProjectCSProjPath = Path.Combine (projectPath, projectName + ".csproj"),
				AppBundlePath = Path.Combine (binPath, projectName + ".app"),
			};
		}

		[SetUp]
		public virtual void Setup ()
		{
			var mtouchPaths = SetupProjectPaths ("MySingleView");

			MonoTouchProjectBinPath = mtouchPaths ["project_binpath"];
			MonoTouchProjectObjPath = mtouchPaths ["project_objpath"];
			MonoTouchProjectCSProjPath = mtouchPaths ["project_csprojpath"];
			MonoTouchProjectPath = mtouchPaths ["project_path"];

			AppBundlePath = mtouchPaths ["app_bundlepath"];

			var libraryPaths = SetupProjectPaths ("MyLibrary", "../MySingleView/", false);

			LibraryProjectBinPath = libraryPaths ["project_binpath"];
			LibraryProjectObjPath = libraryPaths ["project_objpath"];
			LibraryProjectPath = libraryPaths ["project_path"];
			LibraryProjectCSProjPath = libraryPaths ["project_csprojpath"];

			SetupEngine ();

			MonoTouchProject = SetupProject (Engine, MonoTouchProjectCSProjPath);
			LibraryProject = SetupProject (Engine, LibraryProjectCSProjPath);

			CleanUp ();
		}

		public void SetupEngine () 
		{
			Engine = new TestEngine ();
		}

		public Project SetupProject (Engine engine, string projectPath) 
		{
			var proj = new Project (engine);
			proj.Load (projectPath);

			return proj;
		}

		public virtual string TargetFrameworkIdentifier {
			get {
				return "Xamarin.iOS";
			}
		}

		public bool IsWatchOS {
			get { return TargetFrameworkIdentifier == "Xamarin.WatchOS"; }
		}

		public bool IsTVOS {
			get { return TargetFrameworkIdentifier == "Xamarin.TVOS"; }
		}

		public void CleanUp () {

			var paths = SetupProjectPaths ("MySingleView");
			MonoTouchProjectPath = paths ["project_path"];

			TempDir = Path.GetFullPath ("ScratchDir");
			SafeDelete (TempDir);
			Directory.CreateDirectory (TempDir);

			// Ensure the bin and obj directories are cleared
			SafeDelete (Path.Combine (MonoTouchProjectPath, "bin"));
			SafeDelete (Path.Combine (MonoTouchProjectPath, "obj"));

			SafeDelete (Path.Combine (LibraryProjectPath, "bin"));
			SafeDelete (Path.Combine (LibraryProjectPath, "obj"));

			// Reset all the write times as we deliberately set some in the future for our tests
			foreach (var file in Directory.GetFiles (MonoTouchProjectPath, "*.*", SearchOption.AllDirectories))
				File.SetLastWriteTime (file, DateTime.Now);
			foreach (var file in Directory.GetFiles (LibraryProjectPath, "*.*", SearchOption.AllDirectories))
				File.SetLastWriteTime (file, DateTime.Now);
		}

		protected void SafeDelete (string path)
		{
			try {
				if (Directory.Exists (path))
					Directory.Delete (path, true);
				else if (File.Exists (path))
					File.Delete (path);
			} catch {

			}
		}

		public void TestFilesDoNotExist(string baseDir, IEnumerable<string> files)
		{
			foreach (var v in files.Select (s => Path.Combine (baseDir, s)))
				Assert.IsFalse (File.Exists (v) || Directory.Exists (v), "Unexpected file: {0} exists", v);
		}

		public void TestFilesExists (string baseDir, string[] files)
		{
			foreach (var v in files.Select (s => Path.Combine (baseDir, s)))
				Assert.IsTrue (File.Exists (v) || Directory.Exists (v), "Expected file: {0} does not exist", v);
		}

		public void TestStoryboardC (string path) 
		{
			Assert.IsTrue (Directory.Exists (path), "Storyboard {0} does not exist", path);
			Assert.IsTrue (File.Exists (Path.Combine (path, "Info.plist")));
			TestPList (path, new string [] {"CFBundleVersion", "CFBundleExecutable"});
		}

		public void TestPList (string path, string[] keys)
		{
			var plist = PDictionary.FromFile (Path.Combine (path, "Info.plist"));
			foreach (var x in keys) {
				Assert.IsTrue (plist.ContainsKey (x), "Key {0} is not present in {1} Info.plist", x, path);
				Assert.IsNotEmpty (((PString)plist[x]).Value, "Key {0} is empty in {1} Info.plist", x, path);
			}
		}

		[TearDown]
		public virtual void Teardown ()
		{
			SafeDelete (TempDir);
		}

		public T CreateTask<T> () where T : Task, new()
		{
			var t = new T ();
			t.BuildEngine = Engine;
			return t;
		}

		protected string CreateTempFile (string path)
		{
			path = Path.Combine (TempDir, path);
			Directory.CreateDirectory (Path.GetDirectoryName (path));
			using (new FileStream (path, FileMode.CreateNew));
			return path;
		}

		protected DateTime GetLastModified (string file)
		{
			if (Path.GetExtension (file) == ".nib" && !File.Exists (file))
				file = Path.Combine (file, "runtime.nib");

			if (!File.Exists (file))
				Assert.Fail ("Expected file '{0}' did not exist", file);

			return File.GetLastWriteTime (file);
		}

		protected void RemoveItemsByName (Project project, string itemName)
		{
			foreach (var item in project.GetEvaluatedItemsByName (itemName).ToArray ())
				project.RemoveItem (item);
		}

		protected string SetPListKey (string key, PObject value)
		{

			var paths = SetupProjectPaths ("MySingleView");

			var plist = PDictionary.FromFile (Path.Combine (paths ["project_path"], "Info.plist"));
			if (value == null)
				plist.Remove (key);
			else
				plist [key] = value;

			var modifiedPListPath = Path.Combine (TempDir, "modified.plist");
			plist.Save (modifiedPListPath);
			return modifiedPListPath;
		}

		protected void Touch (string file)
		{
			if (!File.Exists (file))
				Assert.Fail ("Expected file '{0}' did not exist", file);
			File.SetLastWriteTime (file, DateTime.Now.AddDays (1));
			System.Threading.Thread.Sleep (1000);
		}

		public void RunTarget (Project project, string target, int expectedErrorCount = 0)
		{
			Engine.BuildProject (project, new [] { target }, new Hashtable { {"Platform", "iPhone"} }, BuildSettings.None);
			Assert.AreEqual (expectedErrorCount, Engine.Logger.ErrorEvents.Count, "#RunTarget-ErrorCount");
		}

		public void RunTarget_WithErrors (Project project, string target)
		{
			Engine.BuildProject (project, new [] { target }, new Hashtable (), BuildSettings.None);
			Assert.IsTrue (Engine.Logger.ErrorEvents.Count > 0, "#RunTarget-HasExpectedErrors");
		}
	}

	public class ProjectPaths : Dictionary<string, string> {
		public string ProjectPath { get { return this ["project_path"]; } set { this ["project_path"] = value; } }
		public string ProjectBinPath { get { return this ["project_binpath"]; } set { this ["project_binpath"] = value; } }
		public string ProjectObjPath { get { return this ["project_objpath"]; } set { this ["project_objpath"] = value; } }
		public string ProjectCSProjPath { get { return this ["project_csprojpath"]; } set { this ["project_csprojpath"] = value; } }
		public string AppBundlePath { get { return this ["app_bundlepath"]; } set { this ["app_bundlepath"] = value; } }
	}
}
