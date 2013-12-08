using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Apps.DashClock.Api;
using Pebble.Android;
using Pebble.Android.Util;

using Log = Android.Util.Log;

namespace DashClockPebbleBatteryExtension
{
	[Service (Label = "Pebble Battery Extension",
	          Permission = "com.google.android.apps.dashclock.permission.READ_EXTENSION_DATA",
	          Icon = "@drawable/ic_dash_icon")]
	[IntentFilter (new[] { "com.google.android.apps.dashclock.Extension" })]
	[MetaData ("protocolVersion", Value = "1")]
	[MetaData ("description", Value = "Report Pebble watch battery status")]
	[MetaData ("settingsActivity", Value = "dashclockpebblebatteryextension.SettingsActivity")]
	public class DashPebbleBatteryService : DashClockExtension
	{
		const string Uuid = "0e6ed8f2-995c-4df9-853e-1e2768a14446";
		const string Tag = "PebbleBatDashClock";

		BatteryDataReceiver batteryReceiver;
		BroadcastReceiver batteryBroadcast;

		protected override void OnInitialize (bool isReconnect)
		{
			if (isReconnect)
				return;
			batteryReceiver = new BatteryDataReceiver (this, Uuid);
			batteryBroadcast = PebbleKit.RegisterReceivedDataHandler (this, batteryReceiver);
		}

		public override void OnDestroy ()
		{
			UnregisterReceiver (batteryBroadcast);
			batteryBroadcast = null;
		}

		protected async override void OnUpdateData (int reason)
		{
			var data = new ExtensionData ();
			if (!PebbleKit.IsWatchConnected (this))
				data.Visible (false);
			else {
				var battery = await GetWatchBatteryLevel ();
				data.Visible (true)
					.Icon (Resource.Drawable.ic_dash_icon)
					.Status (battery.ToString ("P1"));
			}

			PublishUpdate (data);
		}

		async Task<float> GetWatchBatteryLevel ()
		{
			var req = batteryReceiver.GetBatteryLevelAsync ();
			var uuid = Java.Util.UUID.FromString (Uuid);
			PebbleKit.StartAppOnPebble (this, uuid);
			Log.Info (Tag, "Started pebble app");
			var r = await req;
			Log.Info (Tag, "Received value: " + r);
			PebbleKit.CloseAppOnPebble (this, uuid);
			Log.Info (Tag, "Closed pebble app");
			return r;
		}
	}

	class BatteryDataReceiver : PebbleKit.PebbleDataReceiver
	{
		Context context;
		TaskCompletionSource<float> currentBatteryRequest;

		public BatteryDataReceiver (Context context, string uuid) : base (Java.Util.UUID.FromString (uuid))
		{
			this.context = context;
		}

		public Task<float> GetBatteryLevelAsync ()
		{
			if (currentBatteryRequest != null) {
				currentBatteryRequest.TrySetCanceled ();
				currentBatteryRequest = null;
			}
			currentBatteryRequest = new TaskCompletionSource<float> ();
			return currentBatteryRequest.Task;
		}

		public override void ReceiveData (Context context, int transactionID, PebbleDictionary dict)
		{
			if (currentBatteryRequest == null)
				return;
			Log.Info ("PebbleBatDashClockReceiver", "Received data, tid: " + transactionID);
			currentBatteryRequest.TrySetResult (dict.GetInteger (0xba77).FloatValue () / 100f);
			PebbleKit.SendAckToPebble (context, transactionID);
		}
	}
}

