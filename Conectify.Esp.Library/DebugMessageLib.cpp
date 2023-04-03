#include "Arduino.h"
#include "DebugMessageLib.h"
#include "GlobalVariables.h"

int DebugMessage(String message){
    if(GetGlobalVariables()->baseDevice.debugMessage)
        Serial.println(message);
    return 1;
}
