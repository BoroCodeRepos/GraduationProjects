################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../LUFA/Drivers/USB/Core/AVR8/Template/Template_Endpoint_Control_R.c \
../LUFA/Drivers/USB/Core/AVR8/Template/Template_Endpoint_Control_W.c \
../LUFA/Drivers/USB/Core/AVR8/Template/Template_Endpoint_RW.c \
../LUFA/Drivers/USB/Core/AVR8/Template/Template_Pipe_RW.c 

OBJS += \
./LUFA/Drivers/USB/Core/AVR8/Template/Template_Endpoint_Control_R.o \
./LUFA/Drivers/USB/Core/AVR8/Template/Template_Endpoint_Control_W.o \
./LUFA/Drivers/USB/Core/AVR8/Template/Template_Endpoint_RW.o \
./LUFA/Drivers/USB/Core/AVR8/Template/Template_Pipe_RW.o 

C_DEPS += \
./LUFA/Drivers/USB/Core/AVR8/Template/Template_Endpoint_Control_R.d \
./LUFA/Drivers/USB/Core/AVR8/Template/Template_Endpoint_Control_W.d \
./LUFA/Drivers/USB/Core/AVR8/Template/Template_Endpoint_RW.d \
./LUFA/Drivers/USB/Core/AVR8/Template/Template_Pipe_RW.d 


# Each subdirectory must supply rules for building sources it contributes
LUFA/Drivers/USB/Core/AVR8/Template/%.o: ../LUFA/Drivers/USB/Core/AVR8/Template/%.c LUFA/Drivers/USB/Core/AVR8/Template/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Compiler'
	avr-gcc -DF_USB=16000000UL -Wall -Os -fpack-struct -fshort-enums -ffunction-sections -fdata-sections -std=gnu99 -funsigned-char -funsigned-bitfields -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


