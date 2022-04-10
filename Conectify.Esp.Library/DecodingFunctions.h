#ifndef DecodingFunctions_h
#define DecodingFunctions_h

#include "Arduino.h"
#include "BaseThing.h"
#include "Sensors.h"
#include "ArduinoJson.h"
#include "ESP8266WiFi.h"
#include "ConstantsDeclarations.h"
#include "Thing.h"

void DecodeRegisteredThingValue(String response, BaseThing &baseThing);
void RegisterBaseThing(BaseThing &baseThing, ESP8266WiFiClass WiFi, Thing thing);
String serializeThing(BaseThing &baseThing, ESP8266WiFiClass WiFi, Thing thing);

void RegisterSensor(Sensor &sensor, BaseThing &baseThing);
void DecodeRegisteredSensorValue(String payload,Sensor &sensor);
void RegisterActuator(Actuator &sensor, BaseThing &baseThing);
void DecodeRegisteredActuatorValue(String payload,Actuator &actuator);

void SendSensorValuesToServer(Sensor sensor, BaseThing baseThing, Time dateTime,     
    void (*handleFunc)(String commandText, float commandValue, String commandTextParam),
    Actuator* actuators,
    byte actuatorsLength
);

void decodeIncomingJson(String incomingJson,   
    void (*handleFunc)(String commandText, float commandValue, String commandTextParam),
    Time dateTime,
    Actuator* actuators,
    byte actuatorsLength
);
void DecodeObject(JsonObject root,
    void (*handleFunc)(String commandText, float commandValue, String commandTextParam),
    Time dateTime,
    Actuator* actuators,
    byte actuatorsLength
);

void GetValues(
    BaseThing thing,  
    void (*handleFunc)(String commandText, float commandValue, String commandTextParam),
    Time dateTime,
    Actuator* actuators,
    byte actuatorsLength
  );

String RequestTime(BaseThing baseThing);
#endif
