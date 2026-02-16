#ifndef EepRomHander_h
#define EepRomHander_h

#include <Arduino.h>
#include "EEPROM.h"
#include "BaseDevice.h"
#include "Sensors.h"
void SaveBaseDevice(const BaseDevice &baseDevice);
void ReadBaseDevice(BaseDevice &device);

void SaveSensorIDs(Sensor* sensorArray, byte sensorArraySize);
void SaveActuatorIDs(Actuator* actuatorArray, byte actuatorArraySize);
void LoadSensors(Sensor* sensorArray);
void LoadActuators(Actuator* actuatorArray);

void ClearStorage();
#endif
