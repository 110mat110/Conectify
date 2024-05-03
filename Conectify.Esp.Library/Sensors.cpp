#include "Arduino.h"
#include "Sensors.h"
#include "TickTimer.h"
#include "ArduinoJson.h"
#include "DebugMessageLib.h"
#include "ConstantsDeclarations.h"
#include "WebServer.h"
#include "GlobalVariables.h"

#define PMTXT(textarray) (reinterpret_cast<const __FlashStringHelper *>(textarray))

Sensor::Sensor(String sensorName, String valueName, String valueUnit)
{
    isInitialized = false;
    this->sensorName = sensorName;
    this->valueName = valueName;
    this->valueUnit = valueUnit;
}
Sensor::Sensor()
{
    isInitialized = false;
};
String Sensor::SerializeSensor(char deviceId[IdStringLength])
{
    DynamicJsonDocument doc(256);
    if (isInitialized)
        doc[DTid] = id;
    else
    {
        doc[DTid] = "00000000-0000-0000-0000-000000000000";
    }

    doc[type] = SensorType;
    doc[DTSensorName] = sensorName;
    doc[DTSourceDeviceId] = deviceId;
    String json;
    serializeJson(doc, json);
    doc.clear();
    return json;
}
String Sensor::SerializeValue(Time dateTime)
{
    DynamicJsonDocument doc(256);
    doc[type] = ValueType;
    doc[DTSourceId] = id;
    doc[timeCreated] = dateTime.ToJSONString();
    doc[DTvalueName] = valueName;
    doc[DTstringValue] = stringValue;
    doc[DTnumericValue] = numericValue;
    doc[DTvalueUnit] = valueUnit;
    String json;
    serializeJson(doc, json);
    doc.clear();
    return json;
}

String Sensor::ShowHtml()
{
    String sensor_html = String(PMTXT(SENSOR_HTML));
    sensor_html.replace("{{ValueName}}", valueName);
    sensor_html.replace("{{Value}}", String(numericValue));
    sensor_html.replace("{{Unit}}", valueUnit);
    sensor_html.replace("{{deviceID}}", id);
    sensor_html.replace("{{deviceName}}", sensorName);
    sensor_html.replace("%", HTMLPercentileSign);
    sensor_html.replace("°", HTMLDegreeSign);

    String result = "";
    DebugMessage("Replacing actuator literal");
    for (int i = 0; i < GetGlobalVariables()->actuatorArrSize; i++)
    {
        if (GetGlobalVariables()->actuatorsArr[i].IsSensorOfThis(id))
        {
            String currentActuatorHtml = GetGlobalVariables()->actuatorsArr[i].ShowHtml();
            sensor_html.replace("{{actuators}}", currentActuatorHtml);
        }
    }
    sensor_html.replace("{{actuators}}", "");
    return sensor_html;
}

void Sensor::MarkAsRead()
{
    SetChanged(false, "MarkAsRead");
}
void Sensor::SetSensorName(String val) { sensorName = val; }
void Sensor::SetValueName(String val) { valueName = val; }
void Sensor::SetValueUnit(String val) { valueUnit = val; }
String Sensor::GetName() { return sensorName; }
String Sensor::GetValueName() { return valueName; }
String Sensor::GetValueUnit() { return valueUnit; }
String Sensor::GetStringValue() { return stringValue; }
float Sensor::GetNumericValue() { return numericValue; }
void Sensor::SetStringValue(String val)
{
    SetChanged(changed || alwaysUpdateValue || val != stringValue, "setString");
    if (changed)
    {
        stringValue = val;
    }
        else
    {
        DebugMessage("Did not update string value. Old: " + String(stringValue) + " new " + String(val));
    }
}
void Sensor::SetNumericValue(float val)
{
    SetChanged(changed || alwaysUpdateValue || fabs(val - numericValue) > 0.05, "setInt");
    if (changed)
    {
        numericValue = val;
    }
    else
    {
        DebugMessage("Did not update numeric value. Old: " + String(numericValue) + " new " + String(val));
    }
}

bool Sensor::HasChanged()
{
    return changed;
}

void Sensor::SetChanged(bool val, String source)
{
    changed = val;
}

Actuator::Actuator(Sensor *sensor, String ActuatorName)
{
    this->sensor = sensor;
    isInitialized = false;
    actuatorName = ActuatorName;
}
Actuator::Actuator()
{
    isInitialized = false;
}
void Actuator::SetActuatorValue(String stringValue, String unit)
{
    handleStringFunc(stringValue, unit);
    changed = true;
    sensor->SetStringValue(stringValue);
}
void Actuator::SetActuatorValue(float numericValue, String unit)
{
    handleNumericFunc(numericValue, unit);
    changed = true;
    sensor->SetNumericValue(numericValue);
}

void Actuator::RegisterFunctionForStringValue(
    void (*handleFunc)(String commandValue, String Unit))
{
    handleStringFunc = handleFunc;
}

void Actuator::RegisterFunctionForNumericValue(
    void (*handleFunc)(float value, String Unit))
{
    handleNumericFunc = handleFunc;
}

bool Actuator::HasChanged()
{
    return changed;
}

void Actuator::MarkAsRead()
{
    changed = false;
}

bool Actuator::IsSensorOfThis(char id[IdStringLength])
{
    return !strcmp(id, sensor->id);
}

String Actuator::SerializeResponse(Time dateTime)
{
    DynamicJsonDocument doc(256);
    doc[type] = ActionResponseType;
    doc[DTSourceId] = id;
    doc[timeCreated] = dateTime.ToJSONString();
    doc[DTvalueName] = sensor->GetValueName();
    doc[DTstringValue] = sensor->GetStringValue();
    doc[DTnumericValue] = sensor->GetNumericValue();
    doc[DTvalueUnit] = sensor->GetValueUnit();
    doc[DTResponseSourceId] = lastActionId;
    String json;
    serializeJson(doc, json);
    doc.clear();
    return json;
};
String Actuator::ShowHtml()
{
    String actuator_html = String(PMTXT(ACTUATOR_HTML));
    actuator_html.replace("{{ValueName}}", sensor->GetValueName());
    actuator_html.replace("{{Value}}", String(sensor->GetNumericValue()));
    actuator_html.replace("{{Unit}}", sensor->GetValueUnit());
    actuator_html.replace("{{actuatorId}}", id);
    actuator_html.replace("{{deviceName}}", actuatorName);
    actuator_html.replace("%", HTMLPercentileSign);
    actuator_html.replace("°", HTMLDegreeSign);
    return actuator_html;
}

String Actuator::SerializeActuator(char deviceId[IdStringLength])
{
    DynamicJsonDocument doc(256);
    if (isInitialized)
        doc[DTid] = id;
    else
    {
        doc[DTid] = "00000000-0000-0000-0000-000000000000";
    }
    doc[type] = ActuatorType;
    doc[DTvalueName] = actuatorName;
    doc[DTSourceDeviceId] = deviceId;
    doc[DTSensorId] = sensor->id;
    String json;
    serializeJson(doc, json);
    doc.clear();
    return json;
}