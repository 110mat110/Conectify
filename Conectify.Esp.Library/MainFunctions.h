#ifndef MainFunctions_h
#define MainFunctions_h

#include "Arduino.h"
#include "Sensors.h"
#include "BaseDevice.h"
#include "TickTimer.h"
#include "GlobalVariables.h"

void StartupMandatoryRoutine(int psensorArrSize, int pactuatorArrSize, void(*SensorsDeclarations)());
void LoopMandatoryRoutines();

void HandleCommand(String commandText, float commandValue, String commandTextparameter);
void SendViaWebSocket(String message);
#endif