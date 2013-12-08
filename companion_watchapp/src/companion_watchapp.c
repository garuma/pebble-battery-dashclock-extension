#include <pebble.h>

//static DataLoggingSessionRef batteryDataLog = NULL;
static BatteryChargeState batteryState;

int main(void) {
	batteryState = battery_state_service_peek();
	const uint32_t inbound_size = 16;
	const uint32_t outbound_size = dict_calc_buffer_size(2, 4, 4);

	app_message_open(inbound_size, outbound_size);

	DictionaryIterator *iter;
	app_message_outbox_begin(&iter);
	dict_write_int(iter, 0xba77, &batteryState.charge_percent, 1, true);
	dict_write_int(iter, 0xba1e, &batteryState.is_charging, 1, true);
	app_message_outbox_send ();

	APP_LOG(APP_LOG_LEVEL_DEBUG, "Done initializing, pushing value: %d", batteryState.charge_percent);
  
	app_event_loop();
}
