################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../LUFA/Drivers/Peripheral/XMEGA/Serial_XMEGA.c \
../LUFA/Drivers/Peripheral/XMEGA/TWI_XMEGA.c 

OBJS += \
./LUFA/Drivers/Peripheral/XMEGA/Serial_XMEGA.o \
./LUFA/Drivers/Peripheral/XMEGA/TWI_XMEGA.o 

C_DEPS += \
./LUFA/Drivers/Peripheral/XMEGA/Serial_XMEGA.d \
./LUFA/Drivers/Peripheral/XMEGA/TWI_XMEGA.d 


# Each subdirectory must supply rules for building sources it contributes
LUFA/Drivers/Peripheral/XMEGA/%.o: ../LUFA/Drivers/Peripheral/XMEGA/%.c LUFA/Drivers/Peripheral/XMEGA/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Compiler'
	avr-gcc -Wall -g2 -gstabs -O0 -fpack-struct -fshort-enums -ffunction-sections -fdata-sections -std=gnu99 -funsigned-char -funsigned-bitfields -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


