################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../LUFA/Drivers/Peripheral/AVR8/Serial_AVR8.c \
../LUFA/Drivers/Peripheral/AVR8/TWI_AVR8.c 

OBJS += \
./LUFA/Drivers/Peripheral/AVR8/Serial_AVR8.o \
./LUFA/Drivers/Peripheral/AVR8/TWI_AVR8.o 

C_DEPS += \
./LUFA/Drivers/Peripheral/AVR8/Serial_AVR8.d \
./LUFA/Drivers/Peripheral/AVR8/TWI_AVR8.d 


# Each subdirectory must supply rules for building sources it contributes
LUFA/Drivers/Peripheral/AVR8/%.o: ../LUFA/Drivers/Peripheral/AVR8/%.c LUFA/Drivers/Peripheral/AVR8/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Compiler'
	avr-gcc -DF_USB=16000000UL -Wall -Os -fpack-struct -fshort-enums -ffunction-sections -fdata-sections -std=gnu99 -funsigned-char -funsigned-bitfields -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


