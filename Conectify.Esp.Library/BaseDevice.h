#ifndef BaseDevice_h
#define BaseDevice_h

#include "Arduino.h"
#include "ConstantsDeclarations.h"

struct BaseDevice
{
    char id[IdStringLength];
    char ssid[WiFiLength];
    char password[WiFiLength];
    char serverUrl[ServerAdressLength];
    char port[PortLength];
    bool debugMessage;
    int WiFiTimer;
    int SensorTimer;
    char Name[UserInputStringLength];
};
#endif
