#ifndef WebServer_h
#define WebServer_h

#include "Arduino.h"
#include <ESPAsyncWebServer.h>
#include "DebugMessageLib.h"
#include "MainFunctions.h"

void StartWebServer();
const String localIPURL = "http://192.168.4.1/";
String processor(const String &var);
AsyncWebServer* GetWebServer();

const char CSS[] PROGMEM = R"rawliteral(
<style>body{background-color:#000;color:#f0f8ff}h1{color:#f0f8ff}p{color:#f0f8ff}.material-icons{color:#0973b5}.onButton:hover{background-color:#002000;border-bottom-color:#7cfc00}.offButton{border-bottom-color:#8b0000!important}.onButton{border-bottom-color:#3cb371!important}.offButton:hover{background-color:#200000;border-bottom-color:red}.actuatorDiv,.sensorDiv{border:#0973b5;border-style:solid;border-width:1px;border-radius:20px;margin:10px;width:300px;min-height:200px;align-items:center;display:inline-block;justify-content:center}.sensorForm{width:max-content}.submit:hover,button:hover{background-color:#05234e;border-bottom:solid;border-bottom-color:#00f;border-bottom-width:3px}.offButton,.onButton,.submit,button,input{color:#f0f8ff;text-align:center;background-color:transparent;border-left:none;border-right:none;border-top:none;border-bottom:solid;min-width:40px;border-bottom-color:#0973b5;border-bottom-width:3px}.submit,button{height:30px;width:130px}.settings{border:#0973b5;border-style:solid;border-width:1px;border-radius:20px;background-color:#272727;padding:25px}.hiddenSetup{height:10px;border-top:solid 5px #0973b5;overflow:hidden}.hiddenSetup:hover{border-top:solid 1px #0973b5;overflow:visible}</style>
)rawliteral";

const char INDEX_HTML[] PROGMEM = R"rawliteral(
<!DOCTYPE html><html><head><title>Conectify</title><link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons"></head>%css%<script>let toggle = button => {
        let element = document.getElementById("settings");
        let hidden = element.getAttribute("hidden");
        if (hidden) {
            element.removeAttribute("hidden");
            button.innerText = "Hide settings";
        } else {
            element.setAttribute("hidden", "hidden");
            button.innerText = "Show settings";
        }
    }</script><body><h1>%deviceName%</h1>%wifi%<br><div>%sensors%</div><br><button onclick="toggle(this)">Show settings</button><div id="settings" class="settings" hidden="hidden"><form action="wifi" method="POST"><label>Wifi SSID</label><input name="wifiname" value="%SSID%"><br><label>Wifi Password</label><input type="password" name="wifipassword" value="%password%"><br><input type="submit" class="submit" value="Setup wifi"></form><br><br><form action="/device" method="POST"><label>Name</label><input name="setname" value="%deviceName%"></input><br/><br/><label>Device id</label><input name="setid" value="%deviceId%"><br><label>Server URL</label><input name="setserveradress" value="%serveradress%"><br><label for="setserverport">Server port</label><input type="number" name="setserverport" value="%setserverport%"><br><label>Refresh wifi every (s)</label><input type="number" name="wifitimer" value="%wifitimer%"><br><label>Refresh sensors every (s)</label><input type="number" name="sensortimer" value="%sensortimer%"><br><input type="checkbox" name="setdebugmessage" %debugmessage% value="1"><label>Debug message</label><br><input class="submit" type="submit" value="Setup device"></form><form action="reboot.html" method="POST"><input class="submit" name="reboot" type="submit" value="Reboot"></form></div></body></html>
)rawliteral";

const char SENSOR_HTML[] PROGMEM = R"rawliteral(
<div class="sensorDiv"><h4>{{deviceName}}</h4><label>{{ValueName}}: {{Value}}{{Unit}}</label><br>{{actuators}}<br><div class="hiddenSetup"><form><br><label>Device id</label><input name="deviceId" value="{{deviceID}}"><br><label>Device name</label><input name="deviceName" value="{{deviceName}}"><input type="submit" value="Setup device"></form></div></div>
)rawliteral";
const char ACTUATOR_HTML[] PROGMEM = R"rawliteral(
<form class="actuatorForm" action="/actuatorSet" method="POST"><input type="hidden" name="actuatorId" value="{{actuatorId}}"> <input type="submit" name="stringValue" class="onButton" value="on"> <input type="submit" name="stringValue" class="offButton" value="off"></form>
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