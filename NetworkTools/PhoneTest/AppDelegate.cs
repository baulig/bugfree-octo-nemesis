//
// AppDelegate.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace Xamarin.NetworkUtils.PhoneTest
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;

		void Populate (Section section)
		{
			section.Clear ();
			foreach (var entry in ManagedNetstat.GetTcp ()) {
				var text = string.Format ("{0} - {1} - {2}", entry.LocalEndpoint, entry.RemoteEndpoint, entry.State);
				section.Add (new StringElement (text));
			}
		}

		DialogViewController CreateRoot ()
		{
			var netstatSection = new Section ("Open Sockets");

			var root = new RootElement ("Netstat") {
				netstatSection
			};

			var dvc = new DialogViewController (root);
			dvc.RefreshRequested += (sender, e) => {
				Populate (netstatSection);
				dvc.ReloadComplete ();
			};
			Populate (netstatSection);

			CreateSettings (dvc);

			return dvc;
		}

		void CreateSettings (DialogViewController root)
		{
			var settingsButton = new UIBarButtonItem (UIBarButtonSystemItem.Edit);
			root.NavigationItem.RightBarButtonItem = settingsButton;

			var settingsRoot = new RootElement ("Settings");
			var settingsDvc = new DialogViewController (settingsRoot, true);

			settingsButton.Clicked += (sender, e) => {
				root.ActivateController (settingsDvc);
			};

			var section = new Section ();
			settingsRoot.Add (section);

			var showListening = new BooleanElement ("Show listening sockets", false);
			showListening.ValueChanged += (sender, e) => {
			};
			section.Add (showListening);
		}

		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			var root = CreateRoot ();

			var nav = new UINavigationController (root);

			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			window.RootViewController = nav;
			window.MakeKeyAndVisible ();

			return true;
		}
	}
}

