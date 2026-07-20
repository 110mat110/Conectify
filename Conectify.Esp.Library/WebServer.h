#ifndef WebServer_h
#define WebServer_h

#include <Arduino.h>
#include <ESPAsyncWebServer.h>
#include "DebugMessageLib.h"
#include "MainFunctions.h"

void StartWebServer();
const String localIPURL = "http://192.168.4.1/";
String processor(const String &var);
AsyncWebServer* GetWebServer();

const char CSS[] PROGMEM = R"rawliteral(
<style>body{margin:0;background:#0a0a0a;color:#e8eaed;font:14px system-ui,Roboto,sans-serif;line-height:1.4}header{display:flex;align-items:center;justify-content:space-between;gap:10px;height:52px;padding:0 16px;background:#1a1a1a;border-bottom:1px solid #2a2a2a}h1{margin:0;font-size:1.05rem}h4{margin:0 0 8px;font-size:.95rem;font-weight:500}.badge{font-size:.72rem;font-weight:600;padding:4px 10px;border-radius:99px;border:1px solid #2a2a2a;color:#808080}.badge.on{color:#38b000;border-color:#38b000}.badge.off{color:#ffbe0b;border-color:#ffbe0b}.wrap{max-width:960px;margin:0 auto;padding:16px 16px 40px}.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(240px,1fr));gap:14px;margin:16px 0}.card{background:#121212;border:1px solid #2a2a2a;border-radius:14px;padding:16px;margin-top:14px}.grid .card{margin-top:0}.mut{color:#808080;font-size:.8rem}.chip{display:inline-block;background:rgba(58,134,255,.08);border:1px solid rgba(58,134,255,.3);border-radius:10px;padding:2px 8px;font-size:13px;font-weight:500;color:#4fc3f7;margin:4px 0}form{display:grid;gap:6px}label{font-size:.7rem;text-transform:uppercase;color:#808080;margin-top:6px}input:not([type=checkbox]){background:#1e1e1e;border:1px solid #2a2a2a;border-radius:6px;color:#e8eaed;padding:9px 11px;font:inherit}input[type=checkbox]{accent-color:#3a86ff;width:16px;height:16px;margin:0}.row{display:flex;align-items:center;gap:8px;margin-top:8px}.row label{margin:0;text-transform:none;font-size:.85rem}button{font:inherit;font-weight:500;font-size:13px;color:#e8eaed;background:#1a1a1a;border:none;border-bottom:3px solid #3a86ff;border-radius:6px;padding:9px 14px;margin-top:10px}button.on{border-color:#38b000}button.off,button.danger{border-color:#ff006e}button.warn{border-color:#ffbe0b}.actrow{display:flex;gap:8px;margin-top:10px}.actrow button{flex:1;margin-top:0}.btns{display:flex;flex-wrap:wrap;gap:8px}.btns button{margin-top:0}.card details{border-top:1px solid #2a2a2a;margin-top:10px;padding-top:8px}summary{color:#808080;font-size:.78rem}details.card{margin-top:14px;padding-top:16px}details.card summary{font-size:.95rem;font-weight:500;color:#e8eaed}#toast{position:fixed;left:0;right:0;bottom:20px;margin:0 auto;width:fit-content;background:#1a1a1a;border:1px solid #3a86ff;padding:10px 16px;border-radius:10px;opacity:0;transition:opacity .2s}#toast.show{opacity:1}</style>
)rawliteral";

const char INDEX_HTML[] PROGMEM = R"rawliteral(
<!DOCTYPE html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>%deviceName%</title>%css%</head><body><header><h1>%deviceName%</h1>%wifi%</header><div class="wrap"><div class="grid">%sensors%</div><details class="card"><summary>Wi-Fi settings</summary><form action="/wifi" method="POST"><label>Wi-Fi SSID</label><input name="wifiname" value="%SSID%"><label>Wi-Fi password</label><input type="password" name="wifipassword" value="%password%"><button>Save Wi-Fi</button></form></details><details class="card"><summary>Device settings</summary><form action="/device" method="POST"><label>Name</label><input name="setname" value="%deviceName%"><label>Device ID</label><input name="setid" value="%deviceId%"><label>Server URL</label><input name="setserveradress" value="%serveradress%"><label>Server port</label><input type="number" name="setserverport" value="%setserverport%"><label>Refresh Wi-Fi every (s)</label><input type="number" name="wifitimer" value="%wifitimer%"><label>Refresh sensors every (s)</label><input type="number" name="sensortimer" value="%sensortimer%"><div class="row"><input type="checkbox" id="dbg" name="setdebugmessage" value="1" %debugmessage%><label for="dbg">Debug messages</label></div><button>Save device</button></form></details><div class="card"><h4>Actions</h4><div class="btns"><button onclick="ping()">Activity check</button><button class="warn" onclick="if(confirm('Reboot the device?'))act('/reboot.html','Rebooting device...')">Reboot</button><button class="danger" onclick="if(confirm('Erase ALL stored settings and reboot? This cannot be undone.'))act('/cleareeprom','Clearing EEPROM, device will reboot...')">Clear EEPROM</button></div></div></div><div id="toast"></div><script>function toast(m){var t=document.getElementById('toast');t.textContent=m;t.classList.add('show');clearTimeout(t._h);t._h=setTimeout(function(){t.classList.remove('show')},3000)}
function act(u,m){fetch(u,{method:'POST'}).catch(function(){});toast(m)}
function ping(){toast('Checking...');fetch('/activitycheck',{method:'POST'}).then(function(r){return r.text()}).then(function(x){toast('Device is '+x)}).catch(function(){toast('No response from device')})}</script></body></html>
)rawliteral";

const char SENSOR_HTML[] PROGMEM = R"rawliteral(
<div class="card"><h4>{{deviceName}}</h4><div class="mut">{{ValueName}}</div><span class="chip">{{Value}} {{Unit}}</span>{{actuators}}<details><summary>Sensor setup</summary><form action="/sensorId" method="POST"><input type="hidden" name="idx" value="{{idx}}"><label>Sensor ID</label><input name="sensorId" value="{{deviceID}}"><button>Save</button></form></details></div>
)rawliteral";
const char ACTUATOR_HTML[] PROGMEM = R"rawliteral(
<form class="actrow" action="/actuatorSet" method="POST"><input type="hidden" name="idx" value="{{aidx}}"><button class="on" name="stringValue" value="on">ON</button><button class="off" name="stringValue" value="off">OFF</button></form><details><summary>Actuator setup</summary><form action="/actuatorId" method="POST"><input type="hidden" name="idx" value="{{aidx}}"><label>Actuator ID</label><input name="actuatorId" value="{{actuatorId}}"><button>Save</button></form></details>
)rawliteral";

const char WELCOME_PAGE_HTML[] PROGMEM = R"rawliteral(
    <!DOCTYPE html><html><head><title>Conectify</title><link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons"></head>%css%<body><h1>Welcome to conectify</h1><div class="ConnectToWifi" hidden="%connectToWifiHidden%"><p>First things first, you need to connect to wifi</p><form action="/wifi" method="POST"><label>Wifi SSID</label><input name="wifiname"><br><label>Wifi Password</label><input type="password" name="wifipassword"><br><input type="submit" class="submit" value="Setup wifi"></form></div><div class="WifiConnected" hidden="%connectToWifiShown%"><p>Wifi is already connected, well done</p></div><div class="setup"><p>Now, we will setup your conectify device. We will need some details about device and conectify server</p><form action="/setup" method="GET"><label>Device name</label><input name="setname" value=""><br><label>Server URL</label><input name="setserveradress" value=""><br><label for="setserverport">Server port</label><input type="number" name="setserverport" value="5000"><br><label>Refresh wifi every (s)</label><input type="number" name="wifitimer" value="120"><br><label>Refresh sensors every (s)</label><input type="number" name="sensortimer" value="60"><br><p>This is not mandatory. If you do not know ID you do not need to set them</p><label>Device id</label><input name="setid" value=""><br><input type="checkbox" name="setDebugMessage" %debugmessage% value="1"><label>Debug message</label><br><input class="submit" type="submit" value="Setup device"></form><form action="reboot" method="POST"></div><br><p>While setup is not done, you can see current values and use actuators</p><div>%actuators% %sensors%</div></body></html>
    )rawliteral";
#endif

class CaptiveRequestHandler : public AsyncWebHandler
{
public:
    CaptiveRequestHandler() {
    DebugMessage("CaptiveRequestHandler 1");
	GetWebServer()->on("/connecttest.txt", [](AsyncWebServerRequest *request) { request->redirect("http://logout.net"); });	// windows 11 captive portal workaround
    DebugMessage("CaptiveRequestHandler 1");
	GetWebServer()->on("/wpad.dat", [](AsyncWebServerRequest *request) { request->send(404); });								// Honestly don't understand what this is but a 404 stops win 10 keep calling this repeatedly and panicking the esp32 :)

	// Background responses: Probably not all are Required, but some are. Others might speed things up?
	// A Tier (commonly used by modern systems)
	GetWebServer()->on("/generate_204", [](AsyncWebServerRequest *request) { request->redirect(localIPURL); });		   // android captive portal redirect
	GetWebServer()->on("/redirect", [](AsyncWebServerRequest *request) { request->redirect(localIPURL); });			   // microsoft redirect
	GetWebServer()->on("/hotspot-detect.html", [](AsyncWebServerRequest *request) { request->redirect(localIPURL); });  // apple call home
	GetWebServer()->on("/canonical.html", [](AsyncWebServerRequest *request) { request->redirect(localIPURL); });	   // firefox captive portal call home
	GetWebServer()->on("/success.txt", [](AsyncWebServerRequest *request) { request->send(200); });					   // firefox captive portal call home
	GetWebServer()->on("/ncsi.txt", [](AsyncWebServerRequest *request) { request->redirect(localIPURL); });			   // windows call home

	GetWebServer()->onNotFound([](AsyncWebServerRequest *request) {
		request->redirect(localIPURL);
		Serial.print("onnotfound ");
		Serial.print(request->host());	// This gives some insight into whatever was being requested on the serial monitor
		Serial.print(" ");
		Serial.print(request->url());
		Serial.print(" sent redirect to " + localIPURL + "\n");
	});
    }
    virtual ~CaptiveRequestHandler() {}

    bool canHandle(AsyncWebServerRequest *request)
    {
        //request->addInterestingHeader("ANY");
        DebugMessage("I can handle captive request!");
        return true;
    }

    void handleRequest(AsyncWebServerRequest *request)
    {
        DebugMessage("Handling captive request!");
        //request->redirect(localIPURL);
        request->send_P(200, "text/html", INDEX_HTML, processor);
    }
};