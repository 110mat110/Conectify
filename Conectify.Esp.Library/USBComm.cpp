
#include "USBComm.h"
#include "DebugMessageLib.h"
#include "CommandHanlder.h"
#include "ConstantsDeclarations.h"
#include "GlobalVariables.h"

void USBComm::ProcessIncomingChar(char inChar){
  if(inChar == USBCommEndChar){
      ProcessMessage();
      GetGlobalVariables()->SetLedOFF();
  }else{
    if(waitForFirstByte){
        GetGlobalVariables()->SetLedON();
        USBDecodeChar = inChar;
        waitForFirstByte = false;
    } else {
      USBMessage += inChar;    
    }  
  }
}

void USBComm::Reset(){
    waitForFirstByte = true;
    recievedMessage = true;
    USBMessage = "";
}

bool USBComm::ReadMessage(){
    bool retValue = recievedMessage;
    recievedMessage = false;
    return retValue;
}

void USBComm::ProcessMessage(){
  DebugMessage("ProcessMessage");
  if(USBDecodeChar == SetSSID) HandleCommand(CommandWifiName, "",0,USBMessage);
  if(USBDecodeChar == SetPassword) HandleCommand(CommandWifiPassword, "",0, USBMessage);
  if(USBDecodeChar == SetId) HandleCommand(CommandSetId, "",0, USBMessage);
  if(USBDecodeChar == SetServer) HandleCommand(CommandSetAdress, "",0, USBMessage);
  if(USBDecodeChar == SetWiFiTimer) HandleCommand(CommandWifiRefreshTimer, "",USBMessage.toInt(), "");
  if(USBDecodeChar == SetSensoricTimer) HandleCommand(CommandSensorTimer, "",USBMessage.toInt(),"");
  if(USBDecodeChar == SetDebuggingMessages) HandleCommand(CommandDebugMessage, "",USBMessage.toInt(), "");
  if(USBDecodeChar == ClearEEPROMcmd) HandleCommand(CommandClearEEPRom, "",0,"");
  if(USBDecodeChar == ForceTriggerSensor) HandleCommand(CommandTriggerSensor, "", 0, "");
  if(USBDecodeChar == ReconectWifi) HandleCommand(CommandReconectWifi, "", 0, "");
  if(USBDecodeChar == WriteToEEPRom) HandleCommand(CommandSaveToEEPRom, "", 0, "");

  Reset();
}
