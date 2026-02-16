#include <Arduino.h>
#include <Preferences.h>
#include "BaseDevice.h"
#include "DebugMessageLib.h"
#include "Sensors.h"
#include "GlobalVariables.h"

Preferences prefs;
const char* NAMESPACE = "device";  // namespace for all saved data

// ---------- BaseDevice ----------
void SaveBaseDevice(const BaseDevice &baseDevice) {
    prefs.begin(NAMESPACE, false);

    prefs.putBytes("baseDevice", &baseDevice, sizeof(BaseDevice));

    prefs.end();
}

void ReadBaseDevice(BaseDevice &device) {
    prefs.begin(NAMESPACE, true);

    size_t size = prefs.getBytesLength("baseDevice");
    if (size != sizeof(BaseDevice)) {
        DebugMessage("No BaseDevice saved or size mismatch!");
        GetGlobalVariables()->initialized = false;
        prefs.end();
        return;
    }

    prefs.getBytes("baseDevice", &device, sizeof(BaseDevice));

    DebugMessage("--------READING FROM Preferences-------");
    DebugMessage(device.id);
    DebugMessage(device.ssid);
    DebugMessage(device.password);
    DebugMessage(device.serverUrl);
    DebugMessage(String(device.SensorTimer));
    DebugMessage(String(device.WiFiTimer));
    DebugMessage(device.Name);
    DebugMessage("--------END Preferences READING------------");

    prefs.end();
    GetGlobalVariables()->initialized = true;
}


// ---------- Sensors ----------
void SaveSensorIDs(Sensor* sensorArray, byte sensorArraySize) {
    prefs.begin(NAMESPACE, false);

    prefs.putUInt("sensorCount", sensorArraySize);

    for (int i = 0; i < sensorArraySize; i++) {
        char key[12];
        snprintf(key, sizeof(key), "sensor%d", i);
        prefs.putString(key, sensorArray[i].id);
    }

    prefs.end();
}

void LoadSensors(Sensor* sensorArray) {
    prefs.begin(NAMESPACE, true);

    int size = prefs.getUInt("sensorCount", 0);
    if (size == 0) {
        DebugMessage("No sensors are saved!");
        prefs.end();
        return;
    }

    for (int i = 0; i < size; i++) {
        char key[12];
        snprintf(key, sizeof(key), "sensor%d", i);
        String id = prefs.getString(key, "");
        strncpy(sensorArray[i].id, id.c_str(), IdStringLength);
        sensorArray[i].isInitialized = true;

        DebugMessage("Sensor [" + String(i) + "] ID: " + String(sensorArray[i].id));
    }

    prefs.end();
}


// ---------- Actuators ----------
void SaveActuatorIDs(Actuator* actuatorArray, byte actuatorArraySize) {
    prefs.begin(NAMESPACE, false);

    prefs.putUInt("actuatorCount", actuatorArraySize);

    for (int i = 0; i < actuatorArraySize; i++) {
        char key[12];
        snprintf(key, sizeof(key), "actuator%d", i);
        prefs.putString(key, actuatorArray[i].id);
    }

    prefs.end();
}

void LoadActuators(Actuator* actuatorArray) {
    prefs.begin(NAMESPACE, true);

    int size = prefs.getUInt("actuatorCount", 0);
    if (size == 0) {
        DebugMessage("No actuators are saved!");
        prefs.end();
        return;
    }

    for (int i = 0; i < size; i++) {
        char key[12];
        snprintf(key, sizeof(key), "actuator%d", i);
        String id = prefs.getString(key, "");
        strncpy(actuatorArray[i].id, id.c_str(), IdStringLength);
        actuatorArray[i].isInitialized = true;

        DebugMessage("Actuator [" + String(i) + "] ID: " + String(actuatorArray[i].id));
    }

    prefs.end();
}


// ---------- Clear all saved data ----------
void ClearStorage() {
    prefs.begin(NAMESPACE, false);
    prefs.clear();  // clears entire namespace
    prefs.end();
}
