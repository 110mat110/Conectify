#if defined(ARDUINO_ARCH_ESP8266)
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#elif defined(ESP32)
#include <WiFi.h>
#include <HTTPClient.h>
#else
#error Architecture unrecognized by this code.
#endif

#include "Arduino.h"
#include "DecodingFunctions.h"
#include "ArduinoJson.h"
#include "BaseDevice.h"
#include "Sensors.h"
#include "DebugMessageLib.h"
#include "ConstantsDeclarations.h"

struct HTTPResponse
{
  bool success = false;
  String payload;
  int code = -1;
};

HTTPResponse HTTPGet(String url)
{

  HTTPResponse response;
  DebugMessage("Sending request to: " + url);
  // wifiClient.setTimeout(500);
  int watchdog = 0;
  while (watchdog < 3)
  {
    HTTPClient http;
    http.begin(/*wifiClient,*/ url); // Specify request destination

    int httpCode = http.GET(); // Send the request
    DebugMessage("Got response: " + String(httpCode));
    response.success = httpCode == HttpOKCode;
    response.code = httpCode;
    if (response.success)
    {
      String responsePayload = http.getString();        // Get the response payload
      DebugMessage("With payload: " + responsePayload); // Print request response payload
      response.payload = responsePayload;
      http.end();
      break;
    }
    if (response.code == 400 || response.code == 404)
    {
      http.end();
      break;
    }
    watchdog++;
    http.end();
  }

  return response;
}

HTTPResponse HTTPPost(String url, String payload)
{
  HTTPResponse response;
  DebugMessage("Sending request to: " + url);
  DebugMessage("Payload: " + payload);
  int watchdog = 0;
  // wifiClient.setTimeout(500);
  while (watchdog < 3)
  {
    HTTPClient http;
    http.begin(/*wifiClient,*/ url);                          // Specify request destination
    http.addHeader(HeaderContentType, HeaderJsonContentType); // Specify content-type header

    int httpCode = http.POST(payload); // Send the request
    DebugMessage("Got response: " + String(httpCode));
    response.success = httpCode == HttpOKCode;
    response.code = httpCode;
    if (response.success)
    {
      String responsePayload = http.getString();        // Get the response payload
      DebugMessage("With payload: " + responsePayload); // Print request response payload
      response.payload = responsePayload;
      http.end();
      break;
    }
    if (response.code == 400 || response.code == 404)
    {
      http.end();
      break;
    }
    watchdog++;
    http.end();
  }

  return response;
}

String GetServer(BaseDevice &baseDevice)
{
  return String(baseDevice.serverUrl) + ":" + String(baseDevice.port);
}

#pragma region Device
String SerializeDevice(BaseDevice &baseDevice, bool useId)
{
  DynamicJsonDocument doc(384);
  if (useId)
  {
    doc[DTid] = baseDevice.id;
  }
  else
  {
    doc[DTid] = "00000000-0000-0000-0000-000000000000";
  }
  doc[type] = IoTDeviceType;
  doc[ipAdress] = WiFi.localIP().toString();
  doc[macAdress] = WiFi.macAddress();
  doc[deviceName] = baseDevice.Name;
  String json;
  serializeJson(doc, json);
  doc.clear();
  return json;
}

void DecodeRegisteredDeviceValue(String response, BaseDevice &baseDevice)
{
  response.toCharArray(baseDevice.id, IdStringLength, 1);
  DebugMessage("Base device id set to: " + String(baseDevice.id));
}

void RegisterBaseDevice(BaseDevice &baseDevice)
{
  String url = httpPrefix + GetServer(baseDevice) + inputDeviceSuffix;

  HTTPResponse response = HTTPPost(url, SerializeDevice(baseDevice, true));

  if (response.code == 400)
  {
    response = HTTPPost(url, SerializeDevice(baseDevice, false));
  }
  if (response.success)
  {
    DecodeRegisteredDeviceValue(response.payload, baseDevice);
  }
  else
  {
    DebugMessage("Was not able to initialize device!");
  }
}

#pragma endregion

#pragma region Sensor
void DecodeRegisteredSensorValue(String payload, Sensor &sensor)
{
  DebugMessage("Retrieved ID is: " + payload);
  payload.toCharArray(sensor.id, IdStringLength, 1);
  sensor.isInitialized = true;
  DebugMessage("Sensor has saved ID: " + String(sensor.id));
}

void LoadLastValue(Sensor &sensor, BaseDevice &baseDevice)
{
  DebugMessage("Loading sensor last value");
  String url = httpPrefix + GetServer(baseDevice) + lastSensorValue + sensor.id;

  HTTPResponse response = HTTPGet(url);

  if (response.code == 200)
  {
    StaticJsonDocument<64> filter;
    filter[type] = true;
    filter[DTvalueName] = true;
    filter[DTstringValue] = true;
    filter[DTnumericValue] = true;
    filter[DTdestinationId] = true;

    DynamicJsonDocument root(1024);
    auto error = deserializeJson(root, response.payload);

    if (error)
    {
      DebugMessage(response.payload);
      DebugMessage("deserializeJson() failed: ");
      return;
    }
    String stringValue = root[DTvalueName];
    float numericValue = root[DTnumericValue];

    sensor.SetStringValue(stringValue);
    sensor.SetNumericValue(numericValue);
  }
}

void RegisterSensor(Sensor &sensor, BaseDevice &baseDevice)
{
  DebugMessage("-------------Registering sensor-------------------");
  String url = httpPrefix + GetServer(baseDevice) + inputSensorSuffix;

  HTTPResponse response = HTTPPost(url, sensor.SerializeSensor(baseDevice.id));
  // repeat with no ID to ensure that invalid ID is not an issue
  if (response.code == 400)
  {
    DebugMessage("Got invalid parameter. Most probably ID, trying with empty id instead!");
    sensor.isInitialized = false;
    response = HTTPPost(url, sensor.SerializeSensor(baseDevice.id));
  }

  if (response.success)
  {
    DecodeRegisteredSensorValue(response.payload, sensor);

    LoadLastValue(sensor, baseDevice);
  }
  DebugMessage("---------------END Reg. SENSOR-------------------");
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
    filter.clear();
    root.clear();
    return;
  }

  String type = root[type];
  DebugMessage("Type is " + type);
  filter.clear();
  root.clear();
}

String RequestTime(BaseDevice baseDevice)
{
  String payload = "!";
  HTTPClient http;
  String url = httpPrefix + GetServer(baseDevice) + timeSuffix;
  // wifiClient.setTimeout(500);
  DebugMessage("Requesting time at: " + url);
  http.begin(/*wifiClient,*/ url); // Specify request destination

  int httpCode = http.GET(); // Send the request
  DebugMessage("Got response with decodingDevice: " + String(httpCode));
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
void DecodeRegisteredActuatorValue(String payload, Actuator &actuator)
{
  payload.toCharArray(actuator.id, IdStringLength, 1);
  actuator.isInitialized = true;
  DebugMessage("Actuator has saved ID: " + String(actuator.id));
}

void RegisterActuator(Actuator &actuator, BaseDevice &baseDevice)
{
  DebugMessage("-------------Registering actuator-------------------");

  String url = httpPrefix + GetServer(baseDevice) + inputActuatorSuffix;

  HTTPResponse response = HTTPPost(url, actuator.SerializeActuator(baseDevice.id));
  // repeat with no ID to ensure that invalid ID is not an issue
  if (response.code == 400)
  {
    DebugMessage("Got invalid parameter. Most probably ID, trying with empty id instead!");
    actuator.isInitialized = false;
    response = HTTPPost(url, actuator.SerializeActuator(baseDevice.id));
  }

  if (response.success)
  {
    DecodeRegisteredActuatorValue(response.payload, actuator);
  }
  DebugMessage("-------------End reg. actuator--------------------");
}
#pragma endregion
