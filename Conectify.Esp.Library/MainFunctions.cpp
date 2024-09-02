#if defined(ARDUINO_ARCH_ESP8266)
#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <ESP8266HTTPClient.h>
#include <ESPAsyncTCP.h>
#elif defined(ESP32)
#include <WiFi.h>
#include <ESPmDNS.h>
#include <HTTPClient.h>
#include <AsyncTCP.h>
#else
#error Architecture unrecognized by this code.
#endif

#include <WiFiUdp.h>
#include <ArduinoOTA.h>
#include <EEPROM.h>
#include <ArduinoWebsockets.h>
#include "MainFunctions.h"
#include "Arduino.h"
#include "EEPRomHandler.h"
#include "TickTimer.h"
#include "Sensors.h"
#include "BaseDevice.h"
#include "DecodingFunctions.h"
#include "DebugMessageLib.h"
#include "GlobalVariables.h"
#include "USBComm.h"
#include "CommandHandler.h"
#include "Int64String.h"
#include "WebServer.h"

using namespace websockets;

USBComm usb;
String lastIP = "";
WebsocketsClient websocketClient;
DNSServer dnsServer;
Value *watchedValuesArray;
int valuesArrayLength = 0;

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

void StartupMandatoryRoutine(int psensorArrSize, int pactuatorArrSize, void (*SensorsDeclarations)())
{
  delay(5000);
  // setCpuFrequencyMhz(80);
  WiFi.mode(WIFI_AP_STA);
  DebugMessage("Mandatory setup here! Sensors: " + String(psensorArrSize) + " Actuators: " + String(pactuatorArrSize));
  InitializeDevice(psensorArrSize, pactuatorArrSize, SensorsDeclarations);
  InitializeNetwork();
  DebugMessage("-------- END SETUP -------");
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
  // dnsServer.processNextRequest();

  if (websocketClient.available())
  {
    SendAllSensorsToServerIfNeeded();
    websocketClient.poll();
    ArduinoOTA.handle();
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
  GetGlobalVariables()->sensorsArr = new Sensor[GetGlobalVariables()->sensorsArrSize];
  GetGlobalVariables()->actuatorsArr = new Actuator[GetGlobalVariables()->actuatorArrSize];
  DebugMessage("Created empty arrays! Senzor size: " + String(psensorArrSize) + " Actuator size: " + String(pactuatorArrSize));
}

void InitializeDevice(int psensorArrSize, int pactuatorArrSize, void (*SensorsDeclarations)())
{
  CreateBaseDevice();
  DeclareSensorArraysInternal(psensorArrSize, pactuatorArrSize);

  (*SensorsDeclarations)();

  LoadSensorsFromEEPROM(EEPROM, GetGlobalVariables()->sensorsArr);
  LoadActuatorFromEEPROM(EEPROM, GetGlobalVariables()->actuatorsArr);
}

bool InitializeNetwork()
{
  StartWebServer();
  String otaName = String("Conectify - ") + String(GetGlobalVariables()->baseDevice.Name);
  StartOTA(otaName);
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
  WiFi.softAPdisconnect(true);
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

const IPAddress localIP(192, 168, 4, 1);      // the IP address the web server, Samsung requires the IP to be in public space
const IPAddress gatewayIP(192, 168, 4, 1);    // IP address of the network should be the same as the local IP for captive portals
const IPAddress subnetMask(255, 255, 255, 0); // no need to change: https://avinetworks.com/glossary/subnet-mask/

void InitializeAP()
{
  delay(500);
  WiFi.softAPConfig(localIP, gatewayIP, subnetMask);
  WiFi.softAP("ConectifyAP", emptyString, 2, 0, 1);
  DebugMessage("AP created");
  DebugMessage(WiFi.softAPIP().toString());
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
    DebugMessage("Sending via websocket: " + message);
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
      SendViaWebSocket(GetGlobalVariables()->actuatorsArr[i].SerializeResponse(GetGlobalVariables()->dateTime));
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
        SendViaWebSocket(GetGlobalVariables()->sensorsArr[i].SerializeValue(GetGlobalVariables()->dateTime));
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
    GetGlobalVariables()->dateTime.decodeTime(timeNum);
  }
}

void StartOTA(String OTAName)
{
  DebugMessage("Registering OTA");

  int n = OTAName.length();
  char char_array[n + 1];
  strcpy(char_array, OTAName.c_str());

  ArduinoOTA.setHostname(char_array);
  ArduinoOTA.onStart([]()
                     {
    String type;
    if (ArduinoOTA.getCommand() == U_FLASH) {
      type = "sketch";
    } else { // U_FS
      type = "filesystem";
    }

    DebugMessage("Start updating " + type); });
  ArduinoOTA.onEnd([]()
                   { DebugMessage("OTA Failed"); });
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
    DebugMessage("Not online. Cannot register any device!");
  }
}

void HandleIncomingValue(String source, float value, String stringValue, String unit)
{
  if(valuesArrayLength == 0){
    DebugMessage("Values array is empty");
    return;
  }

  char id[IdStringLength];
  source.toCharArray(id, IdStringLength, 0);

  for (int i = 0; i < valuesArrayLength; i++)
  {
    if (!strcmp(id, watchedValuesArray[i].SourceId))
    {
      watchedValuesArray[i].hasChanged = true;
      watchedValuesArray[i].NumericValue = value;
      watchedValuesArray[i].stringValue = stringValue;
      watchedValuesArray[i].unit = unit;
      DebugMessage("Set values for " + source);
      return;
    }
  }
}

void SetWatchDog(Value valuesArray[], int valuesLength)
{
  valuesArrayLength = valuesLength;
  watchedValuesArray = valuesArray;
}

void onMessageCallback(WebsocketsMessage message)
{
  DebugMessage("Got websocket: ");
  DebugMessage(message.data());
  decodeIncomingJson(
      message.data(),
      HandleCommand,
      HandleIncomingValue,
      GetGlobalVariables()->dateTime,
      GetGlobalVariables()->actuatorsArr,
      GetGlobalVariables()->actuatorArrSize);
}

DNSServer *GetDns()
{
  return &dnsServer;
}