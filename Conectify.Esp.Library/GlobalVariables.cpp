#include "GlobalVariables.h"
#include "DebugMessageLib.h"
#include <Arduino.h>

#define LEDON LOW
#define LEDOFF HIGH
#define ONBOARDLED 2

GlobalVariables *globals;
bool GlobalsInitialized = false;

GlobalVariables *GetGlobalVariables()
{
  if (!GlobalsInitialized)
  {
    globals = new GlobalVariables();
    globals->TimeHandler.intervalMS = 100;
    globals->SensoricTimer.SetForceTrigger();
    GlobalsInitialized = true;
  }
  return globals;
}

void GlobalVariables::SetWiFiTimerInSeconds(int secondsPerTimer)
{
  WifiTimer.intervalMS = secondsPerTimer * 1000;
  baseDevice.WiFiTimer = secondsPerTimer;
}
void GlobalVariables::SetSensoricTimerInSeconds(int secondsPerTimer)
{
  SensoricTimer.intervalMS = secondsPerTimer * 1000;
  baseDevice.SensorTimer = secondsPerTimer;
}

void GlobalVariables::InvertLed()
{
  ledstate = GVNot(ledstate);

  SetLed();
}

void GlobalVariables::SetLedON()
{
  ledstate = LEDON;

  SetLed();
}

void GlobalVariables::SetLedOFF()
{
  ledstate = LEDOFF;

  SetLed();
}

void GlobalVariables::SetLed()
{
  pinMode(ONBOARDLED, OUTPUT);
  digitalWrite(ONBOARDLED, ledstate);
}

int GlobalVariables::GVNot(int state)
{
  if (state == LOW)
    return HIGH;
  return LOW;
}

bool GlobalVariables::RestartRequired()
{
  if (WiFiRestartReq)
  {
    WiFiRestartReq = false;
    return true;
  }
  return false;
}

bool GlobalVariables::EEPROMWriteRequired()
{
  if (EEPROMWrteReq)
  {
    EEPROMWrteReq = false;
    return true;
  }
  return false;
}