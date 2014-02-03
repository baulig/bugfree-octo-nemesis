using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Xamarin.NetworkUtils.PhoneTest
{
	public class NetstatTableViewController : UITableViewController
	{
		NetstatTableSource source;
		UIRefreshControl refresh;

		public NetstatTableViewController () : base (UITableViewStyle.Grouped)
		{
			source = new NetstatTableSource ();

			refresh = new UIRefreshControl ();
			refresh.ValueChanged += (sender, e) => {
				source.Refresh ();
				refresh.EndRefreshing ();
				TableView.ReloadData ();
			};
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Add (refresh);
			
			// Register the TableView's data source
			TableView.Source = new NetstatTableSource ();
			TableView.RowHeight = (float)NetstatTableSource.FontSize * 1.2f;
		}
	}
}

