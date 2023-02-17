#include "WebServer.h"
#include "DebugMessageLib.h"
#include "GlobalVariables.h"

String GetIndexPage(){
    String result = ""; 
    result.concat("<!DOCTYPE html><html><head><title>Conectify</title><link rel=\"stylesheet\" href=\"https://fonts.googleapis.com/icon?family=Material+Icons\"></head>");
result.concat("<script> let toggle = button => { let element = document.getElementById(\"settings\"); let hidden = element.getAttribute(\"hidden\");");
        result.concat("if (hidden) {element.removeAttribute(\"hidden\"); button.innerText = \"Hide settings\";");
        result.concat("} else { element.setAttribute(\"hidden\", \"hidden\"); button.innerText = \"Show settings\"; }}");
result.concat("</script><body><h1>");
result.concat(GetGlobalVariables()->baseThing.id);
    result.concat("</h1><i class=\"material-icons\">wifi</i> <i class=\"material-icons\">cloud</i><br />");
    result.concat("<button onclick=\"toggle(this)\">Hide div</button>");
    result.concat("<div id=\"settings\" style=\"background-color:aliceblue;padding:25px;\">");
        result.concat("<form action=\"/wifi\"><label>Wifi SSID</label><input name=\"ssid\">");
        result.concat(GetGlobalVariables()->baseThing.ssid);
        result.concat("</input><br />");
        result.concat("<label>Wifi Password</label><input type=\"password\" name=\"password\"></input><br />");
        result.concat("<input type=\"submit\" value=\"Setup wifi\" /> </form><br /><br />");
        result.concat("<label>Device id </label><input></input><br />");
        result.concat("<label>Device name </label><input></input><br />");
        result.concat("<label>Server URL</label><input></input><br/>");
        result.concat("<input type=\"submit\" value=\"Setup server\"/>");
    result.concat("</div></body></html>");
return result;
}

    // <div>
    //     <form>
    //         <label>{ValueName}: {Value}{Unit}</label>
    //         <br />
    //         <br />
    //         <label>Device id </label><input></input>
    //         <br />
    //         <label>Device name </label><input></input>
    //         <input type="submit" value="Setup device" />
    //     </form>
    // </div>