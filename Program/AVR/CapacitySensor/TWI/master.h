/*
 * master.h
 *
 *  Created on: 24 lut 2022
 *      Author: Arek
 */

#ifndef TWI_MASTER_H_
#define TWI_MASTER_H_

#include <avr/io.h>
#include <util/twi.h>

#include "../common.h"

#define TW_SLA_W(addr) ((addr << 1) | TW_WRITE)
#define TW_SLA_R(addr) ((addr << 1) | TW_READ)

typedef enum
{
	TWI_FREQ_100kHz = 100000,
	TWI_FREQ_250kHz = 250000,
	TWI_FREQ_400kHz = 400000,
	TWI_FREQ_800kHz = 800000,
	TWI_FREQ_1MHz   = 1000000,
} TWI_SCL_FREQ_t;

typedef enum
{
	NACK = 0,
	ACK  = 1,
} TWI_ACK_t;

typedef struct
{
	bool error;
	byte status;
} TWI_SR_t;

extern void twi_init(TWI_SCL_FREQ_t freq);
extern void twi_set_bit_rate(TWI_SCL_FREQ_t freq);

extern void twi_start(void);
extern void twi_stop(void);
extern void twi_write_sla(byte SLA);
extern void twi_write(byte data);
extern byte twi_read(bool ack);

extern void twi_write_buf(byte SLA, byte addr, byte * data_ptr, uint len, bool repeat_start);
extern void twi_read_buf(byte SLA, byte addr, byte * data_ptr, uint len);

extern bool twi_detect(byte SLA);

extern bool twi_error(void);
extern byte twi_status(void);

#endif /* TWI_MASTER_H_ */
