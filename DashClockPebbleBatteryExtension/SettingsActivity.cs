using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Pebble.Android;

namespace DashClockPebbleBatteryExtension
{
	[Activity (Label = "Pebble Battery Extension Settings",
	           Icon = "@drawable/Icon",
	           Exported = true,
	           Theme = "@android:style/Theme.Holo.Light.NoActionBar")]
	public class SettingsActivity : Activity
	{
		const string PbwLocation = "https://neteril.org/pebble/companion_watchapp.pbw";

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			var btn = FindViewById<Button> (Resource.Id.watchAppBtn);
			btn.Click += (sender, e) => {
				var uri = Android.Net.Uri.Parse (PbwLocation);
				var intent = new Intent (Intent.ActionView, uri);
				StartActivity (intent);
			};
		}

		protected override void OnStart ()
		{
			base.OnStart ();
			var fwVersion = PebbleKit.GetWatchFWVersion (this);
			if (fwVersion != null && fwVersion.Major == 1) {
				FindViewById (Resource.Id.backview).SetBackgroundResource (Resource.Drawable.background_error);
				FindViewById (Resource.Id.watchAppBtn).Visibility = ViewStates.Invisible;
				FindViewById<TextView> (Resource.Id.subtitle).SetText (Resource.String.wrong_sdk_message);
				FindViewById<ImageView> (Resource.Id.banner).SetImageResource (Resource.Drawable.ic_banner_error);
			} else {
				FindViewById (Resource.Id.backview).SetBackgroundResource (Resource.Drawable.background);
				FindViewById (Resource.Id.watchAppBtn).Visibility = ViewStates.Visible;
				FindViewById<TextView> (Resource.Id.subtitle).SetText (Resource.String.normal_message);
				FindViewById<ImageView> (Resource.Id.banner).SetImageResource (Resource.Drawable.ic_banner);
			}
		}
	}
}

