#include "Arduino.h"
#include "DebugMessageLib.h"
#include "GlobalVariables.h"

bool IsDebugInitialized = false;
void InitializeDebug();

int DebugMessage(String message){
    InitializeDebug();
    if(GetGlobalVariables()->baseDevice.debugMessage)
        Serial.println(message);
    return 1;
}

void InitializeDebug(){
    if(!IsDebugInitialized){
        Serial.begin(115200);
        IsDebugInitialized = true;
        DebugMessage("Debug initialized");
    }
}
