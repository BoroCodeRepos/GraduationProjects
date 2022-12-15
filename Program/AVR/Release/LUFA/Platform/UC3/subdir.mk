################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../LUFA/Platform/UC3/InterruptManagement.c 

S_UPPER_SRCS += \
../LUFA/Platform/UC3/Exception.S 

OBJS += \
./LUFA/Platform/UC3/Exception.o \
./LUFA/Platform/UC3/InterruptManagement.o 

S_UPPER_DEPS += \
./LUFA/Platform/UC3/Exception.d 

C_DEPS += \
./LUFA/Platform/UC3/InterruptManagement.d 


# Each subdirectory must supply rules for building sources it contributes
LUFA/Platform/UC3/%.o: ../LUFA/Platform/UC3/%.S LUFA/Platform/UC3/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Assembler'
	avr-gcc -x assembler-with-cpp -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '

LUFA/Platform/UC3/%.o: ../LUFA/Platform/UC3/%.c LUFA/Platform/UC3/subdir.mk
	@echo 'Building file: $<'
	@echo 'Invoking: AVR Compiler'
	avr-gcc -DF_USB=16000000UL -Wall -Os -fpack-struct -fshort-enums -ffunction-sections -fdata-sections -std=gnu99 -funsigned-char -funsigned-bitfields -mmcu=atmega32u4 -DF_CPU=16000000UL -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" -c -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '


