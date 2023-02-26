#ifndef WebServer_h
#define WebServer_h

#include "Arduino.h"

const char INDEX_HTML[] PROGMEM = R"rawliteral(
<!DOCTYPE html><html><head><title>Conectify</title><link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons"></head><style>body{background-color:#000;color:#f0f8ff}h1{color:#f0f8ff}p{color:#f0f8ff}.material-icons{color:#0973b5}.onButton:hover{background-color:#002000;border-bottom-color:#7cfc00}.offButton{border-bottom-color:#8b0000!important}.onButton{border-bottom-color:#3cb371!important}.offButton:hover{background-color:#200000;border-bottom-color:red}.actuatorDiv,.sensorDiv{border:#0973b5;border-style:solid;border-width:1px;border-radius:20px;margin:10px;width:300px;min-height:200px;align-items:center;display:inline-block;justify-content:center}.sensorForm{width:max-content}.submit:hover,button:hover{background-color:#05234e;border-bottom:solid;border-bottom-color:#00f;border-bottom-width:3px}.offButton,.onButton,.submit,button,input{color:#f0f8ff;text-align:center;background-color:transparent;border-left:none;border-right:none;border-top:none;border-bottom:solid;min-width:40px;border-bottom-color:#0973b5;border-bottom-width:3px}.submit,button{height:30px;width:130px}.settings{border:#0973b5;border-style:solid;border-width:1px;border-radius:20px;background-color:#272727;padding:25px}.hiddenSetup{height:10px;border-top:solid 5px #0973b5;overflow:hidden}.hiddenSetup:hover{border-top:solid 1px #0973b5;overflow:visible}</style><script>let toggle = button => {
        let element = document.getElementById("settings");
        let hidden = element.getAttribute("hidden");
        if (hidden) {
            element.removeAttribute("hidden");
            button.innerText = "Hide settings"
        } else {
            element.setAttribute("hidden", "hidden");
            button.innerText = "Show settings";
        }
    }</script><body><h1>%thingName%</h1>%wifi%<br><div>%sensors%</div><br><button onclick="toggle(this)">Show settings</button><div id="settings" class="settings" hidden="hidden"><form action="wifi" method="POST"><label>Wifi SSID</label><input name="wifiname" value="%SSID%"><br><label>Wifi Password</label><input type="password" name="wifipassword" value="%password%"><br><input type="submit" class="submit" value="Setup wifi"></form><br><br><form action="/device" method="POST"><label>Device id</label><input name="setid" value="%thingId%"><br><label>Server URL</label><input name="setserveradress" value="%serveradress%"><br><label for="setserverport">Server port</label><input type="number" name="setserverport" value="%setserverport%"><br><label>Refresh wifi every (s)</label><input type="number" name="wifitimer" value="%wifitimer%"><br><label>Refresh sensors every (s)</label><input type="number" name="sensortimer" value="%sensortimer%"><br><input type="checkbox" name="setdebugmessage" %debugmessage% value="1"><label>Debug message</label><br><input class="submit" type="submit" value="Setup device"></form><form action="reboot.html" method="POST"><input class="submit" name="reboot" type="submit" value="Reboot"></form></div></body></html>
)rawliteral";

const char SENSOR_HTML[] PROGMEM = R"rawliteral(
<div class="sensorDiv"><h4>{{deviceName}}</h4><label>{{ValueName}}: {{Value}}{{Unit}}</label><br>{{actuators}}<br><div class="hiddenSetup"><form><br><label>Device id</label><input name="deviceId" value="{{deviceID}}"><br><label>Device name</label><input name="deviceName" value="{{deviceName}}"><input type="submit" value="Setup device"></form></div></div>
)rawliteral";
const char ACTUATOR_HTML[] PROGMEM = R"rawliteral(
<form class="actuatorForm" action="/actuatorSet" method="POST"><input type="hidden" name="actuatorId" value="{{actuatorId}}"> <input type="submit" name="stringValue" class="onButton" value="on"> <input type="submit" name="stringValue" class="offButton" value="off"></form>
)rawliteral";

#endif