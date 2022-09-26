/*
 * master.c
 *
 *  Created on: 24 lut 2022
 *      Author: Arek
 */

#include "master.h"


static TWI_SR_t TWI_SR;

#define TW_CHECK_ERROR_STATUS		\
		if (TWI_SR.error)	{ return; }

#define TW_CHECK_ERROR_STATUS_RET	\
		if (TWI_SR.error)   { return 0; }

void twi_init(TWI_SCL_FREQ_t freq)
{
	// init status register
	TWI_SR.error = false;
	TWI_SR.status = 0;

	twi_set_bit_rate(freq);
}

void twi_set_bit_rate(TWI_SCL_FREQ_t freq)
{
	uint temp = ((F_CPU / freq) - 15);
	TWBR = temp / 2 + temp % 2;
}

bool twi_error()
{
	bool err = TWI_SR.error;
	TWI_SR.error = false;
	return err;
}

byte twi_status(void)
{
	return TWI_SR.status;
}

void twi_start(void)
{
	TW_CHECK_ERROR_STATUS;

	TWCR = _BV(TWINT) | _BV(TWEN) | _BV(TWSTA);

	loop_until_bit_is_set(TWCR, TWINT);

#if TW_CHECK_STATUS
	if (TW_STATUS != TW_START && TW_STATUS != TW_REP_START)
	{
		TWI_SR.error = true;
		TWI_SR.status = TW_STATUS;
	}
#endif
}

void twi_stop(void)
{
	TWCR = _BV(TWINT) | _BV(TWEN) | _BV(TWSTO);
}

void twi_write_sla(byte SLA)
{
	TW_CHECK_ERROR_STATUS;

	TWDR = SLA;
	TWCR = _BV(TWINT) | _BV(TWEN);

	loop_until_bit_is_set(TWCR, TWINT);

	if (TW_STATUS != TW_MT_SLA_ACK && TW_STATUS != TW_MR_SLA_ACK)
	{
		TWI_SR.error = true;
		TWI_SR.status = TW_STATUS;
	}
}

bool twi_detect(byte SLA)
{
	twi_start();
	twi_write_sla(TW_SLA_W(SLA));
	twi_stop();

	return !twi_error();
}

void twi_write(byte data)
{
	TW_CHECK_ERROR_STATUS;

	TWDR = data;
	TWCR = _BV(TWINT) | _BV(TWEN);

	loop_until_bit_is_set(TWCR, TWINT);

#if TW_CHECK_STATUS
	if (TW_STATUS != TW_MT_DATA_ACK)
	{
		TWI_SR.error = true;
		TWI_SR.status = TW_STATUS;
	}
#endif
}

byte twi_read(bool ACK)
{
	TW_CHECK_ERROR_STATUS_RET;

	TWCR = _BV(TWINT) | _BV(TWEN) | (ACK << TWEA);

	loop_until_bit_is_set(TWCR, TWINT);

	if (TW_STATUS != ( ACK ? TW_MR_DATA_ACK : TW_MR_DATA_NACK ))
	{
		TWI_SR.error = true;
		TWI_SR.status = TW_STATUS;
	}

	return TWDR;
}

void twi_write_buf(byte SLA, byte addr, byte * data_ptr, uint len, bool repeat_start)
{
	twi_start();
	twi_write_sla(TW_SLA_W(SLA));
	twi_write(addr);

	while (len)
	{
		len--;
		twi_write(*data_ptr);
		data_ptr++;
	}

	if (!repeat_start)
	{
		twi_stop();
	}
}

void twi_read_buf(byte SLA, byte addr, byte * data_ptr, uint len)
{
	twi_start();
	twi_write_sla(TW_SLA_W(SLA));
	twi_write(addr);
	twi_start();
	twi_write_sla(TW_SLA_R(SLA));

	while (len)
	{
		len--;
		*data_ptr = twi_read( len ? ACK : NACK );
		data_ptr++;
	}

	twi_stop();
}
