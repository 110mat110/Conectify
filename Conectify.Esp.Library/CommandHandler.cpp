#include "CommandHanlder.h"
#include "ConstantsDeclarations.h"
#include "Arduino.h"
#include <EEPROM.h>
#include "EEPRomHandler.h"
#include "GlobalVariables.h"
#include "DebugMessageLib.h"

void HandleCommand(String commandId, String commandText, float commandValue, String commandTextparameter){
  DebugMessage("Recieved command" + commandText + " with param" + commandTextparameter + " and "+ String(commandValue));

  if(commandText == CommandWifiName) commandTextparameter.toCharArray(GetGlobalVariables()->baseThing.ssid,WiFiLength);
  if(commandText == CommandWifiPassword) commandTextparameter.toCharArray(GetGlobalVariables()->baseThing.password,WiFiLength);
  if(commandText == CommandSetId) commandTextparameter.toCharArray(GetGlobalVariables()->baseThing.id,IdStringLength);
  if(commandText == CommandSetAdress) commandTextparameter.toCharArray(GetGlobalVariables()->baseThing.serverUrl,ServerAdressLength);
  if(commandText == CommandWifiRefreshTimer) GetGlobalVariables()->SetWiFiTimerInSeconds(commandValue);
  if(commandText == CommandSensorTimer) GetGlobalVariables()->SetSensoricTimerInSeconds(commandValue);
  if(commandText == CommandDebugMessage) GetGlobalVariables()->baseThing.debugMessage = commandValue==1;
  if(commandText == CommandClearEEPRom) {
    DebugMessage("Clearing eeprom");
    ClearEEPROM(EEPROM);
    ESP.restart();
  }
  if(commandText == CommandTriggerSensor) GetGlobalVariables()->SensoricTimer.SetForceTrigger();
  if(commandText == CommandReconectWifi)  GetGlobalVariables()->WiFiRestartReq = true;
  if(commandText == CommandSaveToEEPRom) GetGlobalVariables()->EEPROMWrteReq = true;
  if(commandText == CommandReboot) ESP.restart();
  //if(commandText == CommandActivityCheck) SendCommandResponseToServer(commandId, GetGlobalVariables() -> baseThing, GetGlobalVariables() -> dateTime);
}
