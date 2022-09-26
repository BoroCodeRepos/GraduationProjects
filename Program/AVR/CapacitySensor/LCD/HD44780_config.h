/*
 * HD44780_config.h
 *
 *  Created on: 1 lut 2022
 *      Author: Arek
 */

#ifndef LCD_HD44780_CONFIG_H_
#define LCD_HD44780_CONFIG_H_

#define LCD_ROWS 			 4
#define LCD_COLS 			20

#define USE_TWI				 1
#define USE_RW 				 0
#define USE_BACKLIGHT		 1
#define USE_AL_FUNC			 1

#define USE_LCD_STREAM		 1
#define USE_LCD_STR_P   	 1
#define USE_LCD_STR_E   	 0
#define USE_LCD_INT     	 0
#define USE_LCD_FLOAT        1
#define USE_LCD_HEX     	 0
#define USE_LCD_DEFCHAR 	 0
#define USE_LCD_DEFCHAR_P    0
#define USE_LCD_DEFCHAR_E    0
#define USE_LCD_CURSOR_ON    0
#define USE_LCD_CURSOR_BLINK 0
#define USE_LCD_CURSOR_HOME  0

#if !USE_TWI
	#define LCD_E_PORT   	A
	#define LCD_RS_PORT		A
	#define LCD_D4_PORT		A
	#define LCD_D5_PORT		A
	#define LCD_D6_PORT		A
	#define LCD_D7_PORT		A

	#define LCD_E  			PA2
	#define LCD_RS 			PA1
	#define LCD_D4 			PA4
	#define LCD_D5 			PA5
	#define LCD_D6 			PA6
	#define LCD_D7 			PA7

#if USE_RW
	#define LCD_RW_PORT		A
	#define LCD_RW 			PA0
#endif

#else
	#define LCD_ADDR		0x27

	#define LCD_RS 			0
	#define LCD_RW			1
	#define LCD_E  			2
	#define LCD_D4 			4
	#define LCD_D5 			5
	#define LCD_D6 			6
	#define LCD_D7 			7

#if USE_BACKLIGHT
	#define LCD_BACKLIGHT   3
#endif

#endif

#endif /* LCD_HD44780_CONFIG_H_ */
