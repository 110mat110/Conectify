#ifndef Sensors_h
#define Sensors_h

#include "Arduino.h"
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
    String SerializeSensor(char thingId[37]);
    String SerializeValue(Time dateTime);
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
public:
    char id[IdStringLength];
    void SetActuatorValue(String stringValue,  String unit);
    void SetActuatorValue(float numericValue, String unit);
    Actuator(Sensor* sensor, String ActuatorName);
    Actuator();
    void RegisterFunctionForStringValue(
        void (*handleFunc)(String commandValue,String Unit)
        );
    void RegisterFunctionForNumericValue(
        void (*handleFunc)(float value, String Unit)
    );
    String SerializeActuator(char thingId[IdStringLength]);
    bool isInitialized;
};

#endif