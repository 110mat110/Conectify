//This is testing file for conectify library. May be used as demo or testing.
#include "TickTimer.h"
#include "Sensors.h"
#include "BaseDevice.h"
#include "DebugMessageLib.h"
#include "MainFunctions.h"
#include "GlobalVariables.h"
#include "DecodingFunctions.h"

#define NAME                        "Test"
#define NoSensors                   1
#define NoActuators                 1

void UserSetupRoutines(){
  GetGlobalVariables()->SetSensoricTimerInSeconds(60);
}

void ActuatorNumericRoutine(float value, String unit){
    digitalWrite(2, value > 0);
}

//Then declare all sensors and actuators
void DeclareSensors(){
GetGlobalVariables()->sensorsArr[0] = Sensor("Test", "Random", "");
}

//This will perform each loop
void UserLoopRoutines(){
  if(GetGlobalVariables()->SensoricTimer.IsTriggered()){
    GetGlobalVariables()->SensoricTimer.ResetTimer();
    GetGlobalVariables()->sensorsArr[0].SetNumericValue(random(0,100));
  }
}

#pragma region HiddenRoutines
void setup() {

StartupMandatoryRoutine(NoSensors, NoActuators, DeclareSensors);
UserSetupRoutines();
}

void loop() {
  LoopMandatoryRoutines();
  if(GetGlobalVariables()->initialized){
    UserLoopRoutines();
  }
}
#pragma endregion
