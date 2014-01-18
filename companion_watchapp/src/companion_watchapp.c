#include <pebble.h>

#define PADDING 25

static BatteryChargeState batteryState;
static Window * window = NULL;
static TextLayer *battery_level_text = NULL;
static TextLayer *explanation_text = NULL;
static char* level = NULL;

static void initialize_ui ()
{
	window = window_create();
	window_stack_push(window, true);
	Layer *window_layer = window_get_root_layer(window);
	GRect bounds = layer_get_frame(window_layer);
	int height = (bounds.size.h - 2 * PADDING) / 2;

	battery_level_text = text_layer_create((GRect){
		.origin = { 0, PADDING },
		.size = bounds.size
	});
	explanation_text = text_layer_create((GRect){
		.origin = { 0, PADDING + height },
		.size = bounds.size
	});

	text_layer_set_text_alignment(battery_level_text, GTextAlignmentCenter);
	text_layer_set_text_alignment(explanation_text, GTextAlignmentCenter);
	text_layer_set_font(explanation_text, fonts_get_system_font(FONT_KEY_GOTHIC_18));
	text_layer_set_font(battery_level_text, fonts_get_system_font(FONT_KEY_BITHAM_42_BOLD));

	level = malloc(5);
	memset(level, '\0', 5);
	snprintf(level, 5, "%d%%", batteryState.charge_percent);
	text_layer_set_text(battery_level_text, level);

	text_layer_set_text(explanation_text, "Communicating with Dashclock. Standby...");

	layer_add_child(window_layer, text_layer_get_layer(battery_level_text));
	layer_add_child(window_layer, text_layer_get_layer(explanation_text));
}

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

	initialize_ui();

	app_event_loop();

	if (battery_level_text != NULL)
		text_layer_destroy(battery_level_text);
	if (explanation_text != NULL)
		text_layer_destroy(explanation_text);
	if (level != NULL)
		free(level);
	if (window != NULL)
		window_destroy(window);
}
