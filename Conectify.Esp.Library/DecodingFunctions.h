#ifndef DecodingFunctions_h
#define DecodingFunctions_h

#include "Arduino.h"
#include "BaseDevice.h"
#include "Sensors.h"
#include "ArduinoJson.h"
#if defined (ARDUINO_ARCH_ESP8266)
#include "ESP8266WiFi.h"
#elif defined(ESP32)
#include "WiFi.h"
#else
#error Architecture unrecognized by this code.
#endif
#include "ConstantsDeclarations.h"

void RegisterBaseDevice(BaseDevice &baseDevice);

void RegisterSensor(Sensor &sensor, BaseDevice &baseDevice);
void RegisterActuator(Actuator &sensor, BaseDevice &baseDevice);

void decodeIncomingJson(String incomingJson,   
    void (*handleFunc)(String commandText, float commandValue, String commandTextParam),
    Time dateTime,
    Actuator* actuators,
    byte actuatorsLength
);

String RequestTime(BaseDevice baseDevice);
#endif
