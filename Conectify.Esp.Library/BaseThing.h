#ifndef BaseThing_h
#define BaseThing_h

#include "Arduino.h"
#include "ConstantsDeclarations.h"

struct BaseThing
{
    char id[IdStringLength];
    char ssid[WiFiLength];
    char password[WiFiLength];
    char serverUrl[ServerAdressLength];
    char port[PortLength];
    bool debugMessage;
    int WiFiTimer;
    int SensorTimer;
};
#endif
