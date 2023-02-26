#include "Arduino.h"
#include "DebugMessageLib.h"
#include "GlobalVariables.h"

int DebugMessage(String message){
    if(GetGlobalVariables()->baseThing.debugMessage)
        Serial.println(message);
    return 1;
}
