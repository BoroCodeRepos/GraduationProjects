#ifndef SYSTEM_H_
#define SYSTEM_H_

#include "common.h"

/* OUTPUTS */
#define SIG_GEN_PORT 	 B
#define SIG_WORK_PORT	 B
#define SIG_GEN		  	PB7
#define SIG_WORK		PB6

/* INPUTS */
#define SIG_S_THR_PORT   D
#define SIG_H_THR_PORT   D
#define SIG_L_THR_PORT   D
#define SIG_PWR_PORT	 D
#define SIG_S_THR		PD4
#define SIG_H_THR       PD2
#define SIG_L_THR		PD3
#define SIG_PWR			PD6

/* LEDS STATE MANAGEMENT */
#define LED1_ON 		sbi(PORTC, PC7)
#define LED1_OFF		cbi(PORTC, PC7)
#define LED2_ON 		sbi(PORTD, PD5)
#define LED2_OFF		cbi(PORTD, PD5)
#define LED3_ON 		sbi(PORTB, PB0)
#define LED3_OFF		cbi(PORTB, PB0)

/* LEDS Macros */
#define LEDS_OFF	\
	LED1_OFF;		\
	LED2_OFF;		\
	LED3_OFF

/* Command Received Signalization LED */
#define LED_CMD_REC_ON	 		LED1_ON
#define LED_CMD_REC_OFF 		LED1_OFF
/* USB Communication LED */
#define LED_USB_CONN			LED2_ON
#define LED_USB_DISCONN			LED2_OFF
#define LED_USB_ERROR		 	LED3_OFF
#define LED_USB_READY        	LED3_ON

/* PINS DIRECTION */
#define SET_PORTS_DIR						\
	sbi(DDR(SIG_WORK_PORT), SIG_WORK);		\
	sbi(DDR(SIG_GEN_PORT), SIG_GEN);		\
	sbi(DDRC, PC7);							\
	sbi(DDRD, PD5);							\
	sbi(DDRB, PB0)

/* INTERNAL PULL-UPS */
#define SET_INTERNAL_PULLUPS				\
	sbi(PORT(SIG_S_THR_PORT), SIG_S_THR);	\
	sbi(PORT(SIG_H_THR_PORT), SIG_H_THR);	\
	sbi(PORT(SIG_L_THR_PORT), SIG_L_THR)

/* BASIC OPERATION */
#define MEASURING_SYSTEM_ON		cbi(PORT(SIG_WORK_PORT), SIG_WORK)
#define MEASURING_SYSTEM_OFF	sbi(PORT(SIG_WORK_PORT), SIG_WORK)
#define SET_GEN_STATE			sbi(PORT(SIG_GEN_PORT), SIG_GEN)
#define CLR_GEN_STATE			cbi(PORT(SIG_GEN_PORT), SIG_GEN)
#define TOG_GEN_STATE			tbi(PORT(SIG_GEN_PORT), SIG_GEN)
#define PWR_STATUS 				( bit_is_set(PIN(SIG_PWR_PORT), SIG_PWR) )
#define V_CAP_IS_UNDER_L_THR	( bit_is_set(PIN(SIG_H_THR_PORT), SIG_H_THR) && bit_is_clear(PIN(SIG_L_THR_PORT), SIG_L_THR) )

/* COMMAND MACROS */
#define	SEND_CONSTANT_VALUES													\
	fprintf_P(&USB_Stream, PSTR("%s "), dtostr(Constants.H_THR,  ACCURACY));	\
	fprintf_P(&USB_Stream, PSTR("%s "), dtostr(Constants.L_THR,  ACCURACY));	\
	fprintf_P(&USB_Stream, PSTR("%s "), dtostr(Constants.H_VOUT, ACCURACY));	\
	fprintf_P(&USB_Stream, PSTR("%s "), dtostr(Constants.L_VOUT, ACCURACY));	\
	fprintf_P(&USB_Stream, PSTR("%lu\r\n"), Constants.R_MEAS)

#define	SEND_CORRECTION_VALUES													\
	fprintf_P(&USB_Stream, PSTR("%s "), 	dtostr(Corrections.A0, ACCURACY));	\
	fprintf_P(&USB_Stream, PSTR("%s "), 	dtostr(Corrections.A1, ACCURACY));	\
	fprintf_P(&USB_Stream, PSTR("%s "), 	dtostr(Corrections.A2, ACCURACY));	\
	fprintf_P(&USB_Stream, PSTR("%s\r\n"),  dtostr(Corrections.A3, ACCURACY))

#define SEND_CONFIRMATION 		CDC_Device_SendString_P(&VirtualSerial_CDC_Interface, PSTR("OK\r\n"));

/* EEPROM OPERATIONS */
#define READ_CONSTANTS__EEMEM				\
	eeprom_read_block((void*)&Constants, (const void*)&Constants_EEMEM, sizeof(CONSTANTS_t))
#define UPDATE_CONSTANTS__EEMEM				\
	eeprom_update_block((const void*)&Constants, (void*)&Constants_EEMEM, sizeof(CONSTANTS_t))
#define READ_CONSTANTS__PGM					\
	memcpy_P((void*)&Constants, (const void*)&Constants_PROGMEM, sizeof(CONSTANTS_t))

#define READ_CORRECTIONS__EEMEM				\
	eeprom_read_block((void*)&Corrections, (const void*)&Corrections_EEMEM, sizeof(CORRECTIONS_t))
#define UPDATE_CORRECTIONS__EEMEM				\
	eeprom_update_block((const void*)&Corrections, (void*)&Corrections_EEMEM, sizeof(CORRECTIONS_t))
#define READ_CORRECTIONS__PGM					\
	memcpy_P((void*)&Corrections, (const void*)&Corrections_PROGMEM, sizeof(CORRECTIONS_t))

#define MAX_SAMPLES_BUF_SIZE 130

typedef enum
{
	Status_OK,
	Status_Error,
	Status_PowerError,
	Status_Unregistered,
	Status_DischargeTimeout,
	Status_MeasurementTimeout,
} STATUS_t;

typedef struct
{
	bool IsLCDRegistered  : 1;	// LCD connection flag
	bool IsSHTRegistered  : 1;	// SHT connection flag
	bool IsHostReady      : 1;  // USB host ready flag
	bool IsCMDReceived    : 1;	// USB cmd received flag
	bool IsMeasurementEnd : 1;	// Measurement end flag

	uint16_t Samples[MAX_SAMPLES_BUF_SIZE];
	uint8_t SampleIdx;

	double Temperature;
	double Humidity;
} SYSTEM_t;

typedef struct
{
	double H_THR;
	double L_THR;
	double H_VOUT;
	double L_VOUT;
	uint32_t R_MEAS;
} CONSTANTS_t;

typedef struct
{
	double A0;
	double A1;
	double A2;
	double A3;
} CORRECTIONS_t;

typedef enum
{
	// From Host
	GET_CONSTANTS 	= 'A',    	// Get Constants Values
	SET_CONSTANTS 	= 'S',    	// Set Constants Values
	DEF_CONSTANTS 	= 'D',    	// Set Default Constants Values
	GET_CORRECTIONS = 'I',  	// Get Corrections Values
	SET_CORRECTIONS = 'O',  	// Set Corrections Values
	DEF_CORRECTIONS = 'P',  	// Set Default Corrections Values
	SET_L_VOUT 		= 'L',      // Set Signal Pin as LOW
	SET_H_VOUT 		= 'H',      // Set Signal Pin as HIGH
	SET_GENERATIONS = 'G',  	// Set Generations on Signal Pin
	SET_NOMINAL 	= 'N',      // Set Signal Pin as NOMINAL (HIGH-Z)
	TRIGGER_MEAS 	= 'M',     	// Trigger Measurement
	GET_TEMP_RH 	= 'R',      // Read Temperature and Humidity
	LCD_AFTERMEAS 	= 'W',    	// Send Calculated Data To Display on LCD
	// To Host
	SEND_TEMP	  	= 'T',		// Send Temperature
	SEND_RH		  	= 'H',		// Send Humidity
	SEND_SP		  	= 'C',		// Send Samples
} COMMAND_t;

#endif /* SYSTEM_H_ */
