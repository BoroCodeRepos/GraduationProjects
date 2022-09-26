#ifndef LCD_HD44780_H_
#define LCD_HD44780_H_

#include "../common.h"
#include "HD44780_config.h"

#if USE_TWI
	#include "../TWI/master.h"
#endif

#if USE_LCD_STREAM
	#include <stdio.h>
#endif

typedef enum
{
	CLEAR = 0x01,
	HOME = 0x02,

	ENTRY_MODE_SHIFT_CURSOR_INCREMENT = 0x06,
	ENTRY_MODE_SHIFT_CURSOR_DECREMENT = 0x04,
	ENTRY_MODE_SHIFT_DISPLAY_INCREMENT = 0x07,
	ENTRY_MODE_SHIFT_DISPLAY_DECREMENT = 0x05,

	DISPLAY_ON_CURSOR_ON_NOBLINK = 0x0C,
	DISPLAY_ON_CURSOR_ON_BLINK = 0x0D,
	DISPLAY_ON_CURSOR_OFF = 0x0C,
	DISPLAY_OFF = 0x08,

	SHIFT_CURSOR_LEFT = 0x10,
	SHIFT_CURSOR_RIGHT = 0x14,
	SHIFT_DISPLAY_LEFT = 0x18,
	SHIFT_DISPLAY_RIGHT = 0x1C,

	FONT5x7_1LINE_4BIT = 0x20,
	FONT5x7_2LINE_4BIT = 0x28,
	FONT5x10_1LINE_4BIT = 0x24,
	FONT5x10_2LINE_4BIT = 0x2C,

	CGRAM_SET = 0x40,
	DDRAM_SET = 0x80,
} LCD_COMMAND;



#define CLR_E cbi(PORT(LCD_E_PORT), LCD_E)
#define SET_E sbi(PORT(LCD_E_PORT), LCD_E)

#define CLR_RS cbi(PORT(LCD_RS_PORT), LCD_RS)
#define SET_RS sbi(PORT(LCD_RS_PORT), LCD_RS)

#if USE_RW
	#define CLR_RW cbi(PORT(LCD_RW_PORT), LCD_RW)
	#define SET_RW sbi(PORT(LCD_RW_PORT), LCD_RW)
#endif

#if (LCD_ROWS == 4 && LCD_COLS == 20)
	#define LCD_LINE0 0x00
	#define LCD_LINE1 0x28
	#define LCD_LINE2 0x14
	#define LCD_LINE3 0x54
#else
	#define LCD_LINE0 0x00
	#define LCD_LINE1 0x40
	#define LCD_LINE2 0x10
	#define LCD_LINE3 0x50
#endif

#if USE_AL_FUNC
#define lcd_str_al(Row, Col, String)    \
	lcd_locate(Row, Col);				\
	lcd_str(String)

#if USE_LCD_STR_P
#define lcd_str_al_P(Row, Col, String)	\
	lcd_locate(Row, Col);				\
	lcd_str_P(String)
#endif

#if USE_LCD_STR_E
#define	lcd_str_al_E(Row, Col, String)	\
	lcd_locate(Row, Col);				\
	lcd_str_E(String)
#endif

#if USE_LCD_HEX
#define lcd_hex_al(Row, Col, Hex)		\
	lcd_locate(Row, Col);				\
	lcd_hex(Hex)
#endif

#if USE_LCD_INT
#define lcd_int_al(Row, Col, Int)		\
	lcd_locate(Row, Col);				\
	lcd_int(Int)

#define lcd_uint_al(Row, Col, Uint)		\
	lcd_locate(Row, Col);				\
	lcd_uint(Uint)

#define lcd_long_al(Row, Col, Long)		\
	lcd_locate(Row, Col);				\
	lcd_long(Long)

#define lcd_ulong_al(Row, Col, Ulong)	\
	lcd_locate(Row, Col);				\
	lcd_ulong(Ulong)
#endif

#if USE_LCD_FLOAT
#define lcd_float_al(Row, Col, Float, Decimals)			\
	lcd_locate(Row, Col);								\
	lcd_float(Float, Decimals)

#define lcd_double_al(Row, Col, Double, Decimals)		\
	lcd_locate(Row, Col);								\
	lcd_double(Double, Decimals)
#endif

#endif

#if USE_RW
	extern byte lcd_get_address();
	extern byte lcd_get_data();
#endif

extern void lcd_init(void);
extern void lcd_cls(void);
extern void lcd_locate(byte row, byte column);
extern void lcd_str(const char * text);
extern void lcd_char(const char _char);

#if USE_LCD_STREAM
	extern void lcd_create_stream(FILE* Stream);
#endif

#if USE_LCD_CURSOR_HOME
	extern void lcd_home(void);
#endif

#if USE_LCD_CURSOR_ON
	extern void lcd_cursor_on(void);
	extern void lcd_cursor_off(void);
#endif

#if USE_LCD_CURSOR_BLINK
	extern void lcd_blink_on(void);
	extern void lcd_blink_off(void);
#endif

#if USE_LCD_STR_P
	extern void lcd_str_P(const char * text);
#endif

#if USE_LCD_STR_E
	extern void lcd_str_E(const char * text);
#endif

#if USE_LCD_DEFCHAR
	extern void lcd_defchar(uint8_t nr, uint8_t * def_char);
#endif

#if USE_LCD_DEFCHAR_P
	extern void lcd_defchar_P(uint8_t nr, uint8_t * def_char);
#endif

#if USE_LCD_DEFCHAR_E
	extern void lcd_defchar_E(uint8_t nr, uint8_t * def_char);
#endif

#if USE_LCD_HEX
	extern void lcd_hex(const int number);
#endif

#if USE_LCD_INT
	extern void lcd_int(const int number);
	extern void lcd_uint(const unsigned number);
	extern void lcd_long(const long number);
	extern void lcd_ulong(const unsigned long number);
#endif

#if USE_LCD_FLOAT
	extern void lcd_float(const float number, const uint8_t decimals);
	extern void lcd_double(const double number, const uint8_t decimals);
#endif

#if USE_BACKLIGHT
	extern void lcd_backlight_on(void);
	extern void lcd_backlight_off(void);
#endif

#endif /* LCD_HD44780_H_ */
