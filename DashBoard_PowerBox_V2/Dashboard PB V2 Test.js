//*** CHECK THIS ProgID ***
var X = new ActiveXObject("Dashboard PB V2.Switch");
WScript.Echo("This is " + X.Name + ")");
// You may want to uncomment this...
// X.Connected = true;
X.SetupDialog();
