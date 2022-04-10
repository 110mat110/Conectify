#ifndef BaseThing_h
#define BaseThing_h

#include "Arduino.h"

struct BaseThing
{
    char id[37];
    char ssid[15];
    char password[15];
    char serverUrl[35];
    char port[6];
    bool debugMessage;
    int WiFiTimer;
    int SensorTimer;
};
#endif
