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
	struct PebbleBatteryStatus {
		public float Percentage;
		public bool IsCharging;
	}

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
			batteryReceiver = new BatteryDataReceiver (Uuid);
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
				try {
					var b = await GetWatchBatteryLevel ();
					var status = b.IsCharging ? string.Format ("Charging ({0:P1})", b.Percentage) : b.Percentage.ToString ("P1");
					data.Visible (true)
						.Icon (Resource.Drawable.ic_dash_icon)
						.Status (status);
				} catch (TaskCanceledException) {
					return;
				}
			}

			PublishUpdate (data);
		}

		async Task<PebbleBatteryStatus> GetWatchBatteryLevel ()
		{
			var req = batteryReceiver.GetBatteryStatusAsync ();
			var uuid = Java.Util.UUID.FromString (Uuid);
			PebbleKit.StartAppOnPebble (this, uuid);
			Log.Info (Tag, "Started pebble app");
			var r = await req;
			Log.Info (Tag, "Received value: " + r.Percentage);
			PebbleKit.CloseAppOnPebble (this, uuid);
			Log.Info (Tag, "Closed pebble app");
			return r;
		}
	}

	class BatteryDataReceiver : PebbleKit.PebbleDataReceiver
	{
		TaskCompletionSource<PebbleBatteryStatus> currentBatteryRequest;

		public BatteryDataReceiver (string uuid) : base (Java.Util.UUID.FromString (uuid))
		{
		}

		public Task<PebbleBatteryStatus> GetBatteryStatusAsync ()
		{
			if (currentBatteryRequest != null) {
				currentBatteryRequest.TrySetCanceled ();
				currentBatteryRequest = null;
			}
			currentBatteryRequest = new TaskCompletionSource<PebbleBatteryStatus> ();
			return currentBatteryRequest.Task;
		}

		public override void ReceiveData (Context context, int transactionID, PebbleDictionary dict)
		{
			if (currentBatteryRequest == null)
				return;
			Log.Info ("PebbleBatDashClockReceiver", "Received data, tid: " + transactionID);
			var level = dict.GetInteger (0xba77).FloatValue () / 100f;
			Log.Info ("PebbleBatDashClockReceiver", "\tLevel: " + level);
			var isCharging = dict.GetInteger (0xba1e).IntValue () == 1;
			Log.Info ("PebbleBatDashClockReceiver", "\tCharging status: " + isCharging);
			currentBatteryRequest.TrySetResult (new PebbleBatteryStatus {
				IsCharging = isCharging,
				Percentage = level
			});
			PebbleKit.SendAckToPebble (context, transactionID);
		}
	}
}

