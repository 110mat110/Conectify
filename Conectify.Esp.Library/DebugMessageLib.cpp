#include "Arduino.h"
#include "DebugMessageLib.h"

int DebugMessage(String message){
    if(true)
        Serial.println(message);
        return 1;
}
