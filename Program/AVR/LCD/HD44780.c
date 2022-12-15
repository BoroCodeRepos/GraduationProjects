#include "HD44780.h"

#if USE_BACKLIGHT
static byte lcd_backlight;
#endif

#if USE_TWI
#define STROBE(data) (data | _BV(LCD_E))
#endif

/*
 *
 * statyczne funkcje steruj¹ce wyœwietlaczem LCD
 *
 * */

#if USE_TWI
static byte lcd_get_nibble(byte data);
static byte lcd_get_nibble(byte data)
{
	byte ret = 0;

	if (data & 0x01) ret |= _BV(LCD_D4);
	if (data & 0x02) ret |= _BV(LCD_D5);
	if (data & 0x04) ret |= _BV(LCD_D6);
	if (data & 0x08) ret |= _BV(LCD_D7);

	return ret;
}
#endif

#if USE_RW & USE_TWI
static byte lcd_parse_data(const byte * data);
static byte lcd_parse_data(const byte * data)
{
	byte ret = 0;

	for (byte i = 0; i < 2; i++)
	{
		if (data[i] & _BV(LCD_D4)) ret |= (i ? 0x01 : 0x10);
		if (data[i] & _BV(LCD_D5)) ret |= (i ? 0x02 : 0x20);
		if (data[i] & _BV(LCD_D6)) ret |= (i ? 0x04 : 0x40);
		if (data[i] & _BV(LCD_D7)) ret |= (i ? 0x08 : 0x80);
	}

	return ret;
}
#endif

#if !USE_TWI
static inline void lcd_set_data_pins_out(void);
static inline void lcd_set_data_pins_out(void)
{
	DDR(LCD_D4_PORT) |= _BV(LCD_D4);
	DDR(LCD_D5_PORT) |= _BV(LCD_D5);
	DDR(LCD_D6_PORT) |= _BV(LCD_D6);
	DDR(LCD_D7_PORT) |= _BV(LCD_D7);
}

static inline void lcd_set_data_pins_in(void);
static inline void lcd_set_data_pins_in(void)
{
	DDR(LCD_D4_PORT) &= ~_BV(LCD_D4);
	DDR(LCD_D5_PORT) &= ~_BV(LCD_D5);
	DDR(LCD_D6_PORT) &= ~_BV(LCD_D6);
	DDR(LCD_D7_PORT) &= ~_BV(LCD_D7);
}

static inline void lcd_set_dir_out(void);
static inline void lcd_set_dir_out(void)
{
	DDR(LCD_E_PORT) |= _BV(LCD_E);
	DDR(LCD_RS_PORT) |= _BV(LCD_RS);
#if USE_RW
	DDR(LCD_RW_PORT) |= _BV(LCD_RW);
#endif
	lcd_set_data_pins_out();
}

static inline void lcd_set_ctrl_pins_state(void);
static inline void lcd_set_ctrl_pins_state(void)
{
	PORT(LCD_E_PORT) &= ~_BV(LCD_E);
	PORT(LCD_RS_PORT) &= ~_BV(LCD_RS);
#if USE_RW
	PORT(LCD_RW_PORT) &= ~_BV(LCD_RW);
#endif
}

static inline void lcd_send_half(const uint8_t data);
static inline void lcd_send_half(const uint8_t data)
{
	if (data & 0x01) PORT(LCD_D4_PORT) |= _BV(LCD_D4);
	else PORT(LCD_D4_PORT) &= ~_BV(LCD_D4);

	if (data & 0x02) PORT(LCD_D5_PORT) |= _BV(LCD_D5);
	else PORT(LCD_D5_PORT) &= ~_BV(LCD_D5);

	if (data & 0x04) PORT(LCD_D6_PORT) |= _BV(LCD_D6);
	else PORT(LCD_D6_PORT) &= ~_BV(LCD_D6);

	if (data & 0x08) PORT(LCD_D7_PORT) |= _BV(LCD_D7);
	else PORT(LCD_D7_PORT) &= ~_BV(LCD_D7);
}
#endif

#if USE_RW & !USE_TWI
static inline uint8_t lcd_read_half(void);
static inline uint8_t lcd_read_half(void)
{
	uint8_t result = 0;
	if (PIN(LCD_D4_PORT) & _BV(LCD_D4)) result |= 0x01;
	if (PIN(LCD_D5_PORT) & _BV(LCD_D5)) result |= 0x02;
	if (PIN(LCD_D6_PORT) & _BV(LCD_D6)) result |= 0x04;
	if (PIN(LCD_D7_PORT) & _BV(LCD_D7)) result |= 0x08;
	return result;
}
#endif

#if USE_RW
static inline uint8_t lcd_read(const bool RS_state);
static inline uint8_t lcd_read(const bool RS_state)
{
#if USE_TWI
#if USE_BACKLIGHT
	byte state = (RS_state << LCD_RS) | _BV(LCD_RW) | lcd_backlight;
#else
	byte state = (RS_state << LCD_RS) | _BV(LCD_RW);
#endif

	state |= lcd_get_nibble(0xFF);

	byte data[2];
	for (byte i = 0; i < 2; i++)
	{
		twi_start();
		twi_write_sla(TW_SLA_W(LCD_ADDR));
		twi_write(state);
		twi_write(STROBE(state));
		twi_stop();

		twi_start();
		twi_write_sla(TW_SLA_R(LCD_ADDR));
		data[i] = twi_read(ACK);
		twi_stop();
	}
	twi_start();
	twi_write_sla(TW_SLA_W(LCD_ADDR));
	twi_write(state);
	twi_stop();

	return lcd_parse_data(data);
#else
	uint8_t result = 0;
	lcd_set_data_pins_in();

	if (RS_state) SET_RS;
	else CLR_RS;
	SET_RW;
	SET_E;
	result |= lcd_read_half() << 4;
	CLR_E;

	SET_E;
	result |= lcd_read_half();
	CLR_E;

	CLR_RW;
	lcd_set_data_pins_out();
	return result;
#endif
}

static inline uint8_t lcd_check_busy_flag();
static inline uint8_t lcd_check_busy_flag()
{
	return (lcd_read(false) & (1 << 7));
}
#endif

static inline void lcd_send(const byte data, const bool RS_state);
static inline void lcd_send(const byte data, const bool RS_state)
{
#if USE_TWI
#if USE_BACKLIGHT
	byte state = (RS_state << LCD_RS) | lcd_backlight;
#else
	byte state = RS_state << LCD_RS;
#endif

	byte higher = state | lcd_get_nibble(data >> 4);
	byte lower  = state | lcd_get_nibble(data);

	byte to_transmit[4] = { STROBE(higher), higher, STROBE(lower), lower };

	twi_start();
	twi_write_sla(TW_SLA_W(LCD_ADDR));
	for (byte i = 0; i < 4; i++)
		twi_write(to_transmit[i]);
	twi_stop();
#else
	if (RS_state) SET_RS;
	else CLR_RS;
#if USE_RW
	CLR_RW;
#endif
	SET_E;
	lcd_send_half(data >> 4);
	CLR_E;

	SET_E;
	lcd_send_half(data);
	CLR_E;
#endif

#if USE_RW
	while (lcd_check_busy_flag())
	{
		NO_STATEMENT;
	}
#else
	_delay_us(120);
#endif
}

static inline void lcd_data(const byte data);
static inline void lcd_data(const byte data)
{
	lcd_send(data, true);
}

static inline void lcd_cmd(const LCD_COMMAND command);
static inline void lcd_cmd(const LCD_COMMAND command)
{
	lcd_send((const byte)command, false);
}

/*
 *
 * funkcje steruj¹ce wyœwietlaczem LCD
 *
 *
 */

void lcd_init(void)
{
#if USE_TWI
	lcd_backlight_on();
	twi_start();
	twi_write_sla(TW_SLA_W(LCD_ADDR));
#if USE_BACKLIGHT
	twi_write(lcd_get_nibble(0xFF) | lcd_backlight);
#else
	twi_write(lcd_get_nibble(0xFF));
#endif
	twi_stop();
	cbi(PORTB, PB0);

	_delay_ms(15);

	byte first_value = lcd_get_nibble(0x03);
	byte second_value = lcd_get_nibble(0x02);
	byte to_transmit[4] = { first_value, first_value, first_value, second_value };

	twi_start();
	twi_write_sla(TW_SLA_W(LCD_ADDR));
	for (byte i = 0; i < 4; i++)
	{
	#if USE_BACKLIGHT
		twi_write(STROBE(to_transmit[i]) | lcd_backlight);
		twi_write(to_transmit[i] | lcd_backlight);
	#else
		twi_write(STROBE(to_transmit[i]));
		twi_write(to_transmit[i]);
	#endif

		if (i == 0) _delay_ms(4.9);
	}
	twi_stop();

#else
	lcd_set_dir_out();
	lcd_set_ctrl_pins_state();

	_delay_ms(15);

	SET_E;
	lcd_send_half(0x03);
	CLR_E;
	_delay_ms(4.1);
	SET_E;
	lcd_send_half(0x03);
	CLR_E;
	_delay_us(100);
	SET_E;
	lcd_send_half(0x03);
	CLR_E;
	_delay_us(100);
#endif
	lcd_cmd(FONT5x7_2LINE_4BIT);
	lcd_cmd(DISPLAY_OFF);
	lcd_cmd(ENTRY_MODE_SHIFT_CURSOR_INCREMENT);
	lcd_cmd(DISPLAY_ON_CURSOR_ON_NOBLINK);

	lcd_cls();
}
void lcd_cls(void)
{
	lcd_cmd(CLEAR);
#if USE_RW == 0
	_delay_ms(4.9);
#endif
}
void lcd_locate(byte row, byte column)
{
	if (row == 1) row = LCD_LINE1;
	else if (row == 2) row = LCD_LINE2;
	else if (row == 3) row = LCD_LINE3;
	else row = LCD_LINE0;
	lcd_cmd(0x80 + row + column);
}

#if USE_RW
byte lcd_get_address(void)
{
	return lcd_read(false) & 0x7F;
}
byte lcd_get_data()
{
	return lcd_read(true);
}
#endif

#if USE_LCD_CURSOR_HOME
void lcd_home(void)
{
	lcd_cmd(HOME);
	#if USE_RW == 0
		_delay_ms(4.9);
	#endif
}
#endif

#if USE_LCD_CURSOR_ON
void lcd_cursor_on(void)
{
	lcd_cmd(DISPLAY_ON_CURSOR_ON_NOBLINK);
}
void lcd_cursor_off(void)
{
	lcd_cmd(DISPLAY_ON_CURSOR_OFF);
}
#endif

#if USE_LCD_CURSOR_BLINK
void lcd_blink_on(void)
{
	lcd_cmd(DISPLAY_ON_CURSOR_ON_BLINK);
}
void lcd_blink_off(void)
{
	lcd_cmd(DISPLAY_ON_CURSOR_ON_NOBLINK);
}
#endif

#if USE_LCD_STREAM
static int lcd_char_stream(char c, FILE * Stream);
static int lcd_char_stream(char c, FILE * Stream)
{
	lcd_char(c);
	return 0;
}
void lcd_create_stream(FILE * Stream)
{
	*Stream = (FILE)FDEV_SETUP_STREAM(lcd_char_stream, NULL, _FDEV_SETUP_WRITE);
}
#endif

#if USE_BACKLIGHT && USE_TWI
void lcd_backlight_on(void)
{
	lcd_backlight = _BV(LCD_BACKLIGHT);

	twi_start();
	twi_write_sla(TW_SLA_W(LCD_ADDR));
	twi_write(lcd_get_nibble(0xFF) | lcd_backlight);
	twi_stop();
}
void lcd_backlight_off(void)
{
	lcd_backlight = 0;

	twi_start();
	twi_write_sla(TW_SLA_W(LCD_ADDR));
	twi_write(lcd_get_nibble(0xFF));
	twi_stop();
}
#endif

/*
 *
 * funkcje prezentacji danych
 *
 * */

void lcd_char(const char _char)
{
#if USE_LCD_DEFCHAR || USE_LCD_DEFCHAR_P || USE_LCD_DEFCHAR_E
	register char data;
	data = ((_char >= 0x80) ? (_char & 0x07) : _char);
	lcd_data(data);
#else
	lcd_data(_char);
#endif
}
void lcd_str(const char * text)
{
	while (*text)
	{
		lcd_char(*text);
		text++;
	}
}

#if USE_LCD_STR_P
void lcd_str_P(const char * text)
{
	register char _char = 0;
	while (1)
	{
		_char = pgm_read_byte(text);

		if (_char == 0) break;

		text++;
		lcd_char(_char);
	}
}
#endif

#if USE_LCD_STR_E
void lcd_str_E(const char * text)
{
	register char _char = 0;
	while (1)
	{
		_char = eeprom_read_byte((const unsigned char*)text);

		if (_char == 0) break;

		text++;
		lcd_char(_char);
	}
}
#endif

#if USE_LCD_INT
void lcd_int(const int number)
{
	char itoa_buf[17];
	lcd_str(itoa(number, itoa_buf, 10));
}
void lcd_uint(const unsigned number)
{
	char utoa_buf[17];
	lcd_str(utoa(number, utoa_buf, 10));
}
void lcd_long(const long number)
{
	char ltoa_buf[21];
	lcd_str(ltoa(number, ltoa_buf, 10));
}
void lcd_ulong(const unsigned long number)
{
	char ultoa_buf[21];
	lcd_str(ultoa(number, ultoa_buf, 10));
}
#endif

#if USE_LCD_HEX
void lcd_hex(const int number)
{
	char itoa_buf[LCD_COLS+1];
	lcd_str(itoa(number, itoa_buf, 16));
}
#endif

#if USE_LCD_FLOAT
void lcd_float(const float number, const uint8_t decimals)
{
	lcd_double((const double)number, decimals);
}
void lcd_double(const double number, const uint8_t decimals)
{
	char double_buf[LCD_COLS];
	dtostrf(number, -LCD_COLS, decimals, double_buf);
	for (uint i = 0; i < LCD_COLS; i++)
	{
		if (double_buf[i] == ' ')
		{
			double_buf[i] = '\0';
			break;
		}
	}
	lcd_str(double_buf);
}
#endif

#if USE_LCD_DEFCHAR
void lcd_defchar(uint8_t nr, uint8_t * def_char)
{
	register uint8_t i, _char;
	lcd_cmd((LCD_COMMAND)(64 + (nr & 0x07) * 8));
	for (i = 0; i < 8; i++)
	{
		_char = *def_char;
		def_char++;
		lcd_data(_char);
	}
}
#endif

#if USE_LCD_DEFCHAR_P
void lcd_defchar_P(uint8_t nr, uint8_t * def_char)
{
	register uint8_t i, _char;
	lcd_cmd((LCD_COMMAND)(64 + (nr & 0x07) * 8));
	for (i = 0; i < 8; i++)
	{
		_char = pgm_read_byte(def_char);
		def_char++;
		lcd_data(_char);
	}
}
#endif

#if USE_LCD_DEFCHAR_E
void lcd_defchar_E(uint8_t nr, uint8_t * def_char)
{
	register uint8_t i, _char;
	lcd_cmd((LCD_COMMAND)(64 + (nr & 0x07) * 8));
	for (i = 0; i < 8; i++)
	{
		_char = eeprom_read_byte(def_char);
		def_char++;
		lcd_data(_char);
	}
}
#endif

