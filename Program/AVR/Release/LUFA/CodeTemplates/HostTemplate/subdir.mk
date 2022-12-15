################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../LUFA/CodeTemplates/HostTemplate/HostApplication.c 

OBJS += \
./LUFA/CodeTemplates/HostTemplate/HostApplication.o 

C_DEPS += \
./LUFA/CodeTemplates/HostTemplate/HostApplication.d 


# Each subdirectory must supply rules for building sources it contributes
LUFA/CodeTemplates/HostTemplate/%.o: ../LUFA/CodeTemplates/HostTemplate/%.c LUFA/CodeTemplates/HostTemplate/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Compiler'
	avr-gcc -DF_USB=16000000UL -Wall -Os -fpack-struct -fshort-enums -ffunction-sections -fdata-sections -std=gnu99 -funsigned-char -funsigned-bitfields -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


