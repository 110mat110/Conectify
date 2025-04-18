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
#include <Update.h>

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


void UpdateOTA(String commandId){
  String softwareUrl = GetSoftwareUrl(GetGlobalVariables()->baseDevice);

  if(softwareUrl != ""){
    DebugMessage("Update url: " + softwareUrl);
    PerformOTAUpdate(softwareUrl);
  } else{
    DebugMessage("Got empty update URL");
  }
}

void PerformOTAUpdate(String githubUrl) {
HTTPClient http;
http.begin(githubUrl);  // Initialize HTTP client with the GitHub URL

int httpCode = http.GET();  // Send HTTP GET request to download the file

if (httpCode == HTTP_CODE_OK) {
  // If the response is successful, proceed with OTA update
  int contentLength = http.getSize();
  
  if (contentLength > 0) {
    bool canBegin = Update.begin(contentLength);
    
    if (canBegin) {
      DebugMessage("Begin OTA update...");
      
      // Fetch the stream from the HTTP response
      WiFiClient *client = http.getStreamPtr();
      
      // Update firmware in chunks
      size_t written = Update.writeStream(*client);
      
      if (written == contentLength) {
        DebugMessage("Written : " + String(written) + " successfully");
      } else {
        DebugMessage("Written only : " + String(written) + "/" + String(contentLength) + ". Retry?");
      }
      
      // End the update process and check the result
      if (Update.end()) {
        if (Update.isFinished()) {
          DebugMessage("Update successfully completed. Sending confirmation");
          ConfirmUpdate(GetGlobalVariables()->baseDevice);
          DebugMessage("Update confirmed. Rebooting");
          ESP.restart();
        } else {
          DebugMessage("Update not finished? Something went wrong.");
        }
      } else {
        DebugMessage("Error Occurred. Error #: " + String(Update.getError()));
      }
    } else {
      DebugMessage("Not enough space to begin OTA");
    }
  } else {
    DebugMessage("Content length is not valid");
  }
} else {
  DebugMessage("Failed to download firmware. HTTP error code: " + String(httpCode));
}

http.end();  // Close the HTTP connection
}
