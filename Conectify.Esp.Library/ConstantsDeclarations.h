#ifndef ConstantsDeclarations_h
#define ConstantsDeclarations_h

#include "Arduino.h"

const String type = "type";
const String IoTDeviceType = "Device";
const String SensorType = "Sensor";
const String CommandType = "Command";
const String ActionType = "Action";
const String ValueType = "Value";
const String ActuatorType = "Actuator";
const String CommandResponseType = "CommandResponse";
const String ActionResponseType = "ActionResponse";

const String ipAdress = "ipadress";
const String macAdress = "MacAdress";
const String position = "position";
const String description = "description";
const String deviceName = "name";
const String timeCreated = "timecreated";
const String Code = "code";

const String DTentity = "entity";
const String DTid = "id";
const String DTvalueName = "name";
const String DTvalueUnit = "unit";
const String DTstringValue = "stringValue";
const String DTnumericValue = "numericValue";
const String DTdestinationId = "destinationId";
const String DTSensorName = "name";
const String DTSourceDeviceId = "sourceDeviceId";
const String DTResponseSourceId = "responseSourceId";
const String DTSourceId = "sourceId";
const String DTSensorId = "sensorId";
const String DTActuatorId = "actuatorId";
const String DTDeviceId = "deviceId";

const String httpPrefix = "http://";
const String inputDeviceSuffix = "/api/device";
const String inputSensorSuffix = "/api/sensors";
const String inputActuatorSuffix = "/api/actuators";
const String timeSuffix = "/api/system/time";
const String inputWebSocketSuffix = "/api/websocket/";

const String HeaderContentType = "Content-Type";
const String HeaderJsonContentType = "application/json";

const String CommandWifiName = "wifiname";
const String CommandWifiPassword = "wifipassword";
const String CommandSetAdress = "setserveradress";
const String CommandSetPort = "setserverport";
const String CommandWifiRefreshTimer = "wifitimer";
const String CommandSensorTimer = "sensortimer";
const String CommandReboot = "reboot";
const String CommandSetId = "setid";
const String CommandSetName = "setname";
const String CommandDebugMessage = "setdebugmessage";
const String CommandClearEEPRom = "cleareeprom";
const String CommandTriggerSensor = "triggersensor";
const String CommandReconectWifi = "reconnectwifi";
const String CommandSaveToEEPRom = "eepromsave";
const String CommandActivityCheck = "activitycheck";
const String CommandSaveDevice = "savedevice";

const String CommandResponseActive = "active";

const String HTMLPercentileSign = "&#37;";
const String HTMLDegreeSign = "&#176;";

const char USBCommEndChar = '#';
const int HttpOKCode = 200;
const int IdStringLength = 37;
const int WiFiLength = 15;
const int ServerAdressLength = 35;
const int PortLength = 6;
const int UserInputStringLength = 30;
#endif
