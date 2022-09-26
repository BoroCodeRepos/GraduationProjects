################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../LUFA/Drivers/USB/Class/Device/AudioClassDevice.c \
../LUFA/Drivers/USB/Class/Device/CCIDClassDevice.c \
../LUFA/Drivers/USB/Class/Device/CDCClassDevice.c \
../LUFA/Drivers/USB/Class/Device/HIDClassDevice.c \
../LUFA/Drivers/USB/Class/Device/MIDIClassDevice.c \
../LUFA/Drivers/USB/Class/Device/MassStorageClassDevice.c \
../LUFA/Drivers/USB/Class/Device/PrinterClassDevice.c \
../LUFA/Drivers/USB/Class/Device/RNDISClassDevice.c 

OBJS += \
./LUFA/Drivers/USB/Class/Device/AudioClassDevice.o \
./LUFA/Drivers/USB/Class/Device/CCIDClassDevice.o \
./LUFA/Drivers/USB/Class/Device/CDCClassDevice.o \
./LUFA/Drivers/USB/Class/Device/HIDClassDevice.o \
./LUFA/Drivers/USB/Class/Device/MIDIClassDevice.o \
./LUFA/Drivers/USB/Class/Device/MassStorageClassDevice.o \
./LUFA/Drivers/USB/Class/Device/PrinterClassDevice.o \
./LUFA/Drivers/USB/Class/Device/RNDISClassDevice.o 

C_DEPS += \
./LUFA/Drivers/USB/Class/Device/AudioClassDevice.d \
./LUFA/Drivers/USB/Class/Device/CCIDClassDevice.d \
./LUFA/Drivers/USB/Class/Device/CDCClassDevice.d \
./LUFA/Drivers/USB/Class/Device/HIDClassDevice.d \
./LUFA/Drivers/USB/Class/Device/MIDIClassDevice.d \
./LUFA/Drivers/USB/Class/Device/MassStorageClassDevice.d \
./LUFA/Drivers/USB/Class/Device/PrinterClassDevice.d \
./LUFA/Drivers/USB/Class/Device/RNDISClassDevice.d 


# Each subdirectory must supply rules for building sources it contributes
LUFA/Drivers/USB/Class/Device/%.o: ../LUFA/Drivers/USB/Class/Device/%.c LUFA/Drivers/USB/Class/Device/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Compiler'
	avr-gcc -DF_USB=16000000UL -Wall -g2 -gstabs -O0 -fpack-struct -fshort-enums -ffunction-sections -fdata-sections -std=gnu99 -funsigned-char -funsigned-bitfields -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


