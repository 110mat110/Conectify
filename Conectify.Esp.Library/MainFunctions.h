#ifndef MainFunctions_h
#define MainFunctions_h

#include "Arduino.h"
#include <DNSServer.h>
#include "Sensors.h"
#include "BaseDevice.h"
#include "TickTimer.h"
#include "GlobalVariables.h"
#include "Value.h"

DNSServer* GetDns();
void StartupMandatoryRoutine(int psensorArrSize, int pactuatorArrSize, void(*SensorsDeclarations)());
void LoopMandatoryRoutines();
bool IsWiFi();

void HandleCommand(String commandText, float commandValue, String commandTextparameter);
void SendViaWebSocket(String message);

void SetWatchDog(Value valuesArray[], int valuesLength);
#endif