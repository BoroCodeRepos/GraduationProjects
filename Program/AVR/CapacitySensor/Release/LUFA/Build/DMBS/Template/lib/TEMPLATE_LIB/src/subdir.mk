################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../LUFA/Build/DMBS/Template/lib/TEMPLATE_LIB/src/template_lib.c 

OBJS += \
./LUFA/Build/DMBS/Template/lib/TEMPLATE_LIB/src/template_lib.o 

C_DEPS += \
./LUFA/Build/DMBS/Template/lib/TEMPLATE_LIB/src/template_lib.d 


# Each subdirectory must supply rules for building sources it contributes
LUFA/Build/DMBS/Template/lib/TEMPLATE_LIB/src/%.o: ../LUFA/Build/DMBS/Template/lib/TEMPLATE_LIB/src/%.c LUFA/Build/DMBS/Template/lib/TEMPLATE_LIB/src/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Compiler'
	avr-gcc -DF_USB=16000000UL -Wall -Os -fpack-struct -fshort-enums -ffunction-sections -fdata-sections -std=gnu99 -funsigned-char -funsigned-bitfields -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


