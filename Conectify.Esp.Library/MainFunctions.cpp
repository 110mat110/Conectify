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
#include <WiFiManager.h>
#include "TickTimer.h"
#include "Sensors.h"
#include "BaseThing.h"
#include "DecodingFunctions.h"
#include "DebugMessageLib.h"
#include "GlobalVariables.h"
#include "USBComm.h"
#include "Thing.h"
#include "CommandHanlder.h"

using namespace websockets;

bool onLine = false; //wifi status
USBComm usb;
bool requestAcc = true;
bool sendSensors = true;
WebsocketsClient client;
WiFiManager wm;
char str[6];

WiFiManagerParameter thingId("thingId", "Thing id", GetGlobalVariables()->baseThing.id, 37);
WiFiManagerParameter siteName("siteName", "Name of server", "",  40);
WiFiManagerParameter sitePort("sitePort", "Port of the server", "", 6);
Thing thing;

void InitializeNetwork(Thing insertedThing);
void InitializeDevice(int psensorArrSize, int pactuatorArrSize, void(*SensorsDeclarations)(), Thing insertedThing);

void HandleWebSetup();
void ReceiveSerialConnection();

bool ConnectToWifi();
void RecievedMessageRoutine();
void SendAllSensorsToServerIfNeeded();
void CreateBaseThing();
void RegisterAllEntities(Thing thing);
void StartOTA(String OTAName);
void AskServerForTime();
void DisconnectWiFi();
void saveParamsCallback();
void onMessageCallback(WebsocketsMessage message);
void OneTimeWifiManagerSetup();

void DeclareSensorArraysInternal(int psensorArrSize, int pactuatorArrSize){
    GetGlobalVariables()->sensorsArrSize = psensorArrSize;
    GetGlobalVariables()->actuatorArrSize = pactuatorArrSize;
    DebugMessage("Generated sensor array");
    GetGlobalVariables()->sensorsArr = new Sensor[GetGlobalVariables()->sensorsArrSize];
    GetGlobalVariables()->actuatorsArr = new Actuator[GetGlobalVariables()->actuatorArrSize];
}

void StartupMandatoryRoutine(int psensorArrSize, int pactuatorArrSize, void(*SensorsDeclarations)(), Thing insertedThing){
  Serial.begin(115200);
  delay(5000);
  DebugMessage("Mandatory setup here!" + String(psensorArrSize) + String(pactuatorArrSize));
  InitializeDevice(psensorArrSize, pactuatorArrSize, SensorsDeclarations, insertedThing);
  OneTimeWifiManagerSetup();
  InitializeNetwork(insertedThing);
  DebugMessage("End of setup");
}

void LoopMandatoryRoutines(){
  //DO NOT USE ANY DELAY IN LOOP, you will kill OTA function
  HandleWebSetup();
  ReceiveSerialConnection();
  RecievedMessageRoutine();
  if(GetGlobalVariables()->WiFiRestartRequired()){
    ConnectToWifi();
    RegisterAllEntities(thing);
  }

  if(GetGlobalVariables()->EEPROMWriteRequired()){  
    SaveToEEPRom(EEPROM, GetGlobalVariables()->baseThing);
    RegisterAllEntities(thing);
  }
  //END OF LOOP SERVER ROUTINES
}

void InitializeDevice(int psensorArrSize, int pactuatorArrSize, void(*SensorsDeclarations)(), Thing insertedThing){
  thing = insertedThing;
  CreateBaseThing();
  DebugMessage("Created base thing");
  DeclareSensorArraysInternal(psensorArrSize,pactuatorArrSize);
  DebugMessage("Internal sensor array declared");
  (*SensorsDeclarations)();
      if(!GetGlobalVariables()->sensorsArr[0].isInitialized)
        DebugMessage("Sensor declared");
  LoadSensorsFromEEPROM(EEPROM, GetGlobalVariables()->sensorsArr);
  LoadActuatorFromEEPROM(EEPROM, GetGlobalVariables()->actuatorsArr);

  thingId.setValue(GetGlobalVariables()->baseThing.id, 37);
  sitePort.setValue(GetGlobalVariables()->baseThing.port, 6);
  siteName.setValue(GetGlobalVariables()->baseThing.serverUrl, 35);
}

void InitializeNetwork(Thing insertedThing){
   if(GetGlobalVariables()->initialized){

    if(wm.autoConnect("ConectifyAP")){
      DebugMessage("Wifi connected, registering OTA");
      StartOTA("Conectify");
      DebugMessage("Registering entites");
      RegisterAllEntities(insertedThing);
      DebugMessage("Requsting time");
      AskServerForTime();
      DebugMessage("Starting web socket");
      client.onMessage(onMessageCallback);
      client.connect(GetGlobalVariables()->baseThing.serverUrl, String(GetGlobalVariables()->baseThing.port).toInt(), inputWebSocketSuffix+String(GetGlobalVariables()->baseThing.id));
      client.ping();
      wm.startWebPortal();
    }
  }
}

void OneTimeWifiManagerSetup(){
    wm.setDarkMode(true);
    wm.addParameter(&thingId);
    wm.addParameter(&siteName);
    wm.addParameter(&sitePort);
    wm.setConfigPortalBlocking(false);
    wm.setBreakAfterConfig(true);
    wm.setSaveParamsCallback(saveParamsCallback);
    wm.setParamsPage(true);
}

void saveParamsCallback() {
  DebugMessage("Save params called");
  //ToDo ach...
  String(thingId.getValue()).toCharArray(GetGlobalVariables()->baseThing.id, 37);
  String(siteName.getValue()).toCharArray(GetGlobalVariables()->baseThing.serverUrl, 40);
  String(sitePort.getValue()).toCharArray(GetGlobalVariables()->baseThing.port, 6);

  SaveToEEPRom(EEPROM, GetGlobalVariables()->baseThing);

  thingId.setValue(GetGlobalVariables()->baseThing.id, 37);
  sitePort.setValue(GetGlobalVariables()->baseThing.port, 6);
  siteName.setValue(GetGlobalVariables()->baseThing.serverUrl, 35);

  InitializeNetwork(thing);
}

void HandleWebSetup()
{
  wm.startWebPortal();
  wm.process();
}

void ReceiveSerialConnection(){
    while (Serial.available() > 0) {
    // read the incoming byte:
    char inChar = (char)Serial.read();
    usb.ProcessIncomingChar(inChar);
    delay(10);
  }
}

void SendAllSensorsToServerIfNeeded(){
  if(!onLine){
    DebugMessage("trying to connect to server withouth wifi!");
    return;
  }

  if(sendSensors){
    for(int i = 0; i<GetGlobalVariables()->sensorsArrSize; i++){
      DebugMessage("Has sensor changed?" + String(GetGlobalVariables()->sensorsArr[i].HasChanged()));
      if(GetGlobalVariables()->sensorsArr[i].HasChanged()){
        GetGlobalVariables()->sensorsArr[i].MarkAsRead();
        SendSensorValuesToServer(
          GetGlobalVariables()->sensorsArr[i],
          GetGlobalVariables()->baseThing, 
          GetGlobalVariables()->dateTime,
          HandleCommand,
          GetGlobalVariables()->actuatorsArr,
          GetGlobalVariables()->actuatorArrSize
        );
        requestAcc = false;
      }
    }
    sendSensors = false;
  }
}

void RegisterSensors(){
  for(int i= 0; i< GetGlobalVariables()->sensorsArrSize; i++){
    RegisterSensor(GetGlobalVariables()->sensorsArr[i], GetGlobalVariables()->baseThing);
    DebugMessage("Sensor registrated with ID:" + String(GetGlobalVariables()->sensorsArr[i].id));
  }
}
void RegisterActuator(){
    for(int i= 0; i< GetGlobalVariables()->actuatorArrSize; i++){
       RegisterActuator(GetGlobalVariables()->actuatorsArr[i], GetGlobalVariables()->baseThing);
    DebugMessage("ActuatorRegistered registrated with ID:" + String(GetGlobalVariables()->actuatorsArr[i].id));
  }
}

void CreateBaseThing(){
      EEPROM.begin(512);
    if(EEPROM.read(0) == 0xFF){
        GetGlobalVariables()->initialized = false;
        DebugMessage("FreshEEPROM, not initialized!");
        EEPROM.end();
    }
    else
    {
      EEPROM.end();
        ReadFromEEPRom(EEPROM,GetGlobalVariables()->baseThing);
        GetGlobalVariables()->SetSensoricTimerInSeconds(GetGlobalVariables()->baseThing.SensorTimer);
        GetGlobalVariables()->SetWiFiTimerInSeconds(GetGlobalVariables()->baseThing.WiFiTimer);
        GetGlobalVariables()->initialized = true;
        DebugMessage("Loaded data from EEPROM");
    }
}

bool ConnectToWifi(){
  if(WiFi.status() == WL_CONNECTED){
    return true;
  }


  GetGlobalVariables()->SetLedOFF();
  WiFi.mode(WIFI_STA);
  WiFi.begin(GetGlobalVariables()->baseThing.ssid, GetGlobalVariables()->baseThing.password);
  WiFi.waitForConnectResult() != WL_CONNECTED;
  for(int i =0; i<10; i++){
    GetGlobalVariables()->InvertLed();
    delay(500);
  }
  onLine = (WiFi.status() == WL_CONNECTED);
  if(!onLine){
    for(int i =0; i<10; i++){
      GetGlobalVariables()->InvertLed();
      delay(50);
    }
  }
  
  GetGlobalVariables()->SetLedOFF();
  return WiFi.status() == WL_CONNECTED;
}

void DisconnectWiFi(){
  if(WiFi.status() != WL_CONNECTED){
    WiFi.disconnect();
  }

  onLine = (WiFi.status() == WL_CONNECTED);
}

 void AskServerForTime(){
  int counter =0;
  String time = "!";
  do{
    time = RequestTime(GetGlobalVariables()->baseThing);


    counter++;
  } while(time == "!" && counter < 5);
  if(time != "!")
    GetGlobalVariables()->dateTime.decodeTime((long) strtol(time.c_str(),NULL,0));
  }

void StartOTA(String OTAName){
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

  ArduinoOTA.onStart([]() {
    String type;
    if (ArduinoOTA.getCommand() == U_FLASH) {
      type = "sketch";
    } else { // U_FS
      type = "filesystem";
    }

    // NOTE: if updating FS this would be the place to unmount FS using FS.end()
    DebugMessage("Start updating " + type);
  });
  ArduinoOTA.onEnd([]() {
    DebugMessage("\nEnd");
  });
  ArduinoOTA.onProgress([](unsigned int progress, unsigned int total) {
    Serial.printf("Progress: %u%%\r", (progress / (total / 100)));
  });
  ArduinoOTA.onError([](ota_error_t error) {
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
    }
  });
  ArduinoOTA.begin();
}

void RecievedMessageRoutine(){
    if(usb.ReadMessage()){

      DebugMessage("Recieved message routine");
  }
}

void RegisterAllEntities(Thing thing){
  if(onLine){
      RegisterBaseThing(GetGlobalVariables()->baseThing, WiFi, thing);
      SaveToEEPRom(EEPROM,GetGlobalVariables()->baseThing);
      RegisterSensors();
      SaveSensorIDsToEEPROM(EEPROM, GetGlobalVariables()->sensorsArr, GetGlobalVariables()->sensorsArrSize);
      RegisterActuator();
      SaveActuatorIDsToEEPROM(EEPROM, GetGlobalVariables()->actuatorsArr, GetGlobalVariables()->actuatorArrSize);
  }
}

void onMessageCallback(WebsocketsMessage message) {
    Serial.print("Got Message: ");
    Serial.println(message.data());
}

void RequestActuatorValues(){
  requestAcc = true;
}
void SendSensoricValues(){
  sendSensors = true;
}
