#include "MainFunctions.h"
#include "Arduino.h"
#include "EEPRomHandler.h"
#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <WiFiUdp.h>
#include <ArduinoOTA.h>
#include <ESP8266HTTPClient.h>
#include <EEPROM.h>
#include <ArduinoWebsockets.h>
#include <ESPAsyncTCP.h>
#include <ESPAsyncWebServer.h>
#include "TickTimer.h"
#include "Sensors.h"
#include "BaseDevice.h"
#include "DecodingFunctions.h"
#include "DebugMessageLib.h"
#include "GlobalVariables.h"
#include "USBComm.h"
#include "CommandHanlder.h"
#include "Int64String.h"
#include "WebServer.h"

using namespace websockets;

USBComm usb;
String lastIP = "";
WebsocketsClient websocketClient;
AsyncWebServer server(80);

bool InitializeNetwork();
void InitializeDevice(int psensorArrSize, int pactuatorArrSize, void (*SensorsDeclarations)());
void ReceiveSerialConnection();
void RecievedMessageRoutine();
void SendAllSensorsToServerIfNeeded();
void CreateBaseDevice();
void RegisterAllEntities();
void StartOTA(String OTAName);
void AskServerForTime();
void onMessageCallback(WebsocketsMessage message);
void onEventsCallback(WebsocketsEvent event, String data);
void ConnectWebsocket();
bool ConnectToWifi();
void InitializeAP();
void StartWebServer();
bool ReloadNetwork();
bool IsWiFi();

void StartupMandatoryRoutine(int psensorArrSize, int pactuatorArrSize, void (*SensorsDeclarations)())
{
  WiFi.mode(WIFI_AP_STA);
  Serial.begin(115200);
  delay(5000);
  DebugMessage("Mandatory setup here! Sensors: " + String(psensorArrSize) + " Actuators: " + String(pactuatorArrSize));
  InitializeDevice(psensorArrSize, pactuatorArrSize, SensorsDeclarations);
  InitializeNetwork();
  DebugMessage("End of setup");
}

void LoopMandatoryRoutines()
{
  // DO NOT USE ANY DELAY IN LOOP, you will kill OTA function
  ReceiveSerialConnection();
  RecievedMessageRoutine();
  if (GetGlobalVariables()->RestartRequired())
  {
    ESP.restart();
  }

  if (GetGlobalVariables()->EEPROMWriteRequired())
  {
    SaveToEEPRom(EEPROM, GetGlobalVariables()->baseDevice);
    RegisterAllEntities();
  }

  if (!IsWiFi() && GetGlobalVariables()->WifiTimer.IsTriggeredNoReset())
  {
    ReloadNetwork();
  }

  if (websocketClient.available())
  {
    SendAllSensorsToServerIfNeeded();
    websocketClient.poll();
  }
  else
  {
    if (IsWiFi())
    {
      ConnectWebsocket();
    }
  }
  //  END OF LOOP SERVER ROUTINES
}

void DeclareSensorArraysInternal(int psensorArrSize, int pactuatorArrSize)
{
  GetGlobalVariables()->sensorsArrSize = psensorArrSize;
  GetGlobalVariables()->actuatorArrSize = pactuatorArrSize;
  DebugMessage("Generated sensor array");
  GetGlobalVariables()->sensorsArr = new Sensor[GetGlobalVariables()->sensorsArrSize];
  GetGlobalVariables()->actuatorsArr = new Actuator[GetGlobalVariables()->actuatorArrSize];
}

void InitializeDevice(int psensorArrSize, int pactuatorArrSize, void (*SensorsDeclarations)())
{
  CreateBaseDevice();
  DebugMessage("Created base device");
  DeclareSensorArraysInternal(psensorArrSize, pactuatorArrSize);
  DebugMessage("Internal sensor array declared");
  (*SensorsDeclarations)();
  if (!GetGlobalVariables()->sensorsArr[0].isInitialized)
    DebugMessage("Sensor declared");
  LoadSensorsFromEEPROM(EEPROM, GetGlobalVariables()->sensorsArr);
  LoadActuatorFromEEPROM(EEPROM, GetGlobalVariables()->actuatorsArr);
}

bool InitializeNetwork()
{
  StartWebServer();
  // StartOTA("Conectify");  TODO play with OTA later
  return ReloadNetwork();
}

bool ReloadNetwork()
{
  if (/*GetGlobalVariables()->initialized &&*/ ConnectToWifi())
  {
    RegisterAllEntities();
    AskServerForTime();
    ConnectWebsocket();
    return true;
  }
  InitializeAP();
  return false;
}

bool ConnectToWifi()
{
  GlobalVariables().SetLedOFF();

  // WiFi.mode(WIFI_STA);
  // WiFi.enableSTA(true);
  delay(1000);
  WiFi.hostname(GetGlobalVariables()->baseDevice.Name);
  WiFi.begin(GetGlobalVariables()->baseDevice.ssid, GetGlobalVariables()->baseDevice.password);

  DebugMessage("Connecting to WiFi " + String(GetGlobalVariables()->baseDevice.ssid) + " with pass: " + String(GetGlobalVariables()->baseDevice.password));
  int watchdog = 0;
  GetGlobalVariables()->WifiTimer.ResetTimer();
  do
  {
    if (watchdog > 15)
    {
      DebugMessage("Could not connect to wifi");
      for (short i = 0; i < 3; i++)
      {
        GlobalVariables().SetLedON();
        delay(150);
        GlobalVariables().SetLedOFF();
        delay(150);
      }

      return false;
    }
    GlobalVariables().SetLedON();
    delay(500);
    GlobalVariables().SetLedOFF();
    delay(500);
    watchdog++;
  } while (!IsWiFi());
  DebugMessage("Successfully connected!");
  WiFi.softAPdisconnect(false);
  return true;
}

bool IsWiFi()
{
  bool wifi = (WiFi.localIP().toString().startsWith("192.168.")) && WiFi.SSID() == String(GetGlobalVariables()->baseDevice.ssid);
  // TODO this is only informative to get IP adress
  if (GetGlobalVariables()->baseDevice.debugMessage)
  {
    if (WiFi.localIP().toString() != lastIP)
    {
      DebugMessage("Local ip: " + WiFi.localIP().toString());
      lastIP = WiFi.localIP().toString();
    }
  }
  return wifi;
}

void InitializeAP()
{
  delay(500);
  WiFi.softAP("ConectifyAP", emptyString, 2, 0, 1);
  DebugMessage("AP created");
  Serial.println(WiFi.softAPIP());
  GlobalVariables().SetLedON();
}

void ConnectWebsocket()
{
  DebugMessage("Starting web socket");
  websocketClient = {};
  websocketClient.onMessage(onMessageCallback);
  websocketClient.onEvent(onEventsCallback);
  DebugMessage("url: " + String(GetGlobalVariables()->baseDevice.serverUrl));
  DebugMessage("port: " + String(String(GetGlobalVariables()->baseDevice.port).toInt()));
  DebugMessage("suffix: " + inputWebSocketSuffix + String(GetGlobalVariables()->baseDevice.id));
  websocketClient.connect(GetGlobalVariables()->baseDevice.serverUrl, String(GetGlobalVariables()->baseDevice.port).toInt(), inputWebSocketSuffix + String(GetGlobalVariables()->baseDevice.id) + "/");
  websocketClient.ping();
}

void SendViaWebSocket(String message)
{
  if (websocketClient.available())
  {
    websocketClient.send(message);
  }
  else
  {
    DebugMessage("Cannot send message, websocket is not active. Trying to reconnect");
    ConnectWebsocket();
  }
}

void ReceiveSerialConnection()
{
  while (Serial.available() > 0)
  {
    // read the incoming byte:
    char inChar = (char)Serial.read();
    usb.ProcessIncomingChar(inChar);
    delay(10);
  }
}

void SendAllSensorsToServerIfNeeded()
{
  if (!IsWiFi())
  {
    DebugMessage("trying to connect to server withouth wifi!");
    return;
  }

  for (int i = 0; i < GetGlobalVariables()->actuatorArrSize; i++)
  {
    if (GetGlobalVariables()->actuatorsArr[i].HasChanged())
    {
      DebugMessage("--------- Actuator response ------------------------");
      DebugMessage("Payload: " + GetGlobalVariables()->actuatorsArr[i].SerializeResponse(GetGlobalVariables()->dateTime));
      websocketClient.send(GetGlobalVariables()->actuatorsArr[i].SerializeResponse(GetGlobalVariables()->dateTime));
      GetGlobalVariables()->actuatorsArr[i].MarkAsRead();
    }
  }
  for (int i = 0; i < GetGlobalVariables()->sensorsArrSize; i++)
  {
    if (GetGlobalVariables()->sensorsArr[i].HasChanged())
    {
      if (GetGlobalVariables()->sensorsArr[i].isInitialized)
      {
        DebugMessage("--------- Sensor Value ------------------------");
        DebugMessage("Payload: " + GetGlobalVariables()->sensorsArr[i].SerializeValue(GetGlobalVariables()->dateTime));
        websocketClient.send(GetGlobalVariables()->sensorsArr[i].SerializeValue(GetGlobalVariables()->dateTime));
      }
      GetGlobalVariables()->sensorsArr[i].MarkAsRead();
    }
  }
}

void RegisterSensors()
{
  for (int i = 0; i < GetGlobalVariables()->sensorsArrSize; i++)
  {
    RegisterSensor(GetGlobalVariables()->sensorsArr[i], GetGlobalVariables()->baseDevice);
    DebugMessage("Sensor registrated with ID: " + String(GetGlobalVariables()->sensorsArr[i].id));
  }
}
void RegisterActuator()
{
  for (int i = 0; i < GetGlobalVariables()->actuatorArrSize; i++)
  {
    RegisterActuator(GetGlobalVariables()->actuatorsArr[i], GetGlobalVariables()->baseDevice);
    DebugMessage("Actuator registrated with ID: " + String(GetGlobalVariables()->actuatorsArr[i].id));
  }
}

void onEventsCallback(WebsocketsEvent event, String data) {}

void CreateBaseDevice()
{
  EEPROM.begin(512);
  if (EEPROM.read(0) == 0xFF)
  {
    GetGlobalVariables()->initialized = false;
    DebugMessage("FreshEEPROM, not initialized!");
    EEPROM.end();
  }
  else
  {
    EEPROM.end();
    ReadFromEEPRom(EEPROM, GetGlobalVariables()->baseDevice);
    if (GetGlobalVariables()->baseDevice.WiFiTimer > 10)
    {
      GetGlobalVariables()->SetWiFiTimerInSeconds(GetGlobalVariables()->baseDevice.WiFiTimer);
    }
    GetGlobalVariables()->SetSensoricTimerInSeconds(GetGlobalVariables()->baseDevice.SensorTimer);
    GetGlobalVariables()->initialized = true;
    DebugMessage("Loaded data from EEPROM");
  }
}

void AskServerForTime()
{
  DebugMessage("Requsting time");
  int counter = 0;
  String time = "!";
  do
  {
    time = RequestTime(GetGlobalVariables()->baseDevice);

    counter++;
  } while (time == "!" && counter < 5);
  if (time != "!")
  {
    DebugMessage("Trying to decode time " + time);
    uint64_t timeNum = strtoull(time.c_str(), NULL, 0);
    DebugMessage("Time decoded: " + int64String(timeNum));
    GetGlobalVariables()->dateTime.decodeTime(timeNum);
  }
}

void StartOTA(String OTAName)
{
  DebugMessage("Registering OTA");
  // Port defaults to 8266
  // ArduinoOTA.setPort(8266);

  int n = OTAName.length();

  // declaring character array
  char char_array[n + 1];

  // copying the contents of the
  // string to char array
  strcpy(char_array, OTAName.c_str());

  // Hostname defaults to esp8266-[ChipID]
  ArduinoOTA.setHostname(char_array);

  // No authentication by default
  // ArduinoOTA.setPassword("admin");

  // Password can be set with it's md5 value as well
  // MD5(admin) = 21232f297a57a5a743894a0e4a801fc3
  // ArduinoOTA.setPasswordHash("21232f297a57a5a743894a0e4a801fc3");

  ArduinoOTA.onStart([]()
                     {
    String type;
    if (ArduinoOTA.getCommand() == U_FLASH) {
      type = "sketch";
    } else { // U_FS
      type = "filesystem";
    }

    // NOTE: if updating FS this would be the place to unmount FS using FS.end()
    DebugMessage("Start updating " + type); });
  ArduinoOTA.onEnd([]()
                   { DebugMessage("\nEnd"); });
  ArduinoOTA.onProgress([](unsigned int progress, unsigned int total)
                        { Serial.printf("Progress: %u%%\r", (progress / (total / 100))); });
  ArduinoOTA.onError([](ota_error_t error)
                     {
    Serial.printf("Error[%u]: ", error);
    if (error == OTA_AUTH_ERROR) {
      DebugMessage("Auth Failed");
    } else if (error == OTA_BEGIN_ERROR) {
      DebugMessage("Begin Failed");
    } else if (error == OTA_CONNECT_ERROR) {
      DebugMessage("Connect Failed");
    } else if (error == OTA_RECEIVE_ERROR) {
      DebugMessage("Receive Failed");
    } else if (error == OTA_END_ERROR) {
      DebugMessage("End Failed");
    } });
  ArduinoOTA.begin();
}

void RecievedMessageRoutine()
{
  if (usb.ReadMessage())
  {
    DebugMessage("Recieved message routine");
  }
}

void RegisterAllEntities()
{
  DebugMessage("Registering entites");
  if ((IsWiFi()))
  {
    RegisterBaseDevice(GetGlobalVariables()->baseDevice);
    SaveToEEPRom(EEPROM, GetGlobalVariables()->baseDevice);
    RegisterSensors();
    SaveSensorIDsToEEPROM(EEPROM, GetGlobalVariables()->sensorsArr, GetGlobalVariables()->sensorsArrSize);
    RegisterActuator();
    SaveActuatorIDsToEEPROM(EEPROM, GetGlobalVariables()->actuatorsArr, GetGlobalVariables()->actuatorArrSize);
  }
  else
  {
    DebugMessage("Not online. Cannot register anydevice!");
  }
}
void HandleCommand(String commandText, float commandValue, String commandTextParam)
{
  HandleCommand("", commandText, commandValue, commandTextParam);
}

void onMessageCallback(WebsocketsMessage message)
{
  DebugMessage("Got Message: ");
  DebugMessage(message.data());
  decodeIncomingJson(
      message.data(),
      HandleCommand,
      GetGlobalVariables()->dateTime,
      GetGlobalVariables()->actuatorsArr,
      GetGlobalVariables()->actuatorArrSize);
}

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
    return GetGlobalVariables()->baseDevice.password;
  }
  if (var == "wifi")
  {
    return IsWiFi() ? "<i class=\"material-icons\">wifi</i>" : "<i class=\"material-icons\">cloud</i>";
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
  DebugMessage("Webserver started");
  server.on("/", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send_P(200, "text/html", INDEX_HTML, processor); });

  server.on("/wifi", HTTP_POST, [](AsyncWebServerRequest *request)
            {
    if (request->hasParam(CommandWifiName, true))
    {
      HandleCommand(CommandWifiName, 0, request->getParam(CommandWifiName, true)->value());
    }
    if (request->hasParam(CommandWifiPassword, true))
    {
      HandleCommand(CommandWifiPassword, 0, request->getParam(CommandWifiPassword, true)->value());
    }
    SaveToEEPRom(EEPROM, GetGlobalVariables()->baseDevice);
    HandleCommand(CommandReconectWifi, 120, "");
    request->redirect("/"); });

  server.on("/reboot", HTTP_POST, [](AsyncWebServerRequest *request)
            {
    HandleCommand(CommandReboot, 0, "");
    request->send_P(200, "text/html", INDEX_HTML, processor); });

  server.on("/device", HTTP_POST, [](AsyncWebServerRequest *request)
            {
    DebugMessage("Device settings");
    if (request->hasParam(CommandSetId, true))
    {
      HandleCommand(CommandSetId, 0, request->getParam(CommandSetId, true)->value());
    }
    if (request->hasParam(CommandSetName, true))
    {
      HandleCommand(CommandSetName, 0, request->getParam(CommandSetName, true)->value());
    }
    if(request->hasParam(CommandSetAdress, true)){
      HandleCommand(CommandSetAdress, 0, request->getParam(CommandSetAdress, true)->value());
    }
    if (request->hasParam(CommandSetPort, true))
    {
      HandleCommand(CommandSetPort, 0, request->getParam(CommandSetPort, true)->value());
    }
    if (request->hasParam(CommandWifiRefreshTimer, true))
    {
      HandleCommand(CommandWifiRefreshTimer, request->getParam(CommandWifiRefreshTimer, true)->value().toInt(), "");
    }
    if (request->hasParam(CommandSensorTimer, true))
    {
      HandleCommand(CommandSensorTimer, request->getParam(CommandSensorTimer, true)->value().toInt(), "");
    }
    if (request->hasParam(CommandDebugMessage, true)){
      HandleCommand(CommandDebugMessage, 1, "");
    } else{
      HandleCommand(CommandDebugMessage, 0, "");
    }
    HandleCommand(CommandSaveDevice, 0, "");
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
