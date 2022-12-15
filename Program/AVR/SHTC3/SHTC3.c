#include "SHTC3.h"

static SHT_SR_t SHTSR;

static inline void sht_calcT(uint16_t temperature);
static inline void sht_calcT(uint16_t temperature)
{
	SHTSR.T = (double)temperature * 175. / 65535. - 45.;
}

static inline void sht_calcRH(uint16_t humidity);
static inline void sht_calcRH(uint16_t humidity)
{
	SHTSR.RH = (double)humidity / 65535. * 100.;
}

static bool sht_checkCRC(byte upper, byte lower, byte CRC);
static bool sht_checkCRC(byte upper, byte lower, byte CRC)
{
	byte data[2] = { upper, lower };
	byte tempCRC = 0xFF;
	byte poly = 0x31;

	for (byte i = 0; i < 2; i++)
	{
		tempCRC ^= data[i];

		for (byte j = 0; j < 8; j++)
		{
			if (tempCRC & 0x80)
			{
				tempCRC = (byte)((tempCRC << 1) ^ poly);
			}
			else
			{
				tempCRC <<= 1;
			}
		}
	}

	if (tempCRC ^ CRC)
	{
		return false;
	}

	return true;
}

static void sht_command(SHT_COMMAND_t cmd);
static void sht_command(SHT_COMMAND_t cmd)
{
	twi_start();
	twi_write_sla(TW_SLA_W(SHT_ADDR));
	twi_write(MSB(cmd));
	twi_write(LSB(cmd));
	twi_stop();
}

static inline void sht_sleep(void);
static inline void sht_sleep(void)
{
	sht_command(SHT_SLEEP);
	_delay_us(240);
}

static inline void sht_wakeup(void);
static inline void sht_wakeup(void)
{
	sht_command(SHT_WAKEUP);
	_delay_us(240);
}

static inline void sht_mode(void);
static inline void sht_mode(void)
{
	if (SHTSR.low_pwr)
	{
		sht_command((SHT_COMMAND_t)SHT_LOWPOW_MEAS_TFIRST);
		_delay_ms(1);
	}
	else
	{
		sht_command((SHT_COMMAND_t)SHT_NORMAL_MEAS_TFIRST);
		_delay_ms(15);
	}
}

void sht_low_pwr(bool value)
{
	SHTSR.low_pwr = value;
}

SHT_STATUS_t sht_checkID(void)
{
	sht_command(SHT_READID);

	twi_start();
	twi_write_sla(TW_SLA_R(SHT_ADDR));
	byte ID_MSB = twi_read(ACK);
	byte ID_LSB = twi_read(ACK);
	byte ID_CRC = twi_read(ACK);
	twi_stop();

	uint16_t ID = MERGE(ID_MSB, ID_LSB);

	if (sht_checkCRC(ID_MSB, ID_LSB, ID_CRC))
	{
		if ((ID & 0x083F) != 0x0807)
		{
			return SHT_Status_ID_Fail;
		}

		return SHT_Status_Nominal;
	}

	return SHT_Status_Error;
}

SHT_STATUS_t sht_init(void)
{
	byte retval = 0;
	sht_wakeup();
	retval = sht_checkID();
	sht_sleep();
	sht_low_pwr(false);
	return retval;
}

SHT_STATUS_t sht_meas(void)
{
	sht_wakeup();
	sht_mode();

	twi_start();
	twi_write_sla(TW_SLA_R(SHT_ADDR));
	byte T_MSB = twi_read(ACK);
	byte T_LSB = twi_read(ACK);
	byte T_CRC = twi_read(ACK);
	byte H_MSB = twi_read(ACK);
	byte H_LSB = twi_read(ACK);
	byte H_CRC = twi_read(ACK);
	twi_stop();

	sht_sleep();

	if (sht_checkCRC(T_MSB, T_LSB, T_CRC) && sht_checkCRC(H_MSB, H_LSB, H_CRC))
	{
		sht_calcT(MERGE(T_MSB, T_LSB));
		sht_calcRH(MERGE(H_MSB, H_LSB));
		return SHT_Status_Nominal;
	}

	SHTSR.T = -45.;
	SHTSR.RH = 0.;
	return SHT_Status_CRC_Fail;
}

double sht_temperature(void)
{
	return SHTSR.T;
}

double sht_humidity(void)
{
	return SHTSR.RH;
}








