//
// NetstatTableSource.cs
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
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Xamarin.NetworkUtils.PhoneTest
{
	public class NetstatTableSource : UITableViewSource
	{
		IList<NetstatEntry> entries;
		protected const string cellIdentifier = "TableCell";

		internal const int FontSize = 14;

		public NetstatTableSource ()
		{
			Refresh ();
		}

		public void Refresh ()
		{
			entries = ManagedNetstat.GetTcp ();
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return entries.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (cellIdentifier);
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);

			var idx = (int)indexPath.IndexAtPosition (1);
			var entry = entries [idx];

			cell.TextLabel.Font = UIFont.SystemFontOfSize (FontSize);
			cell.TextLabel.Text = string.Format ("{0} - {1} - {2}", entry.LocalEndpoint, entry.RemoteEndpoint, entry.State);
			return cell;
		}
	}
}

