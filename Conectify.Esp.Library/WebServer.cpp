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
        return IsWiFi() ? "<i class=\"material-icons\">wifi</i>" : "<i class=\"material-icons\">cloud</i>";
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
            String currentSensorHtml = GetGlobalVariables()->sensorsArr[i].ShowHtml();
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
              HandleCommand("",CommandReboot, 0, "");
              request->redirect("/"); });
    server.on("/reboot.html", HTTP_POST, [](AsyncWebServerRequest *request)
              {
              HandleCommand("",CommandReboot, 0, "");
              request->redirect("/"); });

    server.on("/reboot.html", HTTP_GET, [](AsyncWebServerRequest *request)
              {
              HandleCommand("",CommandReboot, 0, "");
              request->redirect("/"); });
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
    if (request->hasParam(DTActuatorId, true) && request->hasParam(DTstringValue, true) )
    {
      String strId = request->getParam(DTActuatorId, true)->value();
      float numericValue = 0;
      if(request->getParam(DTstringValue, true)->value() == "on") {
        numericValue = 100;
      }

      if (!strId.isEmpty())
      {
        char id[IdStringLength];
        strId.toCharArray(id, IdStringLength, 0);

        for (int i = 0; i < GetGlobalVariables()->actuatorArrSize; i++)
        {
          DebugMessage("Targer ID" + String(GetGlobalVariables()->actuatorsArr[i].id));
          if (GetGlobalVariables()->actuatorsArr[i].isInitialized && !strcmp(id, GetGlobalVariables()->actuatorsArr[i].id))
          {
            DebugMessage("Found target!");
            GetGlobalVariables()->actuatorsArr[i].SetActuatorValue(numericValue, "%");
          }
        }
      }
    }
    request->redirect("/"); });
    server.begin();
}

AsyncWebServer* GetWebServer(){
  DebugMessage("Get web server");
  return &server;
}
