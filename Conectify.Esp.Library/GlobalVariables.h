#ifndef GlobalVariables_h
#define GlobalVariables_h

#include "Arduino.h"
#include "Sensors.h"
#include "BaseDevice.h"
#include "TickTimer.h"

class GlobalVariables{
public:
    Sensor *sensorsArr;
    Actuator *actuatorsArr;
    byte sensorsArrSize = 0;
    byte actuatorArrSize = 0;

    Time dateTime = Time();
    BaseDevice baseDevice;

    TickTimer TimeHandler = TickTimer(1);
    TickTimer WifiTimer = TickTimer(120);
    TickTimer SensoricTimer = TickTimer(1);

    bool initialized;
    int ledstate;

    bool WiFiRestartReq = false;
    bool EEPROMWrteReq = false;

    GlobalVariables(){};

    void SetLedON();
    void SetLedOFF();
    void InvertLed();
    void SetWiFiTimerInSeconds(int secondsPerTimer);
    void SetSensoricTimerInSeconds(int secondsPerTimer);
    bool RestartRequired();
    bool EEPROMWriteRequired();

    private:
    int GVNot(int state);
    void SetLed();
};

GlobalVariables* GetGlobalVariables();

#endif