﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xamarin.MacDev.Tasks
{
	public abstract class PrepareNativeReferencesTaskBase : Task
	{
		public string SessionId { get; set; }

		[Required]
		public string IntermediateOutputPath { get; set; }

		public ITaskItem[] NativeReferences { get; set; }

		#region Output

		[Output]
		public ITaskItem LinkWithAttributes { get; set; }

		[Output]
		public ITaskItem[] EmbeddedResources { get; set; }

		[Output]
		public ITaskItem[] NativeFrameworks { get; set; }

		#endregion

		void AppendLinkTargetProperty (StringBuilder builder, ITaskItem item)
		{
			LinkTarget target;
			string value;

			value = item.GetMetadata ("LinkTarget");
			if (string.IsNullOrEmpty (value))
				return;

			if (!Enum.TryParse (value, out target)) {
				Log.LogError (null, null, null, item.ItemSpec, 0, 0, 0, 0, "Unknown link target: {0}", value);
				return;
			}

			var targets = target.ToString ().Split (',');

			builder.Append (", LinkTarget = ");
			for (int i = 0; i < targets.Length; i++) {
				if (i > 0)
					builder.Append (" | ");
				builder.AppendFormat ("LinkTarget.{0}", targets[i].Trim ());
			}
		}

		bool GetBoolean (ITaskItem item, string property, bool defaultValue)
		{
			var text = item.GetMetadata (property);
			bool value;

			if (string.IsNullOrEmpty (text))
				return defaultValue;

			bool.TryParse (text, out value);

			return value;
		}

		void AppendBooleanProperty (StringBuilder builder, ITaskItem item, string property, bool defaultValue = false)
		{
			if (GetBoolean (item, property, defaultValue))
				builder.AppendFormat (", {0} = true", property);
		}

		void AppendStringProperty (StringBuilder builder, ITaskItem item, string property)
		{
			var value = item.GetMetadata (property);

			if (string.IsNullOrEmpty (value))
				return;

			builder.AppendFormat (", {0} = \"", property);
			for (int i = 0; i < value.Length; i++) {
				if (value[i] == '\\' || value[i] == '"')
					builder.Append ('\\');
				builder.Append (value[i]);
			}
			builder.Append ('"');
		}

		public override bool Execute ()
		{
			Log.LogTaskName ("PrepareNativeReferences");
			Log.LogTaskProperty ("IntermediateOutputPath", IntermediateOutputPath);
			Log.LogTaskProperty ("NativeReferences", NativeReferences);

			if (NativeReferences == null || NativeReferences.Length == 0)
				return !Log.HasLoggedErrors;

			var embeddedResources = new List<ITaskItem> ();
			var nativeFrameworks = new List<ITaskItem> ();
			var text = new StringBuilder ();

			for (int i = 0; i < NativeReferences.Length; i++) {
				NativeReferenceKind kind;
				string value;

				value = NativeReferences[i].GetMetadata ("Kind") ?? string.Empty;
				if (!Enum.TryParse (value, out kind)) {
					Log.LogError (null, null, null, NativeReferences[i].ItemSpec, 0, 0, 0, 0, "Unknown native reference type: {0}", value);
					continue;
				}

				var path = NativeReferences[i].ItemSpec;
				var logicalName = Path.GetFileName (path);

				var item = new TaskItem (path);

				if (kind == NativeReferenceKind.Framework) {
					nativeFrameworks.Add (item);
				} else {
					item.SetMetadata ("LogicalName", logicalName);
					embeddedResources.Add (item);
				}

				text.AppendFormat ("[assembly: ObjCRuntime.LinkWith (\"{0}\"", logicalName);
				AppendLinkTargetProperty (text, NativeReferences[i]);
				AppendBooleanProperty (text, NativeReferences[i], "IsCxx");
				AppendBooleanProperty (text, NativeReferences[i], "NeedsGccExceptionHandling");
				AppendBooleanProperty (text, NativeReferences[i], "SmartLink", true);
				AppendBooleanProperty (text, NativeReferences[i], "ForceLoad");
				AppendStringProperty (text, NativeReferences[i], "Frameworks");
				AppendStringProperty (text, NativeReferences[i], "WeakFrameworks");
				AppendStringProperty (text, NativeReferences[i], "LinkerFlags");
				text.Append (")]");
				text.AppendLine ();
			}

			var linkWith = Path.Combine (IntermediateOutputPath, "LinkWithAttributes.cs");
			Directory.CreateDirectory (Path.GetDirectoryName (linkWith));
			File.WriteAllText (linkWith, text.ToString ());

			EmbeddedResources = embeddedResources.ToArray ();
			NativeFrameworks = nativeFrameworks.ToArray ();
			LinkWithAttributes = new TaskItem (linkWith);

			return !Log.HasLoggedErrors;
		}
	}
}
