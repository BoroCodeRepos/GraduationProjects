################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../LUFA/Drivers/USB/Class/Device/CDCClassDevice.c 

OBJS += \
./LUFA/Drivers/USB/Class/Device/CDCClassDevice.o 

C_DEPS += \
./LUFA/Drivers/USB/Class/Device/CDCClassDevice.d 


# Each subdirectory must supply rules for building sources it contributes
LUFA/Drivers/USB/Class/Device/%.o: ../LUFA/Drivers/USB/Class/Device/%.c LUFA/Drivers/USB/Class/Device/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Compiler'
	avr-gcc -DF_USB=16000000UL -Wall -Os -fpack-struct -fshort-enums -ffunction-sections -fdata-sections -std=gnu99 -funsigned-char -funsigned-bitfields -Wl,-u,vfprintf -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


