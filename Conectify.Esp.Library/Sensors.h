#ifndef Sensors_h
#define Sensors_h

#include <Arduino.h>
#include "TickTimer.h"
#include "ConstantsDeclarations.h"

class  Sensor
{
private:
    String sensorName;
    String valueName;
    String valueUnit;
    String stringValue;
    float numericValue;
    bool changed = false;
    void SetChanged(bool val, String source);
public:
    bool alwaysUpdateValue = false; //if true it will set changed in each set value
    char id[IdStringLength];
    bool isInitialized;
    Sensor(String sensorName, String valueName, String valueUnit);
    Sensor();
    void SetSensorName(String val);
    void SetId(String val);
    void SetValueName(String val);
    void SetValueUnit(String val);
    void SetStringValue(String val);
    void SetNumericValue(float val);
    String GetName();
    String GetValueName();
    String GetValueUnit();
    String GetStringValue();
    float GetNumericValue();
    String SerializeSensor(char deviceId[IdStringLength]);
    String SerializeValue(Time dateTime);
    String ShowHtml();
    void MarkAsRead();
    bool HasChanged();
};

class  Actuator
{
private:
    String actuatorName;
    Sensor* sensor;
    void (*handleStringFunc)(String commandValue,String Unit);
    void (*handleNumericFunc)(float value, String Unit);
    bool changed = false;
public:
    String lastActionId;
    char id[IdStringLength];
    bool isInitialized;

    void SetActuatorValue(String stringValue,  String unit);
    void SetActuatorValue(float numericValue, String unit);

    String SerializeActuator(char deviceId[IdStringLength]);
    String SerializeResponse(Time dateTime);
    String ShowHtml();

    Actuator(Sensor* sensor, String ActuatorName);
    Actuator();

    bool HasChanged();
    bool IsSensorOfThis(char id[IdStringLength]);
    void MarkAsRead();
    void RegisterFunctionForStringValue(
        void (*handleFunc)(String commandValue,String Unit)
        );
    void RegisterFunctionForNumericValue(
        void (*handleFunc)(float value, String Unit)
    );


};

#endif