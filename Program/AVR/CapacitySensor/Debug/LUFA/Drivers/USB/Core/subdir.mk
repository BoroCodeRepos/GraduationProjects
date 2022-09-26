################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../LUFA/Drivers/USB/Core/ConfigDescriptors.c \
../LUFA/Drivers/USB/Core/DeviceStandardReq.c \
../LUFA/Drivers/USB/Core/Events.c \
../LUFA/Drivers/USB/Core/HostStandardReq.c \
../LUFA/Drivers/USB/Core/USBTask.c 

OBJS += \
./LUFA/Drivers/USB/Core/ConfigDescriptors.o \
./LUFA/Drivers/USB/Core/DeviceStandardReq.o \
./LUFA/Drivers/USB/Core/Events.o \
./LUFA/Drivers/USB/Core/HostStandardReq.o \
./LUFA/Drivers/USB/Core/USBTask.o 

C_DEPS += \
./LUFA/Drivers/USB/Core/ConfigDescriptors.d \
./LUFA/Drivers/USB/Core/DeviceStandardReq.d \
./LUFA/Drivers/USB/Core/Events.d \
./LUFA/Drivers/USB/Core/HostStandardReq.d \
./LUFA/Drivers/USB/Core/USBTask.d 


# Each subdirectory must supply rules for building sources it contributes
LUFA/Drivers/USB/Core/%.o: ../LUFA/Drivers/USB/Core/%.c LUFA/Drivers/USB/Core/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Compiler'
	avr-gcc -DF_USB=16000000UL -Wall -g2 -gstabs -O0 -fpack-struct -fshort-enums -ffunction-sections -fdata-sections -std=gnu99 -funsigned-char -funsigned-bitfields -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


