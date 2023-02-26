#include "Arduino.h"
#include "DecodingFunctions.h"
#include "ArduinoJson.h"
#include "BaseThing.h"
#include "ESP8266HTTPClient.h"
#include "Sensors.h"
#include "DebugMessageLib.h"
#include "ESP8266WiFi.h"
#include "ConstantsDeclarations.h"
#include "Thing.h"
#include <WiFiClient.h>

WiFiClient wifiClient;

struct HTTPResponse
{
  bool success = false;
  String payload;
};

HTTPResponse HTTPPost(String url, String payload)
{
  HTTPResponse response;
  DebugMessage("Sending request to: " + url);
  DebugMessage("Payload: " + payload);
  int watchdog = 0;
  while (watchdog < 3)
  {
    HTTPClient http;
    http.begin(wifiClient, url); // Specify request destination                         
    http.addHeader(HeaderContentType, HeaderJsonContentType); // Specify content-type header

    int httpCode = http.POST(payload); // Send the request
    DebugMessage("Got response: " + String(httpCode));
    response.success = httpCode == HttpOKCode;
    if (response.success)
    {
      String responsePayload = http.getString();        // Get the response payload
      DebugMessage("With payload: " + responsePayload); // Print request response payload
      response.payload = responsePayload;
      http.end();
      break;
    }
    watchdog ++;
    http.end();
  }
  return response;
}

String GetServer(BaseThing &baseThing)
{
  return String(baseThing.serverUrl) + ":" + String(baseThing.port);
}

#pragma region Thing

void DecodeRegisteredThingValue(String response, BaseThing &baseThing)
{
  response.toCharArray(baseThing.id, IdStringLength, 1);
  DebugMessage("Base device id set to: " + String(baseThing.id));
}

void RegisterBaseThing(BaseThing &baseThing, ESP8266WiFiClass WiFi, Thing thing)
{
  // DebugMessage("Registering base thing");
  String url = httpPrefix + GetServer(baseThing) + inputThingSuffix;

  HTTPResponse response = HTTPPost(url, serializeThing(baseThing, WiFi, thing));
  if(response.success){
    DecodeRegisteredThingValue(response.payload, baseThing);
  }
}

String serializeThing(BaseThing &baseThing, ESP8266WiFiClass WiFi, Thing thing)
{
  DynamicJsonDocument doc(384);
  doc[DTid] = baseThing.id;
  doc[position][description] = thing.thingPositionDescription;
  doc[position][posLat] = thing.latitude;
  doc[position][posLong] = thing.longitude;
  doc[type] = IoTThingType;
  doc[ipAdress] = WiFi.localIP().toString();
  doc[macAdress] = WiFi.macAddress();
  doc[thingName] = thing.thingName;
  String json;
  serializeJson(doc, json);
  doc.clear();
  return json;
}

#pragma endregion

#pragma region Sensor
void RegisterSensor(Sensor &sensor, BaseThing &baseThing)
{
  DebugMessage("-------------Registering sensor-------------------");
  String url = httpPrefix + GetServer(baseThing) + inputSensorSuffix;

  HTTPResponse response = HTTPPost(url, sensor.SerializeSensor(baseThing.id));
  //repeat with no ID to ensure that invalid ID is not an issue
  if (!response.success)
  {
    DebugMessage("First time did not work. Could not read sensor!");
    sensor.isInitialized = false;
    response = HTTPPost(url, sensor.SerializeSensor(baseThing.id));
  }

  if(response.success){
    DecodeRegisteredSensorValue(response.payload, sensor);
  }
  DebugMessage("---------------END Reg. SENSOR-------------------");
}

void DecodeRegisteredSensorValue(String payload, Sensor &sensor)
{
  DebugMessage("Retrieved ID is: " + payload);
  payload.toCharArray(sensor.id, IdStringLength, 1);
  sensor.isInitialized = true;
  DebugMessage("Actuator has saved ID: " + String(sensor.id));
}
#pragma endregion

#pragma region IncomingValue
void decodeIncomingJson(String incomingJson,
                        void (*handleFunc)(String commandText, float commandValue, String commandTextParam),
                        Time dateTime,
                        Actuator *actuators,
                        byte actuatorsLength)
{
  StaticJsonDocument<64> filter;
  filter[type] = true;
  filter[DTvalueName] = true;
  filter[DTstringValue] = true;
  filter[DTnumericValue] = true;
  filter[DTdestinationId] = true;

  DynamicJsonDocument root(1024);
  auto error = deserializeJson(root, incomingJson);

  if (error)
  {
    DebugMessage(incomingJson);
    DebugMessage("deserializeJson() failed: ");
    return;
  }
  DebugMessage("decoding object");
  if (root[type] == CommandType)
  {
    String commandText = root[DTvalueName];
    float commandValue = root[DTnumericValue];
    String commandTextparameter = root[DTstringValue];

    DebugMessage("got command: " + commandText + " with num value: " + commandValue + " and text value: " + commandTextparameter);

    (*handleFunc)(commandText, commandValue, commandTextparameter);
    filter.clear();
    root.clear();
    return;
  }

  if (root[type] == ActionType)
  {
    DebugMessage("recognized acton");

    if (!root[DTdestinationId].isNull())
    {

      String strId = root[DTdestinationId].as<String>();
      DebugMessage("Message for: " + strId);
      if (!strId.isEmpty())
      {
        char id[IdStringLength];
        strId.toCharArray(id, IdStringLength, 0);

        for (int i = 0; i < actuatorsLength; i++)
        {
          DebugMessage("Targer ID" + String(actuators[i].id));
          if (actuators[i].isInitialized && !strcmp(id, actuators[i].id))
          {
            DebugMessage("Found target!");
            actuators[i].lastActionId = root[DTid].as<String>();
            if (!root[DTstringValue].isNull())
              actuators[i].SetActuatorValue(root[DTstringValue].as<String>(), root[DTvalueUnit].as<String>());
            if (!root[DTnumericValue].isNull())
              actuators[i].SetActuatorValue(root[DTnumericValue].as<float>(), root[DTvalueUnit].as<String>());
          }
        }
      }
    }
    else
    {
      DebugMessage("got empty destination");
    }
    String time = root[timeCreated];
    DebugMessage(time);
    // TODO update system time
    filter.clear();
    root.clear();
    return;
  }

  String type = root[type];
  DebugMessage("Type is " + type);
  filter.clear();
  root.clear();
}

String RequestTime(BaseThing baseThing)
{
  String payload = "!";
  HTTPClient http;
  String url = httpPrefix + GetServer(baseThing) + timeSuffix;

  DebugMessage("Requesting time at: " + url);
  http.begin(wifiClient, url); // Specify request destination

  int httpCode = http.GET(); // Send the request
  DebugMessage("Got response with decodingThing: " + String(httpCode));
  if (httpCode == HttpOKCode)
  {
    payload = http.getString(); // Get the response payload
    DebugMessage(payload);      // Print request response payload
  }
  http.end();
  return payload;
}
#pragma endregion

#pragma region Actuator

void RegisterActuator(Actuator &actuator, BaseThing &baseThing)
{
  DebugMessage("-------------Registering actuator-------------------");
  
  String url = httpPrefix + GetServer(baseThing) + inputActuatorSuffix;
  int watchdog = 0;

  HTTPResponse response = HTTPPost(url, actuator.SerializeActuator(baseThing.id));
  //repeat with no ID to ensure that invalid ID is not an issue
  if (!response.success)
  {
    DebugMessage("First time did not work. Could not read actuator!");
    actuator.isInitialized = false;
    response = HTTPPost(url, actuator.SerializeActuator(baseThing.id));
  }

  if(response.success){
    DecodeRegisteredActuatorValue(response.payload, actuator);
  }
  DebugMessage("-------------End reg. actuator--------------------");
}

void DecodeRegisteredActuatorValue(String payload, Actuator &actuator)
{
  payload.toCharArray(actuator.id, IdStringLength, 1);
  actuator.isInitialized = true;
  DebugMessage("Actuator has saved ID: " + String(actuator.id));
}
#pragma endregion

#pragma region CommandResponse

String CreateCommandResponse(BaseThing thing, Time dateTime)
{
  DynamicJsonDocument doc(256);
  doc[type] = CommandResponseType;
  doc[DTSourceId] = thing.id;
  doc[timeCreated] = dateTime.ToJSONString();
  doc[DTvalueName] = "";
  doc[DTstringValue] = "";
  doc[DTnumericValue] = 0;
  doc[DTvalueUnit] = "";
  String json;
  serializeJson(doc, json);
  doc.clear();
  return json;
}

#pragma endregion
