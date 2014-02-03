//
// SettingsController.cs
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
	public class SettingsController : DialogViewController
	{
		public readonly Settings Settings;
		bool modified;

		public SettingsController ()
			: base (new RootElement ("Settings"), true)
		{
			Settings = new Settings ();

			var section = new Section ();
			Root.Add (section);

			var showListening = new BooleanElement ("Show listening sockets", Settings.ShowListening);
			showListening.ValueChanged += (sender, e) => {
				Settings.ShowListening = showListening.Value;
				modified = true;
			};
			section.Add (showListening);

			var showLocal = new BooleanElement ("Show local connections", Settings.ShowLocal);
			showLocal.ValueChanged += (sender, e) => {
				Settings.ShowLocal = showLocal.Value;
				modified = true;
			};
			section.Add (showLocal);

			var autoRefresh = new BooleanElement ("Auto refresh", Settings.AutoRefresh);
			autoRefresh.ValueChanged += (sender, e) => {
				Settings.AutoRefresh = autoRefresh.Value;
				modified = true;
			};
			section.Add (autoRefresh);
		}
	}
}

