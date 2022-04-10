#ifndef Thing_h
#define Thing_h

#include "Arduino.h"

struct Thing
{
    String thingPositionDescription;
    float longitude;
    float latitude;
    String thingName;
};
#endif