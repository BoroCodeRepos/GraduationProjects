#ifndef MAIN_H_
#define MAIN_H_

/* Including Libraries */
#include <avr/io.h>
#include <avr/wdt.h>
#include <avr/power.h>
#include <avr/eeprom.h>
#include <avr/pgmspace.h>
#include <avr/interrupt.h>
#include <util/delay.h>
#include <stdio.h>
/* Including Config Files */
#include "common.h"
#include "system.h"
/* Including Own Libraries */
#include "TWI/master.h"
#include "LCD/HD44780.h"
#include "SHTC3/SHTC3.h"
#include "RingBuffer/RingBuffer.h"
/* USB Libraries */
#include "Descriptors.h"
#include "LUFA/Version.h"
#include "LUFA/Drivers/USB/USB.h"
#include "LUFA/Platform/Platform.h"

/* Definitions */
#define ENABLE_NOISE_CANCELER	1		// Enable noise canceler - Input Capture Mod
#define USE_DEFAULT_CONSTANTS 	1		// Defining Defaults Constants Values in EEPROM
#define USE_DEFAULT_CORRECTIONS 1		// Defining Defaults Corrections Values in EEPROM
#define MAX_USB_BUF_SIZE 		128		// USB Buffer Size
#define PRECISION				0.1		// Temperature and Humidity results precision
#define AFTERPOINT  			1		// Temperature and Humidity results after points
#define ACCURACY				4		// Accuracy of Constants Values
#define ENDCMD					'\n'	// End of Command char
#define GENERATION_FREQ			1E6		// Generator Frequency in Calibration moment - Hz unit (8 MHz - 32 kHz)

/*
 Timer 0 Calculations of Generator Work
 fclk / (fosc * 2N) - 1
 N = Prescaler value = 1
*/
#define OCR_VALUE 		((byte)((8E6 / GENERATION_FREQ) - 1))

/* System Struct Declaration */
volatile SYSTEM_t System;
/* Constants Struct Declaration */
static CONSTANTS_t Constants;
/* Corrections Struct Declaration */
static CORRECTIONS_t Corrections;
/* Streams to USB and LCD */
static FILE USB_Stream, LCD_Stream;
/* USB Buffer variables */
static byte Buffer[MAX_USB_BUF_SIZE];
static RingBuffer_t USB_Buffer;
/* CMD Buffer */
static byte CMD_Buffer[MAX_USB_BUF_SIZE];

/* System Initialization Functions */
static inline void InitApp(void);
static inline void Init_IO(void);
static inline void Init_Pheripherals(void);
static inline void Init_Variables(void);
static inline void Init_USB(void);
static inline void Init_InputCapture(void);
static inline void Init_Message(void);

/* Accumulating & Parsing Data */
static inline void USB_ReceiveData(void);
static inline void CMD_Parse(void);

/* Capacity Measuring Function */
static inline STATUS_t CapacityMeasurement(void);
/* Temperature & Humidity Measuring Function */
static inline STATUS_t TemperatureMeasurement(void);

/* Generating Signal Functions */
static inline void EnableGenerations(void);
static inline void DisableGenerations(void);
/* Input Capturing Functions */
static inline void EnableInputCapture(void);
static inline void DisableInputCapture(void);

/* Base Conversions Functions */
static double Round(double N, double Precision);
static char * dtostr(double N, byte AfterPoint);

/* USB Events Function Prototypes */
void EVENT_USB_Device_Connect(void);
void EVENT_USB_Device_Disconnect(void);
void EVENT_USB_Device_ConfigurationChanged(void);
void EVENT_USB_Device_ControlRequest(void);

#if USE_DEFAULT_CONSTANTS
const CONSTANTS_t EEMEM Constants_EEMEM =
{
	.H_THR   = 3.3500,
	.L_THR   = 1.6770,
	.H_VOUT  = 5.0230,
	.L_VOUT  = 0.0000,
	.R_MEAS  = 191100,
};
#else
const CONSTANTS_t EEMEM Constants_EEMEM;
#endif

const CONSTANTS_t PROGMEM Constants_PROGMEM =
{
	.H_THR   = 3.3500,
	.L_THR   = 1.6770,
	.H_VOUT  = 5.0230,
	.L_VOUT  = 0.0000,
	.R_MEAS  = 191100,
};

#if USE_DEFAULT_CORRECTIONS
const CORRECTIONS_t EEMEM Corrections_EEMEM =
{
	.A0 =  3.2231,
	.A1 = -4.4837,
	.A2 =  2.7930,
	.A3 = -4.6443,
};
#else
const CORRECTIONS_t EEMEM Corrections_EEMEM;
#endif

const CORRECTIONS_t PROGMEM Corrections_PROGMEM =
{
	.A0 =  3.2231,
	.A1 = -4.4837,
	.A2 =  2.7930,
	.A3 = -4.6443,
};

/* USB ClassInfo Implementation */
USB_ClassInfo_CDC_Device_t VirtualSerial_CDC_Interface =
{
	.Config =
	{
		.ControlInterfaceNumber   = INTERFACE_ID_CDC_CCI,
		.DataINEndpoint           =
		{
			.Address          = CDC_TX_EPADDR,
			.Size             = CDC_TXRX_EPSIZE,
			.Banks            = 1,
		},
		.DataOUTEndpoint =
		{
			.Address          = CDC_RX_EPADDR,
			.Size             = CDC_TXRX_EPSIZE,
			.Banks            = 1,
		},
		.NotificationEndpoint =
		{
			.Address          = CDC_NOTIFICATION_EPADDR,
			.Size             = CDC_NOTIFICATION_EPSIZE,
			.Banks            = 1,
		},
	},
};


#endif /* MAIN_H_ */
