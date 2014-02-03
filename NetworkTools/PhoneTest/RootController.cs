//
// RootController.cs
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
using System.Net;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace Xamarin.NetworkUtils.PhoneTest
{
	public class RootController : DialogViewController
	{
		public readonly SettingsController SettingsController;

		Section section;

		public RootController ()
			: base (new RootElement ("Netstat"))
		{
			section = new Section ("Open Sockets");
			Root.Add (section);

			SettingsController = new SettingsController (this);

			RefreshRequested += (sender, e) => {
				Populate ();
				ReloadComplete ();
			};
		}

		void Populate ()
		{
			section.Clear ();
			foreach (var entry in ManagedNetstat.GetTcp ()) {
				if (!Filter (entry))
					continue;
				var text = string.Format ("{0} - {1} - {2}", entry.LocalEndpoint, entry.RemoteEndpoint, entry.State);
				section.Add (new StringElement (text));
			}
		}

		bool IsLocalHost (IPAddress address)
		{
			var bytes = address.GetAddressBytes ();
			if (bytes.Length != 4)
				return false;
			if (bytes [0] != 127)
				return false;
			if (bytes [1] != 0)
				return false;
			if (bytes [2] != 0)
				return false;
			if (bytes [3] != 1)
				return false;
			return true;
		}

		bool Filter (NetstatEntry entry)
		{
			if (!SettingsController.Settings.ShowListening && entry.State == TcpState.Listen)
				return false;
			if (!SettingsController.Settings.ShowLocal) {
				if (IsLocalHost (entry.LocalEndpoint.Address) && IsLocalHost (entry.RemoteEndpoint.Address))
					return false;
			}
			return true;
		}

		public override void ViewDidAppear (bool animated)
		{
			Populate ();
			base.ViewDidAppear (animated);
		}
	}
}
