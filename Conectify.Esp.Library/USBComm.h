#ifndef USBComm_h
#define USBComm_h

#include <Arduino.h>

class  USBComm
{
private:
    String USBMessage = "";
    bool waitForFirstByte = true;
    char USBDecodeChar = 'a';
    void ProcessMessage();

    const char SetSSID = '0';
    const char SetPassword = '1';
    const char SetId = '2';
    const char SetServer = '3';
    const char SetWiFiTimer = '4';
    const char SetSensoricTimer = '5';
    const char SetDebuggingMessages = '6';
    const char ClearEEPROMcmd = '7';
    const char ForceTriggerSensor ='8';

    const char ReconectWifi = 'w';
    const char WriteToEEPRom = 'q';

public:
    bool recievedMessage = false; //USB recieved message
    void ProcessIncomingChar(char c);
    void Reset();
    bool ReadMessage();
};


#endif