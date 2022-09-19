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
#include <ArduinoWebsockets.h>

using namespace websockets;

String GetServer(BaseThing &baseThing){
  return String(baseThing.serverUrl)+":"+String(baseThing.port);
}

#pragma region Thing

void DecodeRegisteredThingValue(String response, BaseThing &baseThing){
  response.toCharArray(baseThing.id, IdStringLength, 1);
  DebugMessage("Base device id set to :" + String(baseThing.id));
}

void RegisterBaseThing(BaseThing &baseThing, ESP8266WiFiClass WiFi, Thing thing){
  //DebugMessage("Registering base thing");
  HTTPClient http; 
  String url = httpPrefix + GetServer(baseThing)+inputThingSuffix;
  Serial.print(url);
  DebugMessage("Sending thing to: " + url);
  DebugMessage("Payload:"+ serializeThing(baseThing, WiFi, thing));
  http.begin(url);      //Specify request destination
  http.addHeader("Content-Type", "application/json");  //Specify content-type header

  int httpCode = http.POST(serializeThing(baseThing, WiFi, thing));   //Send the request
  DebugMessage("Got response with decodingThing:" + String(httpCode));
  if(httpCode == HttpOKCode){
    String payload = http.getString();                  //Get the response payload
    DebugMessage(payload);    //Print request response payload
    DecodeRegisteredThingValue(payload, baseThing);
  }
  http.end();
}

String serializeThing(BaseThing &baseThing, ESP8266WiFiClass WiFi, Thing thing){
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
void RegisterSensor(Sensor &sensor, BaseThing &baseThing){

  DebugMessage("-------------Registering sensor-------------------");
  DebugMessage(baseThing.id);
  HTTPClient http; 
  String url = httpPrefix + GetServer(baseThing)+ inputSensorSuffix;
  DebugMessage("Payload:"+ sensor.SerializeSensor(baseThing.id));
  http.begin(url);      //Specify request destination
  http.addHeader(HeaderContentType, HeaderJsonContentType);  //Specify content-type header

  int httpCode = http.POST(sensor.SerializeSensor(baseThing.id));   //Send the request
  DebugMessage("Got response:" + String(httpCode));
  if(httpCode == HttpOKCode){
    String payload = http.getString();                  //Get the response payload
    DebugMessage(payload);    //Print request response payload
    DecodeRegisteredSensorValue(payload, sensor);
  }
  http.end();
  DebugMessage("---------------END Reg. SENSOR-------------------");
}

/*
//TODO check wth is that?
void SendSensorValuesToServer(Sensor sensor, BaseThing baseThing, Time dateTime,     
    void (*handleFunc)(String commandText, float commandValue, String commandTextParam),
    Actuator* actuators,
    byte actuatorsLength
){

  if(!sensor.isInitialized)
  {
    DebugMessage("Sensor is not initialized!");
    return;
  }
  sensor.MarkAsRead();

  DebugMessage("--------- Sensor Value ------------------------");
  DebugMessage("Payload: " + sensor.SerializeValue(dateTime) );
  String url = httpPrefix + GetServer(baseThing)+ inputBareValueSuffix + returnAllValuesSuffix;
  DebugMessage("To adress: " + url);
  HTTPClient http; 

  http.begin(url);      //Specify request destination
  http.addHeader(HeaderContentType, HeaderJsonContentType);  //Specify content-type header

  int httpCode = http.POST(sensor.SerializeValue(dateTime));   //Send the request
  if(httpCode == HttpOKCode){
    String payload = http.getString();                  //Get the response payload
 
    DebugMessage("Response: " + payload);    //Print request response payload
    decodeIncomingJson(payload, handleFunc, dateTime, actuators, actuatorsLength);
  }
  http.end();  //Close connection
}
*/

void SendSensorValuesToServer(Sensor &sensor, Time &dateTime, WebsocketsClient websocketClient){
  DebugMessage("Here I shall send sensor value to WS");
}
void DecodeRegisteredSensorValue(String payload,Sensor &sensor){
  DebugMessage("Retrieved ID is: " + payload);
  payload.toCharArray(sensor.id, IdStringLength, 1);
  sensor.isInitialized = true;
  DebugMessage("Actuator has saved ID: " + String(sensor.id));
}

void RegisterActuator(Actuator &actuator, BaseThing &baseThing){
  DebugMessage("-------------Registering actuator-------------------");
  HTTPClient http; 
  String url = httpPrefix + GetServer(baseThing)+ inputActuatorSuffix;
  DebugMessage("Payload:"+ actuator.SerializeActuator(baseThing.id));

  http.begin(url);      //Specify request destination
  http.addHeader(HeaderContentType, HeaderJsonContentType);  //Specify content-type header

  int httpCode = http.POST(actuator.SerializeActuator(baseThing.id));   //Send the request
  DebugMessage("Got response:" + String(httpCode));
  if(httpCode == HttpOKCode){
    String payload = http.getString();                  //Get the response payload
    DebugMessage(payload);    //Print request response payload
    DecodeRegisteredActuatorValue(payload, actuator);
  }
  http.end();
  DebugMessage("-------------End reg. actuator--------------------");
}

void DecodeRegisteredActuatorValue(String payload,Actuator &actuator){
  DebugMessage("Retrieved ID is: " + payload);
  payload.toCharArray(actuator.id, IdStringLength, 1);
  actuator.isInitialized = true;
  DebugMessage("Actuator has saved ID: " + String(actuator.id));
}
#pragma endregion

#pragma region IncomingValue
void decodeIncomingJson(String incomingJson,   
    void (*handleFunc)(String commandText, float commandValue, String commandTextParam),
    Time dateTime,
    Actuator* actuators,
    byte actuatorsLength
) {
  StaticJsonDocument<64> filter;
  filter[type] = true;
  filter[DTvalueName] = true;
  filter[DTstringValue] = true;
  filter[DTnumericValue] = true;
  filter[DTdestinationId] = true;

  DynamicJsonDocument root(1024);
  auto error = deserializeJson(root, incomingJson);

	if(error) {
    DebugMessage(incomingJson);
		DebugMessage("deserializeJson() failed: ");
		return;
	}
   DebugMessage("decoding object");
  if (root[type] == CommandType) {
		String commandText = root[DTvalueName];
		float commandValue = root[DTnumericValue];
		String commandTextparameter = root[DTstringValue];

		DebugMessage("got command: " + commandText + " with num value: " + commandValue + " and text value: " + commandTextparameter);

		(*handleFunc)(commandText, commandValue, commandTextparameter);
  filter.clear();
  root.clear();
		return;
	}

	if (root[type] == ActionType) {
    DebugMessage("recognized acton");
    

    if(!root[DTdestinationId].isNull()){
      
      String strId = root[DTdestinationId].as<String>();
      DebugMessage("Message for: "+ strId);
      if(!strId.isEmpty()){
        char id[IdStringLength];
        strId.toCharArray(id, IdStringLength, 0);

        for(int i=0; i<actuatorsLength; i++){
          DebugMessage("Targer ID" + String(actuators[i].id));
          if(actuators[i].isInitialized && !strcmp(id, actuators[i].id)){
          DebugMessage("Found target!");
            if(!root[DTstringValue].isNull())
              actuators[i].SetActuatorValue(root[DTstringValue].as<String>(), root[DTvalueUnit].as<String>());
            if(!root[DTnumericValue].isNull())
              actuators[i].SetActuatorValue(root[DTnumericValue].as<float>(), root[DTvalueUnit].as<String>());
          }     
        }
      }
    }
    else{
      DebugMessage("got empty destination");
    }
		String time = root[timeCreated];
		DebugMessage(time);
   //TODO update system time
  filter.clear();
  root.clear();
		return;
	}


	String type = root[type];
	DebugMessage("Type is " + type);
  filter.clear();
  root.clear();
}


String RequestTime(BaseThing baseThing){
  String payload = "!";
  HTTPClient http; 
  String url = httpPrefix + GetServer(baseThing)+timeSuffix;

  DebugMessage("Requesting time at: " + url);
  http.begin(url);      //Specify request destination

  int httpCode = http.GET();   //Send the request
  DebugMessage("Got response with decodingThing:" + String(httpCode));
  if(httpCode == HttpOKCode){
    payload = http.getString();                  //Get the response payload
    DebugMessage(payload);    //Print request response payload
  }
  http.end();
  return payload;
  }
#pragma endregion

#pragma region Actuator

void GetValues(  
    BaseThing thing,
    void (*handleFunc)(String commandText, float commandValue, String commandTextParam),
    Time dateTime,
    Actuator* actuators,
    byte actuatorsLength
  ){

  String url = httpPrefix + String(thing.serverUrl)+ outputThingSuffix + String(thing.id);
  DebugMessage("To adress: " + url);
  HTTPClient http; 

 http.begin(url);      //Specify request destination
 
  int httpCode = http.GET();   //Send the request
  DebugMessage("Response:" + String(httpCode));
  if(httpCode == HttpOKCode){
    String payload = http.getString();                  //Get the response payload
 
    DebugMessage("Response: " + payload);    //Print request response payload
    decodeIncomingJson(payload, handleFunc, dateTime, actuators, actuatorsLength);
  }
  http.end();  //Close connection 
}


#pragma endregion

#pragma region CommandResponse
void SendCommandResponseToServer(String commandId, BaseThing baseThing, Time dateTime     
    //void (*handleFunc)(String commandText, float commandValue, String commandTextParam),
    //Actuator* actuators,
    //byte actuatorsLength
){

  DebugMessage("--------- Command response ------------------------");
  //DebugMessage("Payload: " + CreateCommandResponse(baseThing, dateTime) );
  String url = httpPrefix + GetServer(baseThing)+ inputBareValueSuffix;
  DebugMessage("To adress: " + url);
  HTTPClient http; 

  http.begin(url);      //Specify request destination
  http.addHeader(HeaderContentType, HeaderJsonContentType);  //Specify content-type header
  /*
  int httpCode = http.POST(CreateCommandResponse(baseThing, dateTime));   //Send the request

  if(httpCode == HttpOKCode){
    String payload = http.getString();                  //Get the response payload
 
    DebugMessage("Response: " + payload);    //Print request response payload
    decodeIncomingJson(payload, handleFunc, dateTime, actuators, actuatorsLength);
  }*/
  http.end();  //Close connection
}

String CreateCommandResponse(BaseThing thing, Time dateTime){
    DynamicJsonDocument doc(256);
    doc[type] = CommandResponseType;
    doc[DTSourceId] = thing.id;
    doc[timeCreated] = dateTime.ToJSONString();
    doc[DTvalueName] = "";
    doc[DTstringValue] = "";
    doc[DTnumericValue] = 0;
    doc[DTvalueUnit]= "";
    String json;
    serializeJson(doc, json);
    doc.clear();
    return json;
}

#pragma endregion
