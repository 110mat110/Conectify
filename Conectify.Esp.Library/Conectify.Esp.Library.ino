#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <WiFiUdp.h>
#include <DHT.h>
#include "TickTimer.h"
#include "Sensors.h"
#include "BaseThing.h"
#include "DebugMessageLib.h"
#include "MainFunctions.h"
#include "DecodingFunctions.h"
#include "GlobalVariables.h"
#include "Thing.h"


#define NAME                        "Interior_Sensor"
#define NoSensors                   3
#define NoActuators                 0
#define THING_POSITION_DESCRIPTION  "Obyvacka"
#define THING_LAT                   0
#define THING_LONG                  0
#define DHTPIN                      13
#define DHTTYPE                     DHT11

DHT dht(DHTPIN, DHTTYPE);

void UserSetupRoutines(){
  dht.begin();
}

//Then declare all sensors and actuators
void DeclareSensors(){
  GetGlobalVariables()->sensorsArr[0] = Sensor("DHT11", "Temperature", "C");
  GetGlobalVariables()->sensorsArr[1] = Sensor("DHT11", "Humidity", "%Rh");
  GetGlobalVariables()->sensorsArr[2] = Sensor("Light", "Light", "%");
}

//This will perform each loop
void UserLoopRoutines(){
  if(GetGlobalVariables()->SensoricTimer.IsTriggered()){
    GetGlobalVariables()->sensorsArr[0].SetNumericValue(getDHTTemperature());
    GetGlobalVariables()->sensorsArr[1].SetNumericValue(getDHTHumidity());
    GetGlobalVariables()->sensorsArr[2].SetNumericValue(map(analogRead(A0),0,1023,0,100));
  }
}

float getDHTTemperature() {
  DebugMessage("DHT Temp reading:");
  DebugMessage(String(dht.readTemperature()));
  return dht.readTemperature();
}
float getDHTHumidity() {
  return dht.readHumidity();
}


#pragma region HiddenRoutines
void setup() {
Thing IoTThing;
IoTThing.thingPositionDescription = THING_POSITION_DESCRIPTION;
IoTThing.thingName = NAME;
IoTThing.longitude = THING_LAT;
IoTThing.latitude = THING_LONG;

StartupMandatoryRoutine(NoSensors, NoActuators, DeclareSensors, IoTThing);
UserSetupRoutines();
}

void loop() {
  LoopMandatoryRoutines();
  if(GetGlobalVariables()->initialized){
    UserLoopRoutines();
  }
}
#pragma endregion