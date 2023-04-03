#ifndef DecodingFunctions_h
#define DecodingFunctions_h

#include "Arduino.h"
#include "BaseDevice.h"
#include "Sensors.h"
#include "ArduinoJson.h"
#include "ESP8266WiFi.h"
#include "ConstantsDeclarations.h"
#include <ArduinoWebsockets.h>

using namespace websockets;

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
