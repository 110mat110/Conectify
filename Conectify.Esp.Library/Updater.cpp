#include <Arduino.h>
#include "Updater.h"
#include "esp_system.h"
#include "esp_idf_version.h"

#if defined(ARDUINO_ARCH_ESP8266)
#include <ESP8266WiFi.h>
#elif defined(ESP32)
#include <WiFi.h>
#include <HTTPClient.h>
#else
#error Architecture unrecognized by this code.
#endif

#include <Update.h>
#include "DebugMessageLib.h"

// Constructor
Updater::Updater()
{
    lastCheck = 0;
}

// Destructor
Updater::~Updater()
{
}

void Updater::TryUpdate(const char *token)
{
    TryUpdate(token, "https://updaterapi.premiumasp.net/");
}

void Updater::TryUpdate(const char *token, const char *baseUrl)
{
    if (millis() < lastCheck)
    {
        lastCheck = 0;
    }

    if (millis() - lastCheck < 600000)
    {
        return;
    }

    lastCheck = millis();
    HTTPClient http;

    // --- Normalize base URL ---
    char normalizedBase[128];
    size_t len = strlen(baseUrl);
    if (len > 0 && baseUrl[len - 1] == '/')
    {
        snprintf(normalizedBase, sizeof(normalizedBase), "%s", baseUrl);
    }
    else
    {
        snprintf(normalizedBase, sizeof(normalizedBase), "%s/", baseUrl);
    }

    // --- Construct URLs ---
    char url[256];
    snprintf(url, sizeof(url), "%sdevice/download/%s/%012llx",
             normalizedBase, token, ESP.getEfuseMac());
    DebugMessage(url);

    http.begin(url);

    int httpCode = http.GET();
    if (httpCode == HTTP_CODE_OK)
    {
        DebugMessage("Got update. Loading");
        int contentLength = http.getSize();
        if (contentLength > 0)
        {
            bool canBegin = Update.begin(contentLength);
            if (canBegin)
            {
                DebugMessage("UPDATE: Writing update");

                WiFiClient *client = http.getStreamPtr();
                size_t written = Update.writeStream(*client);
                if (written == contentLength)
                {
                    DebugMessage("UPDATE: New firmware length is correct");
                    if (Update.end())
                    {
                        DebugMessage("UPDATE: Update ended");
                        if (Update.isFinished())
                        {
                            char udUrl[256];
                            snprintf(udUrl, sizeof(udUrl), "%sdevice/update-done/%s/%012llx",
                                     normalizedBase, token, ESP.getEfuseMac());
                            DebugMessage(udUrl);
                            http.begin(udUrl);
                            http.GET();
                            http.end();
                            DebugMessage("Rebooting");
                            ESP.restart();
                        } else{ DebugMessage("UPDATE: Update ended, but it is not finished");}
                    } else{ DebugMessage("UPDATE: Update did not ended");}
                } else{ DebugMessage("UPDATE: Written length != length of firmware");}
            } else {
                DebugMessage("UPDATE: Can begin is false");
            }
        }
    }

    http.end();
}

