#include <ESPAsyncWebServer.h>
#include <DNSServer.h>
#include "WebServer.h"
#include "GlobalVariables.h"
#include "DebugMessageLib.h"
#include "EEPRomHandler.h"
#include "MainFunctions.h"
#include "CommandHandler.h"

AsyncWebServer server(80);

String processor(const String &var)
{
    // Serial.println(var);
    if (var == "SSID")
    {
        return GetGlobalVariables()->baseDevice.ssid;
    }
    if (var == "deviceId")
    {
        return GetGlobalVariables()->baseDevice.id;
    }
    if (var == "serveradress")
    {
        return GetGlobalVariables()->baseDevice.serverUrl;
    }
    if (var == "password")
    {
        return EmptyPassword;
    }
    if (var == "wifi")
    {
        return IsWiFi() ? "<span class=\"badge on\">Wi-Fi connected</span>" : "<span class=\"badge off\">Access point mode</span>";
    }
    if (var == "connectToWifiHidden")
    {
        DebugMessage("Status of connectToWifiHidden: ");

        String response = IsWiFi() ? "false" : "true";
        DebugMessage(response);
        return response;
    }
    if (var == "connectToWifiShown")
    {
        return IsWiFi() ? "true" : "false";
    }

    if (var == "css"){
      return CSS;
    }

    if (var == "sensors")
    {
        String result = "";
        DebugMessage("Replacing sensor literal");
        for (int i = 0; i < GetGlobalVariables()->sensorsArrSize; i++)
        {
            String currentSensorHtml = GetGlobalVariables()->sensorsArr[i].ShowHtml(i);
            result.concat(currentSensorHtml);
        }
        return result;
    }
    if (var == CommandWifiRefreshTimer)
    {
        return String(GetGlobalVariables()->baseDevice.WiFiTimer);
    }
    if (var == CommandSensorTimer)
    {
        return String(GetGlobalVariables()->baseDevice.SensorTimer);
    }
    if (var == CommandSetPort)
    {
        return String(GetGlobalVariables()->baseDevice.port);
    }
    if (var == "debugmessage")
    {
        if (GetGlobalVariables()->baseDevice.debugMessage)
        {
            return "checked";
        }
        else
        {
            return "";
        }
    }
    if (var == "deviceName")
    {
        return GetGlobalVariables()->baseDevice.Name;
    }
    return String();
}

void StartWebServer()
{
  DebugMessage("DNS on " + WiFi.softAPIP().toString());
    //GetDns()->start(53, "*", WiFi.softAPIP());
    //DebugMessage("DNS set");
    //server.addHandler(new CaptiveRequestHandler()).setFilter(ON_AP_FILTER); // only when requested from AP
    DebugMessage("Webserver started");

    server.on("/", HTTP_GET, [](AsyncWebServerRequest *request)
              { 
                request->send_P(200, "text/html", INDEX_HTML, processor); 
              // if(GetGlobalVariables()->initialized)
              //   request->send_P(200, "text/html", INDEX_HTML, processor); 
              // else
              //   request->send_P(200, "text/html", WELCOME_PAGE_HTML, processor); 
              });

    server.on("/wifi", HTTP_POST, [](AsyncWebServerRequest *request)
              {
    if (request->hasParam(CommandWifiName, true))
    {
      HandleCommand("",CommandWifiName, 0, request->getParam(CommandWifiName, true)->value());
    }
    if (request->hasParam(CommandWifiPassword, true))
    {
      HandleCommand("",CommandWifiPassword, 0, request->getParam(CommandWifiPassword, true)->value());
    }
    SaveBaseDevice( GetGlobalVariables()->baseDevice);
    HandleCommand("",CommandReconectWifi, 120, "");
    request->redirect("/"); });

    server.on("/reboot", HTTP_POST, [](AsyncWebServerRequest *request)
              {
              request->send(200, "text/plain", "rebooting");
              HandleCommand("",CommandReboot, 0, ""); });
    server.on("/reboot.html", HTTP_POST, [](AsyncWebServerRequest *request)
              {
              request->send(200, "text/plain", "rebooting");
              HandleCommand("",CommandReboot, 0, ""); });

    server.on("/reboot.html", HTTP_GET, [](AsyncWebServerRequest *request)
              {
              request->redirect("/");
              HandleCommand("",CommandReboot, 0, ""); });
    server.on("/device", HTTP_POST, [](AsyncWebServerRequest *request)
              {
    DebugMessage("Device settings");
    if (request->hasParam(CommandSetId, true))
    {
      HandleCommand("",CommandSetId, 0, request->getParam(CommandSetId, true)->value());
    }
    if (request->hasParam(CommandSetName, true))
    {
      HandleCommand("",CommandSetName, 0, request->getParam(CommandSetName, true)->value());
    }
    if(request->hasParam(CommandSetAdress, true)){
      HandleCommand("",CommandSetAdress, 0, request->getParam(CommandSetAdress, true)->value());
    }
    if (request->hasParam(CommandSetPort, true))
    {
      HandleCommand("",CommandSetPort, 0, request->getParam(CommandSetPort, true)->value());
    }
    if (request->hasParam(CommandWifiRefreshTimer, true))
    {
      HandleCommand("",CommandWifiRefreshTimer, request->getParam(CommandWifiRefreshTimer, true)->value().toInt(), "");
    }
    if (request->hasParam(CommandSensorTimer, true))
    {
      HandleCommand("",CommandSensorTimer, request->getParam(CommandSensorTimer, true)->value().toInt(), "");
    }
    if (request->hasParam(CommandDebugMessage, true)){
      HandleCommand("",CommandDebugMessage, 1, "");
    } else{
      HandleCommand("",CommandDebugMessage, 0, "");
    }
    HandleCommand("",CommandSaveDevice, 0, "");
    request->redirect("/"); });

    server.on("/setup", HTTP_POST, [](AsyncWebServerRequest *request)
              {
    DebugMessage("Device first setup");
    if (request->hasParam(CommandSetId, true))
    {
      HandleCommand("",CommandSetId, 0, request->getParam(CommandSetId, true)->value());
    }
    if (request->hasParam(CommandSetName, true))
    {
      HandleCommand("",CommandSetName, 0, request->getParam(CommandSetName, true)->value());
    }
    if(request->hasParam(CommandSetAdress, true)){
      HandleCommand("",CommandSetAdress, 0, request->getParam(CommandSetAdress, true)->value());
    }
    if (request->hasParam(CommandSetPort, true))
    {
      HandleCommand("",CommandSetPort, 0, request->getParam(CommandSetPort, true)->value());
    }
    if (request->hasParam(CommandWifiRefreshTimer, true))
    {
      HandleCommand("",CommandWifiRefreshTimer, request->getParam(CommandWifiRefreshTimer, true)->value().toInt(), "");
    }
    if (request->hasParam(CommandSensorTimer, true))
    {
      HandleCommand("",CommandSensorTimer, request->getParam(CommandSensorTimer, true)->value().toInt(), "");
    }
    if (request->hasParam(CommandDebugMessage, true)){
      HandleCommand("",CommandDebugMessage, 1, "");
    } else{
      HandleCommand("",CommandDebugMessage, 0, "");
    }
    HandleCommand("",CommandSaveDevice, 0, "");
    request->redirect("/"); });

    server.on("/actuatorSet", HTTP_POST, [](AsyncWebServerRequest *request)
              {
    DebugMessage("Actuator set");
    if (request->hasParam("idx", true) && request->hasParam(DTstringValue, true))
    {
      int idx = request->getParam("idx", true)->value().toInt();
      float numericValue = (request->getParam(DTstringValue, true)->value() == "on") ? 100 : 0;

      if (idx >= 0 && idx < GetGlobalVariables()->actuatorArrSize)
      {
        GetGlobalVariables()->actuatorsArr[idx].SetActuatorValue(numericValue, "%");
      }
    }
    request->redirect("/"); });

    server.on("/actuatorId", HTTP_POST, [](AsyncWebServerRequest *request)
              {
    DebugMessage("Actuator id set");
    if (request->hasParam("idx", true) && request->hasParam(DTActuatorId, true))
    {
      int idx = request->getParam("idx", true)->value().toInt();
      String strId = request->getParam(DTActuatorId, true)->value();

      if (idx >= 0 && idx < GetGlobalVariables()->actuatorArrSize && !strId.isEmpty())
      {
        strId.toCharArray(GetGlobalVariables()->actuatorsArr[idx].id, IdStringLength, 0);
        GetGlobalVariables()->actuatorsArr[idx].isInitialized = true;
        SaveActuatorIDs(GetGlobalVariables()->actuatorsArr, GetGlobalVariables()->actuatorArrSize);
      }
    }
    request->redirect("/"); });

    server.on("/sensorId", HTTP_POST, [](AsyncWebServerRequest *request)
              {
    DebugMessage("Sensor id set");
    if (request->hasParam("idx", true) && request->hasParam("sensorId", true))
    {
      int idx = request->getParam("idx", true)->value().toInt();
      String strId = request->getParam("sensorId", true)->value();

      if (idx >= 0 && idx < GetGlobalVariables()->sensorsArrSize && !strId.isEmpty())
      {
        strId.toCharArray(GetGlobalVariables()->sensorsArr[idx].id, IdStringLength, 0);
        GetGlobalVariables()->sensorsArr[idx].isInitialized = true;
        SaveSensorIDs(GetGlobalVariables()->sensorsArr, GetGlobalVariables()->sensorsArrSize);
      }
    }
    request->redirect("/"); });

    server.on("/activitycheck", HTTP_POST, [](AsyncWebServerRequest *request)
              {
    HandleCommand("", CommandActivityCheck, 0, "");
    request->send(200, "text/plain", "active"); });

    server.on("/cleareeprom", HTTP_POST, [](AsyncWebServerRequest *request)
              {
    request->send(200, "text/plain", "clearing");
    HandleCommand("", CommandClearEEPRom, 0, ""); });

    server.begin();
}

AsyncWebServer* GetWebServer(){
  DebugMessage("Get web server");
  return &server;
}
