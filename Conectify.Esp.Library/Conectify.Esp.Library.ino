//This is testing file for conectify library. May be used as demo or testing.

#include <WiFiUdp.h>
#include "TickTimer.h"
#include "Sensors.h"
#include "BaseThing.h"
#include "DebugMessageLib.h"
#include "MainFunctions.h"
#include "DecodingFunctions.h"
#include "GlobalVariables.h"

#define NAME                        "Testing"
#define NoSensors                   1
#define NoActuators                 1
#define THING_POSITION_DESCRIPTION  "x"
#define THING_LAT                   0
#define THING_LONG                  0
#define NoSensors 1
#define NoActuators 1

int led = 0;

void UserSetupRoutines(){
  pinMode(led, OUTPUT);
}

void ActuatorNumericRoutine(float value, String unit){
    SetLed(value == 0); 
}
void ActuatorStringRoutine(String value, String unit){
    DebugMessage("Got into stringRoutine");
}

//Then declare all sensors and actuators
void DeclareSensors(){
    GetGlobalVariables()->sensorsArr[0] = Sensor("Testing Sensor", "Light", "%");
    Actuator ac = Actuator(&GetGlobalVariables()->sensorsArr[0],"Testing actuator");
    ac.RegisterFunctionForNumericValue(ActuatorNumericRoutine);
    ac.RegisterFunctionForStringValue(ActuatorStringRoutine);
    GetGlobalVariables()->actuatorsArr[0] = ac;
    if(!GetGlobalVariables()->sensorsArr[0].isInitialized)
        DebugMessage("Sensor declared");

  DebugMessage("Sensors declared");
}

//This will perform each loop
void UserLoopRoutines(){
  if(GetGlobalVariables()->SensoricTimer.IsTriggered()){
    GetGlobalVariables()->SensoricTimer.ResetTimer();
    SendSensoricValues();
  }
}

void SetLed(bool state){  
  digitalWrite(led, state);
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
