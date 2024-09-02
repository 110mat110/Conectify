#ifndef Value_h
#define Value_h

#include "Arduino.h"
#include "ConstantsDeclarations.h"

struct Value
{
    char SourceId[IdStringLength];
    float NumericValue;
    bool hasChanged;
    String stringValue;
    String unit;
};

#endif