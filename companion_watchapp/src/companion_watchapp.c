#include <pebble.h>

//static DataLoggingSessionRef batteryDataLog = NULL;
static BatteryChargeState batteryState;

int main(void) {
	batteryState = battery_state_service_peek();
	const uint32_t inbound_size = 16;
	const uint32_t outbound_size = 16;

	app_message_open(inbound_size, outbound_size);

	DictionaryIterator *iter;
	app_message_outbox_begin(&iter);
	dict_write_int(iter, 0xba77, &batteryState.charge_percent, 1, true);
	app_message_outbox_send ();

	APP_LOG(APP_LOG_LEVEL_DEBUG, "Done initializing, pushing value: %d", batteryState.charge_percent);
  
	app_event_loop();
}

/* int main(void) { */
/* 	batteryDataLog = data_logging_create(0xba77, DATA_LOGGING_INT, 1, true); */
/* 	batteryState = battery_state_service_peek(); */

/* 	APP_LOG(APP_LOG_LEVEL_DEBUG, "Done initializing, pushing value: %d", batteryState.charge_percent); */
	
/* 	data_logging_log(batteryDataLog, &batteryState.charge_percent, 1); */
  
/* 	app_event_loop(); */
/* 	data_logging_finish(batteryDataLog); */
/* } */
