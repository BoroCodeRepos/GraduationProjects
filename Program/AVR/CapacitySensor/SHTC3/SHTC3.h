/*
 * SHTC3.h
 *
 *  Created on: 24 lut 2022
 *      Author: Arek
 */

#ifndef SHTC3_SHTC3_H_
#define SHTC3_SHTC3_H_

#include <avr/io.h>

#include "../common.h"
#include "../TWI/master.h"

#define SHT_ADDR 0x70

typedef enum
{
	SHT_SOFTRESET = 0x805D,
	SHT_READID = 0xEFC8,
	SHT_WAKEUP = 0x3517,
	SHT_SLEEP = 0xB098,
} SHT_COMMAND_t;

typedef enum
{
	SHT_NORMAL_MEAS_TFIRST_STRETCH = 0x7CA2,
	SHT_LOWPOW_MEAS_TFIRST_STRETCH = 0x6458,
	SHT_NORMAL_MEAS_HFIRST_STRETCH = 0x5C24,
	SHT_LOWPOW_MEAS_HFIRST_STRETCH = 0x44DE,

	SHT_NORMAL_MEAS_TFIRST = 0x7866,
	SHT_LOWPOW_MEAS_TFIRST = 0x609C,
	SHT_NORMAL_MEAS_HFIRST = 0x58E0,
	SHT_LOWPOW_MEAS_HFIRST = 0x401A,
} SHT_MEAS_MODE_t;

typedef enum
{
	SHT_Status_Nominal = 0,
	SHT_Status_Error,
	SHT_Status_CRC_Fail,
	SHT_Status_ID_Fail
} SHT_STATUS_t;

typedef struct
{
	SHT_STATUS_t status;
	bool low_pwr;

	double T;
	double RH;
} SHT_SR_t;

extern void sht_low_pwr(bool value);
extern SHT_STATUS_t sht_checkID(void);
extern SHT_STATUS_t sht_init(void);
extern SHT_STATUS_t sht_meas(void);

extern double sht_temperature(void);
extern double sht_humidity(void);


#endif /* SHTC3_SHTC3_H_ */
