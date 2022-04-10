#include "Arduino.h"
#include "EEPROM.h"
#include "BaseThing.h"
#include "DebugMessageLib.h"
#include "EEPRomHandler.h"
#include "Sensors.h"

void SaveToEEPRom(EEPROMClass eeprom, BaseThing baseThing){
    eeprom.begin(512);
    eeprom.put((sizeof(byte)*2),baseThing);
    eeprom.commit();
    eeprom.end();
}

void SaveSensorIDsToEEPROM(EEPROMClass eeprom, Sensor* sensorArray, byte sensorArraySize){
    eeprom.begin(512);
    int pos = (sizeof(byte) *2 )+sizeof(BaseThing);
    eeprom.put(0,sensorArraySize);

    for(int i=0; i<sensorArraySize; i++)
        eeprom.put(pos + i*37, sensorArray[i].id);

    eeprom.commit();
    eeprom.end();
}

void SaveActuatorIDsToEEPROM(EEPROMClass eeprom, Actuator* actuatorArray, byte actuatorArraySize){
    eeprom.begin(512);
    eeprom.put(sizeof(byte),actuatorArraySize);
    byte noOfSensors = 0;
    eeprom.get(0,noOfSensors);
    int pos = (sizeof(byte) *2 )+sizeof(BaseThing) + noOfSensors*37;

    for(int i=0; i<actuatorArraySize; i++)
        eeprom.put(pos + i*37, actuatorArray[i].id);

    eeprom.commit();
    eeprom.end();
}

void LoadSensorsFromEEPROM(EEPROMClass eeprom, Sensor* sensorArray){
    DebugMessage("Reading sensors from eeprom");

    eeprom.begin(512);
    byte sizeOfArray = 0;
    eeprom.get(0,sizeOfArray);
    DebugMessage("Size of array is:");
    DebugMessage(String(sizeOfArray));
    if(sizeOfArray == 0xFF) {
        DebugMessage("No sensors are saved!");   
        return;
    }
    int pos = (sizeof(byte)*2) + sizeof(BaseThing);
    for (int i = 0; i < sizeOfArray; i++)
    {

        eeprom.get(pos + i*37,sensorArray[i].id);
        DebugMessage("Got sensor ID:");
        DebugMessage(String(sensorArray[i].id));
        sensorArray[i].isInitialized = true;
    }
    eeprom.end();
}

void LoadActuatorFromEEPROM(EEPROMClass eeprom, Actuator* actuatorArray){
    DebugMessage("Reading actuators from eeprom");

    eeprom.begin(512);

    byte noOfSensors = 0;
    eeprom.get(0,noOfSensors);
    int pos = (sizeof(byte) *2 )+sizeof(BaseThing) + noOfSensors*37;

    byte sizeOfArray = 0;
    eeprom.get(sizeof(byte),sizeOfArray);
    if(sizeOfArray == 0xFF) {
        DebugMessage("No actuators are saved!");   
        return;
    }
    for (int i = 0; i < sizeOfArray; i++)
    {

        eeprom.get(pos + i*37,actuatorArray[i].id);
        DebugMessage("Got Actuator ID:");
        DebugMessage(String(actuatorArray[i].id));
        actuatorArray[i].isInitialized = true;
    }
    eeprom.end();
}

void ClearEEPROM(EEPROMClass eeprom){
    eeprom.begin(512);
    eeprom.write(0,(byte)0xFF);
    eeprom.write(sizeof(byte),(byte)0xFF);
    eeprom.commit();
    eeprom.end();
}

void ReadFromEEPRom(EEPROMClass eeprom, BaseThing &thing){
    DebugMessage("READING FROM EEPROM");
    eeprom.begin(512);
    eeprom.get((sizeof(byte)*2), thing);
    DebugMessage("SIZE:");
    DebugMessage(String(sizeof(BaseThing)));
    DebugMessage(thing.id);
    DebugMessage(thing.ssid);
    DebugMessage(thing.password);
    DebugMessage(thing.serverUrl);
    DebugMessage(String(thing.SensorTimer));
    DebugMessage(String(thing.WiFiTimer));
    DebugMessage("--------------------");
    eeprom.end();
}
