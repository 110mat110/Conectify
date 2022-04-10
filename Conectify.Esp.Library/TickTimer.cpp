#include "Arduino.h"
#include "TickTimer.h"

TickTimer::TickTimer(int defaultIntervalInSeconds){
    previousTrigger =0;
    intervalMS = defaultIntervalInSeconds*1000;
	forceTrigger = false;
  }
  unsigned long TickTimer::MillisSincePrevious(){
    return millis() - previousTrigger;
  }
  unsigned long TickTimer::SecondsSincePrevious(){
    return MillisSincePrevious()/1000;
  }
  bool TickTimer::IsTriggered(){
    bool res = (MillisSincePrevious() >= intervalMS || forceTrigger);
	if(res)
		ResetTimer();
	return res;
  }

bool TickTimer::IsTriggeredNoReset(){
	return (MillisSincePrevious() >= intervalMS || forceTrigger);
}
  void TickTimer::ResetTimer(){
    previousTrigger = millis();
	forceTrigger = false;
  }

  void TickTimer::SetForceTrigger(){
	  forceTrigger = true;
  }

String Time::ToJSONString(){
	  unsigned long time = startTime + millis();
		return String(time);	
}
    Time::Time(){
		startTime = 0;
    }

void Time::decodeTime(unsigned long time){
	startTime = time - millis();
}