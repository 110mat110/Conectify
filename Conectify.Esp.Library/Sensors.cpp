#include "Arduino.h"
#include "Sensors.h"
#include "TickTimer.h"
#include "ArduinoJson.h"
#include "DebugMessageLib.h"
#include "ConstantsDeclarations.h"

Sensor::Sensor(String sensorName, String valueName, String valueUnit)
{
    isInitialized = false;
    this->sensorName = sensorName;
    this->valueName = valueName;
    this->valueUnit = valueUnit;
    DebugMessage("Full constructor");
}
Sensor:: Sensor(){
    isInitialized = false;
    DebugMessage("Dummy contructor");
};
String Sensor::SerializeSensor(char thingId[IdStringLength]){
    DynamicJsonDocument doc(256);
    if(isInitialized)
        doc[DTid] = id;
    doc[type] = SensorType;
    doc[DTSensorName] = sensorName;
    doc[DTSourceThingId] = thingId;
    String json;
    serializeJson(doc, json);
    doc.clear();
    return json;
  }
String Sensor::SerializeValue(Time dateTime){
    DynamicJsonDocument doc(256);
    doc[type] = ValueType;
    doc[DTSourceId] = id;
    doc[timeCreated] = dateTime.ToJSONString();
    doc[DTvalueName] = valueName;
    doc[DTstringValue] = stringValue;
    doc[DTnumericValue] = numericValue;
    doc[DTvalueUnit]= valueUnit;
    String json;
    serializeJson(doc, json);
    doc.clear();
    return json;
  }

void Sensor::MarkAsRead(){
    SetChanged(false, "MarkAsRead");
}
    void Sensor::SetSensorName(String val){sensorName = val;}
    void Sensor::SetValueName(String val){valueName = val;}
    void Sensor::SetValueUnit(String val){valueUnit = val;}

    void Sensor::SetStringValue(String val){
        if(val!=stringValue)
            DebugMessage(val + " != " + stringValue);
        else
            DebugMessage(val + " = " + stringValue);
        
        DebugMessage("setString " + String(changed) + String(alwaysUpdateValue));

        SetChanged( changed || alwaysUpdateValue || val != stringValue, "SetString");
        stringValue = val;
    }
void Sensor::SetNumericValue(float val){

    DebugMessage("setint " + String(changed) + String(alwaysUpdateValue)+"||" + String(val) + ", "+ String(numericValue));

    SetChanged( changed || alwaysUpdateValue || fabs(val - numericValue) >0.05, "setInt");
    numericValue = val;
}

bool Sensor::HasChanged(){
    return changed;
}

void Sensor::SetChanged(bool val, String source){
    changed = val;
    DebugMessage("Changed set by " + source + " to: " + String(val));
}

Actuator::Actuator(Sensor* sensor, String ActuatorName){
    this -> sensor = sensor;
    isInitialized = false;
    actuatorName = ActuatorName;
}
Actuator::Actuator(){
    isInitialized = false;
    DebugMessage("Dummy ctuator created!");
}
void Actuator::SetActuatorValue(String stringValue,String unit){
    handleStringFunc(stringValue,unit);

    sensor->SetStringValue(stringValue);
}
void Actuator::SetActuatorValue(float numericValue, String unit){
    handleNumericFunc(numericValue, unit);

    sensor->SetNumericValue(numericValue);
}


void Actuator::RegisterFunctionForStringValue(
        void (*handleFunc)(String commandValue,String Unit)
    ){

handleStringFunc = handleFunc;
}

void Actuator::RegisterFunctionForNumericValue(
        void (*handleFunc)(float value, String Unit)
    ){
        handleNumericFunc = handleFunc;
    }

String Actuator::SerializeActuator(char thingId[37]){
    DynamicJsonDocument doc(256);
    if(isInitialized)
        doc[DTid] = id;
    doc[type] = ActuatorType;
    doc[DTvalueName] = actuatorName;
    doc[DTSourceThingId] = thingId;
    doc[DTSensorId] = sensor->id;
    String json;
    serializeJson(doc, json);
    doc.clear();
    return json;

  }