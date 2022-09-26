/*
 * types.h
 *
 *  Created on: 1 lut 2022
 *      Author: Arek
 */

#ifndef COMMON_H_
#define COMMON_H_

// ---- Configuration
#define USE_STDBOOL 1
// ---- Including useful libraries
#include <avr/io.h>
#include <avr/sfr_defs.h>
#include <avr/sleep.h>
#include <avr/pgmspace.h>
#include <avr/eeprom.h>
#include <avr/interrupt.h>
#include <util/delay.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>


#if USE_STDBOOL
	#include <stdbool.h>
#else
	typedef enum { false = 0, true = 1 } bool;
#endif

// ---- typedefs
typedef uint8_t byte;
typedef uint16_t uint;
typedef unsigned char uchar;

// ---- Defs simplification macros
#define PORT(X)  SPORT(X)
#define PIN(X)   SPIN(X)
#define DDR(X)   SDDR(X)
#define SPORT(X) (PORT##X)
#define SPIN(X)  (PIN##X)
#define SDDR(X)  (DDR##X)

#define NO_STATEMENT

#define tbi(reg, bit) (reg ^=  _BV(bit))
#define sbi(reg, bit) (reg |=  _BV(bit))
#define cbi(reg, bit) (reg &= _NBV(bit))

#define _NBV(bit) (~_BV(bit))

#define MSB(x) ((byte)(x >> 8))
#define LSB(x) ((byte)(x & 0xFF))
#define MERGE(high, low) (uint16_t)((high << 8) | low)

#endif /* COMMON_H_ */
