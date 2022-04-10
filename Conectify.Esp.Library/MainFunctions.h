#ifndef MainFunctions_h
#define MainFunctions_h

#include "Arduino.h"
#include "Sensors.h"
#include "BaseThing.h"
#include "TickTimer.h"
#include "GlobalVariables.h"
#include "Thing.h"

void SendUpdate();

void StartupMandatoryRoutine(int psensorArrSize, int pactuatorArrSize, void(*SensorsDeclarations)(), Thing thing);
void LoopMandatoryRoutines();

void RequestActuatorValues();
void SendSensoricValues();

void HandleCommand(String commandText, float commandValue, String commandTextparameter);

#endif