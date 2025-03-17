#if defined (ARDUINO_ARCH_ESP8266)
#include <ESP8266WiFi.h>
#elif defined(ESP32)
#include <WiFi.h>
#include <HTTPClient.h>
#else
#error Architecture unrecognized by this code.
#endif
#include <EEPROM.h>
#include "CommandHandler.h"
#include "ConstantsDeclarations.h"
#include <Arduino.h>
#include "EEPRomHandler.h"
#include "GlobalVariables.h"
#include "DebugMessageLib.h"
#include "DecodingFunctions.h"
#include "MainFunctions.h"



void SendCommandResponseToServer(String commandId, String response);
void UpdateOTA(String commandId);
void PerformOTAUpdate(String githubUrl);

void HandleCommand(String commandId, String commandText, float commandValue, String commandTextparameter){
  DebugMessage("Recieved command " + commandText + " with param" + commandTextparameter + " and "+ String(commandValue));

  if(commandText == CommandWifiName) commandTextparameter.toCharArray(GetGlobalVariables()->baseDevice.ssid,WiFiLength);
  if(commandText == CommandWifiPassword && commandText != EmptyPassword) commandTextparameter.toCharArray(GetGlobalVariables()->baseDevice.password,WiFiLength);
  if(commandText == CommandSetId) commandTextparameter.toCharArray(GetGlobalVariables()->baseDevice.id,IdStringLength);
  if(commandText == CommandSetName) commandTextparameter.toCharArray(GetGlobalVariables()->baseDevice.Name,UserInputStringLength);
  if(commandText == CommandSetAdress) commandTextparameter.toCharArray(GetGlobalVariables()->baseDevice.serverUrl,ServerAdressLength);
  if(commandText == CommandSetPort) commandTextparameter.toCharArray(GetGlobalVariables()->baseDevice.port, PortLength);
  if(commandText == CommandWifiRefreshTimer) GetGlobalVariables()->SetWiFiTimerInSeconds(commandValue);
  if(commandText == CommandSensorTimer) GetGlobalVariables()->SetSensoricTimerInSeconds(commandValue);
  if(commandText == CommandDebugMessage) GetGlobalVariables()->baseDevice.debugMessage = commandValue==1;
  if(commandText == CommandClearEEPRom) {
    DebugMessage("Clearing eeprom");
    ClearEEPROM(EEPROM);
    ESP.restart();
  }
  if(commandText == CommandTriggerSensor) GetGlobalVariables()->SensoricTimer.SetForceTrigger();
  if(commandText == CommandReconectWifi)  {
    GetGlobalVariables()->WifiTimer.SetForceTrigger();
    GetGlobalVariables()->initialized;
  }
  if(commandText == CommandSaveToEEPRom) GetGlobalVariables()->EEPROMWrteReq = true;
  if(commandText == CommandReboot) ESP.restart();
  if(commandText == CommandSaveDevice){
        SaveToEEPRom(EEPROM, GetGlobalVariables()->baseDevice);
        RegisterBaseDevice(GetGlobalVariables()->baseDevice);

  }
  if(commandText == CommandActivityCheck) SendCommandResponseToServer(commandId, CommandResponseActive);
  if(commandText == CommandUpdateAvaliable) UpdateOTA(commandId);
}

void SendCommandResponseToServer(String commandId, String response){
    if(commandId == ""){
      DebugMessage("Do not have original command ID. Not sending response to websocket");
      return;
    }

    DynamicJsonDocument doc(512);
    doc[type] = CommandResponseType;
    doc[DTSourceId] = commandId;
    doc[timeCreated] = GetGlobalVariables() -> dateTime.ToJSONString();
    doc[DTvalueName] = response;
    doc[DTstringValue] = "";
    doc[DTnumericValue] = 0;
    doc[DTvalueUnit] = "";
    String json;
    serializeJson(doc, json);
    doc.clear();

    SendViaWebSocket(json);
}
