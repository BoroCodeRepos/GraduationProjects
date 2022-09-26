################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../LUFA/Drivers/Board/Temperature.c 

OBJS += \
./LUFA/Drivers/Board/Temperature.o 

C_DEPS += \
./LUFA/Drivers/Board/Temperature.d 


# Each subdirectory must supply rules for building sources it contributes
LUFA/Drivers/Board/%.o: ../LUFA/Drivers/Board/%.c LUFA/Drivers/Board/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Compiler'
	avr-gcc -DF_USB=16000000UL -Wall -Os -fpack-struct -fshort-enums -ffunction-sections -fdata-sections -std=gnu99 -funsigned-char -funsigned-bitfields -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


