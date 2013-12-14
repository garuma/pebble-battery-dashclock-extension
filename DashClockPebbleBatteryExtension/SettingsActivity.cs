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
		const string Sdk2Location = "https://developer.getpebble.com/2/getting-started/";
		const int WrongFirmwareVersion = 1;

		PebbleKit.FirmwareVersionInfo fwVersion;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			var btn = FindViewById<Button> (Resource.Id.watchAppBtn);
			btn.Click += HandleClick;
		}

		void HandleClick (object sender, EventArgs e)
		{
			var uri = Android.Net.Uri.Parse (fwVersion != null && fwVersion.Major == WrongFirmwareVersion ? Sdk2Location : PbwLocation);
			var intent = new Intent (Intent.ActionView, uri);
			StartActivity (intent);
		}

		protected override void OnStart ()
		{
			base.OnStart ();
			fwVersion = PebbleKit.GetWatchFWVersion (this);

			if (fwVersion != null && fwVersion.Major == WrongFirmwareVersion) {
				FindViewById (Resource.Id.backview).SetBackgroundResource (Resource.Drawable.background_error);
				FindViewById<TextView> (Resource.Id.subtitle).SetText (Resource.String.wrong_sdk_message);
				FindViewById<ImageView> (Resource.Id.banner).SetImageResource (Resource.Drawable.ic_banner_error);
				FindViewById<Button> (Resource.Id.watchAppBtn).SetText (Resource.String.button_install_sdk_two);
			} else {
				FindViewById (Resource.Id.backview).SetBackgroundResource (Resource.Drawable.background);
				FindViewById<TextView> (Resource.Id.subtitle).SetText (Resource.String.normal_message);
				FindViewById<ImageView> (Resource.Id.banner).SetImageResource (Resource.Drawable.ic_banner);
				FindViewById<Button> (Resource.Id.watchAppBtn).SetText (Resource.String.button_install_watchapp);
			}
		}
	}
}

