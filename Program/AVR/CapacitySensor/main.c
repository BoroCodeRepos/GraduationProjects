#include "main.h"

/* Main Function */
int main(void)
{
	/* App Initialization */
	InitApp();
	GlobalInterruptEnable();

	while (true)
	{
		/* Check & Accumulate Available Data from USB */
		USB_ReceiveData();
		/* Parse Received Commands from Host */
		CMD_Parse();
		/* USB Communication Required Tasks */
		CDC_Device_USBTask(&VirtualSerial_CDC_Interface);
		USB_USBTask();
	}

	return 0;
}

/* Appication init */
void InitApp(void)
{
	Init_IO();
	Init_Pheripherals();
	Init_Variables();
	Init_USB();
	Init_InputCapture();
	Init_Message();
}
/* Base Initialization Functions */
void Init_IO(void)
{
	/* IO initialization */
	MEASURING_SYSTEM_OFF;
	LEDS_OFF;
	SET_INTERNAL_PULLUPS;
	SET_PORTS_DIR;
}
void Init_Pheripherals(void)
{
	twi_init(TWI_FREQ_400kHz);
	System.IsLCDRegistered = twi_detect(LCD_ADDR);
	System.IsSHTRegistered = twi_detect(SHT_ADDR);

	/* Device initialization */
	if (System.IsSHTRegistered)
	{
		sht_init();
		LED2_ON;
	}
	if (System.IsLCDRegistered)
	{
		lcd_init();
		lcd_create_stream(&LCD_Stream);
		LED3_ON;
	}
}
void Init_Variables(void)
{
	/* Buffors initialization */
	for (byte i = 0; i < MAX_SAMPLES_BUF_SIZE; i++)
	{
		System.Samples[i] = 0;
	}
	/* Variables initialization */
	System.IsCMDReceived = false;
	System.IsMeasurementEnd = false;
	System.IsHostReady  = false;
	System.Temperature = -45.0;
	System.Humidity = 0.0;
	RingBuffer_Init(&USB_Buffer, Buffer, MAX_USB_BUF_SIZE);
	/* Constants Values from EEPROM */
	READ_CONSTANTS__EEMEM;
	/* Corrections Values from EEPROM */
	READ_CORRECTIONS__EEMEM;
}
//READ_CONSTANTS__PGM;
//READ_CORRECTIONS__PGM;
void Init_USB(void)
{
	/* USB initialization */
	cbi(MCUSR, WDRF);
	wdt_disable();
	clock_prescale_set(clock_div_1);
	USB_Init();
	CDC_Device_CreateStream(&VirtualSerial_CDC_Interface, &USB_Stream);
}
void Init_InputCapture(void)
{
#if ENABLE_NOISE_CANCELER
	/* Input Capture Noise Canceler For ICP1 */
	sbi(TCCR1B, ICNC1);
#endif
	/* Input Capture Edge Select - Rising Edge */
	sbi(TCCR1B, ICES1);
}
void Init_Message(void)
{
	if (System.IsLCDRegistered)
	{
		lcd_str_al_P(0, 0, PSTR("********************"));
		lcd_str_al_P(1, 0, PSTR("*  CapacitySensor  *"));
		lcd_str_al_P(2, 0, PSTR("*  PG 2022 / 2023  *"));
		lcd_str_al_P(3, 0, PSTR("********************"));
	}
	_delay_ms(1000);
	LEDS_OFF;
	_delay_ms(500);
}

/* Accumulating & Parsing Data */
void USB_ReceiveData(void)
{
	static bool CMD_BufferOverflow = false;
	if (!RingBuffer_IsFull(&USB_Buffer))
	{
		int16_t USB_Received = CDC_Device_ReceiveByte(&VirtualSerial_CDC_Interface);
		/* Check Received Status */
		if (USB_Received >= 0)
		{
			/* Insert Received Data */
			byte Data = (byte)USB_Received;
			RingBuffer_Insert(&USB_Buffer, Data);
			if (Data == ENDCMD)
			{
				if (CMD_BufferOverflow)
				{
					RingBuffer_Clear(&USB_Buffer);
					CMD_BufferOverflow = false;
					LED_CMD_REC_OFF;
				}
				else
				{
					/* CMD Received Flag */
					System.IsCMDReceived = true;
					LED_CMD_REC_ON;
				}
			}
		}
	}
	else
	{
		/* Buffer Overflow Error */
		RingBuffer_Clear(&USB_Buffer);
		CMD_BufferOverflow = true;
		LED_CMD_REC_ON;
		fprintf_P(&USB_Stream, PSTR("Device Error: USB Buffer Overflow!\r\n"));
	}
}
void CMD_Parse(void)
{
	if (System.IsCMDReceived)
	{
		/* Clear Flag */
		System.IsCMDReceived = false;
		/* Size of Received Data */
		size_t Size = RingBuffer_Size(&USB_Buffer);

		if (Size == 2)
		{
			/* Primary Commands */
			RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ENDCMD);
			switch (CMD_Buffer[0])
			{
				case GET_CONSTANTS:		/* Get Constants Values */
				{
					SEND_CONSTANT_VALUES;
					break;
				}
				case DEF_CONSTANTS: 	/* Set Default Constants Values */
				{
					READ_CONSTANTS__PGM;
					UPDATE_CONSTANTS__EEMEM;
					SEND_CONFIRMATION;
					break;
				}
				case GET_CORRECTIONS:	/* Get Corrections Values */
				{
					SEND_CORRECTION_VALUES;
					break;
				}
				case DEF_CORRECTIONS:	/* Set Default Corrections Values */
				{
					READ_CORRECTIONS__PGM;
					UPDATE_CORRECTIONS__EEMEM;
					SEND_CONFIRMATION;
					break;
				}
				case TRIGGER_MEAS:		/* Trigger Measurement */
				{
					CapacityMeasurement();
					break;
				}
				case GET_TEMP_RH: 		/* Get Temperature and Humidity */
				{
					STATUS_t Status = TemperatureMeasurement();
					if (Status == Status_OK)
					{
						fprintf_P(&USB_Stream, PSTR("%s "), dtostr(System.Temperature, AFTERPOINT));
						fprintf_P(&USB_Stream, PSTR("%s\r\n"), dtostr(System.Humidity, AFTERPOINT));
					}
					else if (Status == Status_Error)
					{
						fprintf_P(&USB_Stream, PSTR("Device Error: Read temperature unexpected error: %u\r\n"), twi_status());
					}
					else /* Status == Status_Unregistered */
					{
						fprintf_P(&USB_Stream, PSTR("Device Error: SHTC3 module unregistered\r\n"));
					}
					break;
				}
				case SET_H_VOUT:		/* Set Signal Pin as HIGH */
				{
					DisableGenerations();
					SET_GEN_STATE;
					MEASURING_SYSTEM_ON;
					SEND_CONFIRMATION;
					break;
				}
				case SET_L_VOUT: 		/* Set Signal Pin as LOW */
				{
					DisableGenerations();
					CLR_GEN_STATE;
					MEASURING_SYSTEM_ON;
					SEND_CONFIRMATION;
					break;
				}
				case SET_GENERATIONS: 	/* Set Generation on Signal Pin */
				{
					EnableGenerations();
					MEASURING_SYSTEM_ON;
					SEND_CONFIRMATION;
					break;
				}
				case SET_NOMINAL:		/* Set Signal Pin as Nominal (HIGH-Z) */
				{
					DisableGenerations();
					CLR_GEN_STATE;
					MEASURING_SYSTEM_OFF;
					SEND_CONFIRMATION;
					break;
				}
				default:				/* Unrecognized Command Signalization */
				{
					fprintf_P(&USB_Stream, PSTR("Device Error: Unrecognized Command: %c\r\n"), CMD_Buffer[0]);
					break;
				}
			}
		}
		else
		{
			/* Secondary Commands */
			RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ' ');
			switch (CMD_Buffer[0])
			{
				case SET_CONSTANTS:
				{
					Constants.H_THR  = strtod((const char *)RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ' '), NULL);
					Constants.L_THR  = strtod((const char *)RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ' '), NULL);
					Constants.H_VOUT = strtod((const char *)RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ' '), NULL);
					Constants.L_VOUT = strtod((const char *)RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ' '), NULL);
					Constants.R_MEAS = strtoul((const char *)RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ENDCMD), NULL, 10);
					UPDATE_CONSTANTS__EEMEM;
					SEND_CONFIRMATION;
					break;
				}
				case SET_CORRECTIONS:
				{
					Corrections.A0 = strtod((const char *)RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ' '), NULL);
					Corrections.A1 = strtod((const char *)RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ' '), NULL);
					Corrections.A2 = strtod((const char *)RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ' '), NULL);
					Corrections.A3 = strtod((const char *)RingBuffer_GetAsString(&USB_Buffer, CMD_Buffer, ' '), NULL);
					UPDATE_CORRECTIONS__EEMEM;
					SEND_CONFIRMATION;
					break;
				}
				case LCD_AFTERMEAS:
				{
					if (System.IsLCDRegistered)
					{
						byte TimeStamp[10], Capacity[10], TicksCP[7], TicksDP[7], Progress[5];

						RingBuffer_GetAsString(&USB_Buffer, TimeStamp, ' ');
						RingBuffer_GetAsString(&USB_Buffer, Capacity, ' ');
						RingBuffer_GetAsString(&USB_Buffer, TicksCP, ' ');
						RingBuffer_GetAsString(&USB_Buffer, TicksDP, ' ');
						RingBuffer_GetAsString(&USB_Buffer, Progress, ENDCMD);

						lcd_cls();
						if (Progress[0])
						{
							fprintf_P(&LCD_Stream, PSTR("t %s "), TimeStamp);
							fprintf_P(&LCD_Stream, PSTR("P %s/100"), Progress);
						}
						else
						{
							fprintf_P(&LCD_Stream, PSTR("TimeStamp  %s"), TimeStamp);
						}
						lcd_locate(1, 0);
						fprintf_P(&LCD_Stream, PSTR("Capacity %s pF"), Capacity);
						lcd_locate(2, 0);
						fprintf_P(&LCD_Stream, PSTR("Ticks %s / "), TicksCP);
						fprintf_P(&LCD_Stream, PSTR("%s"), TicksDP);
						lcd_locate(3, 0);
						if (System.IsSHTRegistered)
						{
							fprintf_P(&LCD_Stream, PSTR("T %s%cC "), dtostr(System.Temperature, AFTERPOINT), 223);
							lcd_locate(3, 10);
							fprintf_P(&LCD_Stream, PSTR("RH %s %%"), dtostr(System.Humidity, AFTERPOINT));
						}
						else
						{
							fprintf_P(&LCD_Stream, PSTR("T ------  RH ------ "));
						}
					}
					SEND_CONFIRMATION;
					break;
				}
				default:
				{
					fprintf_P(&USB_Stream, PSTR("Error: Unrecognized Command: %c\r\n"), CMD_Buffer[0]);
				}
			}
		}
		RingBuffer_Clear(&USB_Buffer);
		LED_CMD_REC_OFF;
	}
}

/* Capacity Measuring Function */
STATUS_t CapacityMeasurement(void)
{
	/* Check Power Status */
	if (!PWR_STATUS)
	{
		fprintf_P(&USB_Stream, PSTR("Error: Check the power supply of the module.\r\n"));
		return Status_PowerError;
	}
	/* Discharge Capacity */
	CLR_GEN_STATE;
	MEASURING_SYSTEM_ON;
	/* Temperature and Humidity Measurement */
	STATUS_t Status = TemperatureMeasurement();
	/* Wait for LOW State on V_Cap Pin */
	uint cnt = 0;
	while (!V_CAP_IS_UNDER_L_THR)
	{
		_delay_us(100);
		cnt++;
		if (cnt > 100)
		{
			fprintf_P(&USB_Stream, PSTR("Error: Discharging the tested capacity is unattainable.\r\n"));
			return Status_DischargeTimeout;
		}
	}
	/* Measurement conditions ready - capacity discharged, power supply checked, temperature measured */
	/* Set sample index to 0 */
	System.SampleIdx = 0;
	System.IsMeasurementEnd = false;
	EnableInputCapture();
	SET_GEN_STATE;
	/* Waiting for end measurement */
	cnt = 0;
	while (!System.IsMeasurementEnd)
	{
		_delay_us(100);
		cnt++;
		if (cnt > 5000)
		{
			DisableInputCapture();
			CLR_GEN_STATE;
			MEASURING_SYSTEM_OFF;
			fprintf_P(&USB_Stream, PSTR("Error: Measurement timeout the tested capacity.\r\n"));
			return Status_MeasurementTimeout;
		}
	}
	/* Measurement End - Disable measuring system */
	DisableInputCapture();
	CLR_GEN_STATE;
	MEASURING_SYSTEM_OFF;
	/* Data presentation */
	if (Status == Status_OK)
	{
		fprintf_P(&USB_Stream, PSTR("%c %s "), SEND_TEMP, dtostr(System.Temperature, AFTERPOINT));
		fprintf_P(&USB_Stream, PSTR("%c %s "), SEND_RH, dtostr(System.Humidity, AFTERPOINT));
	}
	fprintf_P(&USB_Stream, PSTR("%c "), SEND_SP);
	for (byte i = 0; i < MAX_SAMPLES_BUF_SIZE - 1; i++)
	{
		fprintf_P(&USB_Stream, PSTR("%u "), System.Samples[i]);
	}
	fprintf_P(&USB_Stream, PSTR("%u\r\n"), System.Samples[MAX_SAMPLES_BUF_SIZE - 1]);
	return Status_OK;
}

/* Temperature & Humidity Measuring Function */
STATUS_t TemperatureMeasurement(void)
{
	if (System.IsSHTRegistered)
	{
		SHT_STATUS_t Status = sht_meas();
		if (Status == SHT_Status_Nominal)
		{
			System.Temperature = Round(sht_temperature(), PRECISION);
			System.Humidity = Round(sht_humidity(), PRECISION);
			return Status_OK;
		}
		else
		{
			System.IsSHTRegistered = false;
			twi_error();
			return Status_Error;
		}
	}
	return Status_Unregistered;
}

/* Generating Signal Functions */
void EnableGenerations(void)
{
	OCR0A = OCR_VALUE;
	TCCR0A |= _BV(COM0A0) | _BV(WGM01);
	TCCR0B |= _BV(CS00);
}
void DisableGenerations(void)
{
	TCCR0A = 0;
	TCCR0B = 0;
	OCR0A  = 0;
}

/* Input Capturing Functions */
void EnableInputCapture(void)
{
	/* Timer Clock - No Prescaling - Enable */
	sbi(TCCR1B, CS10);
	/* Clear Pending Interrupts */
	sbi(TIFR1, ICF1);
	/* Input Capture Interrupt Enable */
	sbi(TIMSK1, ICIE1);
}
void DisableInputCapture(void)
{
	/* Timer Clock - No Prescaling - Disable */
	cbi(TCCR1B, CS10);
	/* Input Capture Interrupt Disable */
	cbi(TIMSK1, ICIE1);
}

/* Input Capture Interrupt */
ISR(TIMER1_CAPT_vect, ISR_BLOCK)
{
	System.Samples[System.SampleIdx++] = ICR1;
	if (System.SampleIdx == MAX_SAMPLES_BUF_SIZE)
	{
		System.IsMeasurementEnd = true;
	}
	TOG_GEN_STATE;
	TCNT1 = 0;
}

/* Base Conversions Functions */
double Round(double N, double Precision)
{
	int M = (int)N;
	int W = (int)((N - M) * 100);
	int P = (int)(Precision * 100);
	int T = W % P;

	if (T > (P >> 1))
	{
		return M + (W - W % P + P) / 100.;
	}

	return M + (W - W % P) / 100.;
}
char * dtostr(double N, byte AfterPoint)
{
	static char double_buf[20];
	dtostrf(N, -20, AfterPoint, double_buf);
	for (byte i = 0; i < 20; i++)
	{
		if (double_buf[i] == ' ')
		{
			double_buf[i] = 0;
			break;
		}
	}
	return double_buf;
}

/* Event handler for the library USB Connection event. */
void EVENT_USB_Device_Connect(void)
{
	LED_USB_CONN;
}
/* Event handler for the library USB Disconnection event. */
void EVENT_USB_Device_Disconnect(void)
{
	LED_USB_ERROR;
	LED_USB_DISCONN;
	System.IsHostReady = false;
}
/* Event handler for the library USB Configuration Changed event. */
void EVENT_USB_Device_ConfigurationChanged(void)
{
	bool ConfigSuccess = true;
	ConfigSuccess &= CDC_Device_ConfigureEndpoints(&VirtualSerial_CDC_Interface);
	if (ConfigSuccess)
	{
		LED_USB_READY;
	}
	else
	{
		LED_USB_ERROR;
	}
}
/* Event handler for the library USB Control Request reception event. */
void EVENT_USB_Device_ControlRequest(void)
{
	CDC_Device_ProcessControlRequest(&VirtualSerial_CDC_Interface);
}
/* CDC class driver callback function the processing of changes to the virtual control lines sent from the host. */
void EVENT_CDC_Device_ControLineStateChanged(USB_ClassInfo_CDC_Device_t *const CDCInterfaceInfo)
{
	System.IsHostReady = (CDCInterfaceInfo->State.ControlLineStates.HostToDevice & CDC_CONTROL_LINE_OUT_DTR) != 0;
	if (System.IsHostReady)
	{
		LED_USB_READY;
	}
	else
	{
		LED_USB_ERROR;
	}
}
