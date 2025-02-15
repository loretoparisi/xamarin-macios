// 
// UIScreen.cs: Helper methods for UIScreen.
//
// Authors:
//   Miguel de Icaza
//     
// Copyright 2010 Novell, Inc
// Copyright 2014 Xamarin Inc.
//

#if !WATCH

using System;
using System.Collections;
using XamCore.Foundation; 
using XamCore.ObjCRuntime;
using XamCore.CoreGraphics;

namespace XamCore.UIKit {
	public partial class UIScreen {

#if !XAMCORE_2_0
		[Obsolete ("Use CreateDisplayLink instead")]
		XamCore.CoreAnimation.CADisplayLink DisplayLink (NSObject target, Selector sel)
		{
			return CreateDisplayLink (target, sel);
		}
#endif
		
		public XamCore.CoreAnimation.CADisplayLink CreateDisplayLink (NSAction action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			var d = new NSActionDispatcher (action);
			return CreateDisplayLink (d, NSActionDispatcher.Selector);
		}

		public UIImage Capture ()
		{
			// This is from: https://developer.apple.com/library/ios/#qa/qa2010/qa1703.html
			var selScreen = new Selector ("screen");
			var size = Bounds.Size;

			UIGraphics.BeginImageContextWithOptions (size, false, 0);

			try {
				var context = UIGraphics.GetCurrentContext ();

				foreach (var window in UIApplication.SharedApplication.Windows) {
					if (window.RespondsToSelector (selScreen) && window.Screen != this)
						continue;

					context.SaveState ();
					context.TranslateCTM (window.Center.X, window.Center.Y);
					context.ConcatCTM (window.Transform);
					context.TranslateCTM (-window.Bounds.Size.Width * window.Layer.AnchorPoint.X, -window.Bounds.Size.Height * window.Layer.AnchorPoint.Y);

					window.Layer.RenderInContext (context);
					context.RestoreState ();
				}

				return UIGraphics.GetImageFromCurrentImageContext ();
			} finally {
				UIGraphics.EndImageContext ();
			}
		}
	}
}

#endif // !WATCH
