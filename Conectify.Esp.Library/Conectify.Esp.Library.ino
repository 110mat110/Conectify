// This is testing file for conectify library. May be used as demo or testing.
#include "TickTimer.h"
#include "Sensors.h"
#include "BaseDevice.h"
#include "DebugMessageLib.h"
#include "MainFunctions.h"
#include "GlobalVariables.h"
#include "DecodingFunctions.h"
#include <DHT.h>

#define pinDHT 5
#define typDHT11 DHT11     // DHT 11
#define NAME "Garduino"
#define NoSensors 4
#define NoActuators 0

DHT dht(pinDHT, typDHT11);

void UserSetupRoutines()
{
  dht.begin();
}

// Then declare all sensors and actuators
void DeclareSensors()
{
  GetGlobalVariables()->sensorsArr[0] = Sensor("DHT11", "Temperature", "°C");
  GetGlobalVariables()->sensorsArr[1] = Sensor("DHT11", "Humidity", "%rH");
  GetGlobalVariables()->sensorsArr[2] = Sensor("Soil", "Moisture", "%rH");
  GetGlobalVariables()->sensorsArr[3] = Sensor("Light", "Light", "%");
}

// This will perform each loop
void UserLoopRoutines()
{
  if (GetGlobalVariables()->SensoricTimer.IsTriggered())
  {
    int s1 = analogRead(A0);
    int s2 = analogRead(A1);
    int s3 = analogRead(A3);

    int s = (s1+s2+s3)/3;
    float soilHumidity = map(s, 0, 4095, 100, 0);

    int raw_light = analogRead(A4);
    float light = map(raw_light, 0, 4095, 0, 100);

    float temp = dht.readTemperature();
    float hum = dht.readHumidity();

    GetGlobalVariables()->sensorsArr[0].SetNumericValue(temp);
    GetGlobalVariables()->sensorsArr[1].SetNumericValue(hum);
    GetGlobalVariables()->sensorsArr[2].SetNumericValue(soilHumidity);
    GetGlobalVariables()->sensorsArr[3].SetNumericValue(light);
  }
}

#pragma region HiddenRoutines
void setup()
{
  StartupMandatoryRoutine(NoSensors, NoActuators, DeclareSensors);
  UserSetupRoutines();
  GetGlobalVariables()->SensoricTimer.SetForceTrigger();
}

void loop()
{
  LoopMandatoryRoutines();
  UserLoopRoutines();
}
#pragma endregion
