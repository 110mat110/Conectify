#include "TickTimer.h"
#include "Sensors.h"
#include "BaseDevice.h"
#include "DebugMessageLib.h"
#include "MainFunctions.h"
#include "GlobalVariables.h"
#include "DecodingFunctions.h"

#define NoSensors                   1
#define NoActuators                 1

void UserSetupRoutines(){
  GetGlobalVariables()->SetSensoricTimerInSeconds(60);
}

void ActuatorNumericRoutine(float value, String unit){
    digitalWrite(2, value > 0);
}


void ActuatorStringRoutine(String value, String unit){
}

//Then declare all sensors and actuators
void DeclareSensors(){
  GetGlobalVariables()->sensorsArr[0] = Sensor("Test", "Random", "");
    Actuator ac = Actuator(&GetGlobalVariables()->sensorsArr[0], "Test");
  ac.RegisterFunctionForNumericValue(ActuatorNumericRoutine);
  ac.RegisterFunctionForStringValue(ActuatorStringRoutine);
  GetGlobalVariables()->actuatorsArr[0] = ac;
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
  SetSoftwareName("Conectify.Random");
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
