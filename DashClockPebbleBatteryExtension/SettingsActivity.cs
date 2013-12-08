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
	}
}

