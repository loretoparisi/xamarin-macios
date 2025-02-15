// 
// UIView.cs: Implements the managed UIView
//
// Authors:
//   Geoff Norton.
//     
// Copyright 2009 Novell, Inc
// Copyrigh 2014, Xamarin Inc.
//

#if !WATCH

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XamCore.Foundation; 
using XamCore.ObjCRuntime;
using XamCore.CoreGraphics;

namespace XamCore.UIKit {
	public partial class UIView : IEnumerable {

		public void Add (UIView view)
		{
			AddSubview (view);
		}

		public void AddSubviews (params UIView [] views)
		{
			if (views == null)
				return;
			foreach (var v in views)
				AddSubview (v);
		}

		public IEnumerator GetEnumerator ()
		{
			UIView [] subviews = Subviews;
			if (subviews == null)
				yield break;
			foreach (UIView uiv in subviews)
				yield return uiv;
		}

		public static void BeginAnimations (string animation)
		{
			BeginAnimations (animation, IntPtr.Zero);
		}
			
#if !XAMCORE_2_0
		[CompilerGenerated]
		[Since (4,2)]
		public virtual bool EnableInputClicksWhenVisible {
			[Since (4,2)]
			[Export ("enableInputClicksWhenVisible")]
			get {
				global::MonoTouch.UIKit.UIApplication.EnsureUIThread ();
				return false;
			}
		}
#endif

		[Register]
		class _UIViewStaticCallback : NSObject {
			static _UIViewStaticCallback shared;
			public const string start = "start";
			public const string end = "end";
			public event NSAction WillStart, WillEnd;

			public _UIViewStaticCallback ()
			{
				IsDirectBinding = false;
			}

			[Preserve (Conditional = true)]
			[Export ("start")]
			public void OnStart ()
			{
				if (WillStart != null)
					WillStart ();
			}

			[Preserve (Conditional = true)]
			[Export ("end")]
			public void OnEnd ()
			{
				shared = null;
				if (WillEnd != null)
					WillEnd ();
			}

			public static _UIViewStaticCallback Prepare ()
			{
				if (shared == null){
					shared = new _UIViewStaticCallback ();
					SetAnimationDelegate (shared);
				}
				return shared;
			}
		}
		
		public static event NSAction AnimationWillStart {
			add {
				_UIViewStaticCallback.Prepare ().WillStart += value;
			}
			remove {
				_UIViewStaticCallback.Prepare ().WillStart -= value;
			}
		}

		public static event NSAction AnimationWillEnd {
			add {
				_UIViewStaticCallback.Prepare ().WillEnd += value;
			}
			remove {
				_UIViewStaticCallback.Prepare ().WillEnd -= value;
			}
		}

		[Advice ("Use the *Notify method that has `UICompletionHandler completion` parameter, the `bool` will tell you if the operation finished")]
		public static void Animate (double duration, NSAction animation, NSAction completion)
		{
			// animation null check will be done in AnimateNotify
			AnimateNotify (duration, animation, (x) => { 
				if (completion != null)
					completion (); 
			});
		}

		[Advice ("Use the *Notify method that has `UICompletionHandler completion` parameter, the `bool` will tell you if the operation finished")]
		public static void Animate (double duration, double delay, UIViewAnimationOptions options, NSAction animation, NSAction completion)
		{
			// animation null check will be done in AnimateNotify
			AnimateNotify (duration, delay, options, animation, (x) => {
				if (completion != null)
					completion (); 
			});
		}

		[Advice ("Use the *Notify method that has `UICompletionHandler completion` parameter, the `bool` will tell you if the operation finished")]
		public static void Transition (UIView fromView, UIView toView, double duration, UIViewAnimationOptions options, NSAction completion)
		{
			TransitionNotify (fromView, toView, duration, options, (x) => {
				if (completion != null)
					completion (); 
			});
		}

		[Advice ("Use the *Notify method that has `UICompletionHandler completion` parameter, the `bool` will tell you if the operation finished")]
		public static void Transition (UIView withView, double duration, UIViewAnimationOptions options, NSAction animation, NSAction completion)
		{
			// animation null check will be done in AnimateNotify
			TransitionNotify (withView, duration, options, animation, (x) => {
				if (completion != null)
					completion (); 
			});
		}

#if !XAMCORE_2_0
		[Advice ("Use the version with a `ref float actualFontSize`")]
		public CGSize DrawString (string str, CGPoint point, nfloat width, XamCore.UIKit.UIFont font, nfloat minFontSize, nfloat actualFontSize, XamCore.UIKit.UILineBreakMode breakMode, XamCore.UIKit.UIBaselineAdjustment adjustment)
		{
			nfloat temp = actualFontSize;
			return DrawString (str, point, width, font, minFontSize, ref temp, breakMode, adjustment);
		}

		[Obsolete ("Use TranslatesAutoresizingMaskIntoConstraints")]
		bool TranslatesAutoresizingMaskIntoConstrainst { 
			get { return TranslatesAutoresizingMaskIntoConstraints; }
			set { TranslatesAutoresizingMaskIntoConstraints = value; }
		}
#endif

		public static Task<bool> AnimateAsync (double duration, NSAction animation)
		{
			return AnimateNotifyAsync (duration, animation);
		}
	}
}

#endif // !WATCH
