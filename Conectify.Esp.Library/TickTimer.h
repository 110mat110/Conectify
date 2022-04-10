#ifndef TickTimer_h
#define TickTimer_h

#include "Arduino.h"

class TickTimer
{
    public:
        TickTimer(int defaultIntervalInSeconds);
        void ResetTimer();
        bool IsTriggered();
        bool IsTriggeredNoReset();
        void SetForceTrigger();
        unsigned long SecondsSincePrevious();
        unsigned long MillisSincePrevious();
        unsigned long intervalMS; //interval is in miliseconds
    private :
        unsigned long previousTrigger;
        bool forceTrigger;
};

class Time {
    public:
	    unsigned long startTime;
        Time();
        void decodeTime(unsigned long time);
	    String ToJSONString();
        
};

#endif