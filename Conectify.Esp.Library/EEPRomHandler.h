#ifndef EepRomHander_h
#define EepRomHander_h

#include "Arduino.h"
#include "EEPROM.h"
#include "BaseThing.h"
#include "Sensors.h"

void SaveToEEPRom(EEPROMClass eeprom, BaseThing baseThing);
void ReadFromEEPRom(EEPROMClass eeprom, BaseThing &baseThing);

void SaveSensorIDsToEEPROM(EEPROMClass eeprom, Sensor* sensorArray, byte sensorArraySize);
void SaveActuatorIDsToEEPROM(EEPROMClass eeprom, Actuator* actuatorArray, byte actuatorArraySize);
void LoadSensorsFromEEPROM(EEPROMClass eeprom, Sensor* sensorArray);
void LoadActuatorFromEEPROM(EEPROMClass eeprom, Actuator* actuatorArray);
void ClearEEPROM(EEPROMClass eeprom);
#endif
