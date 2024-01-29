/* Written by: Florin Dumitrescu
 *  for ascom_powerbox_and_ambientconditions built with
 *  Arduino NANO V3
 *  January 2024
 *  DashBoard Arduino code
 */
//INCLUDE LIBRARIES

#include <DHTStable.h>;
DHTStable DHT;



//DECLARE VARIABLES
int DC_JACK_STATE = 0;
int PWM1_STATE = 0;
int PWM2_STATE = 0;
float NTC1_VALUE = 0;
int PWM1_AUTO = 0;
float NTC2_VALUE = 0;
int PWM2_AUTO = 0;
float TEMP = 0.0;
int HUM_REL = 0;
float HUM_ABS = 0.0;
float DEWPOINT = 0.0;
float VOLT = 0.00;
float VOLT_TEMP = 0.00;
int PIN_VALUE_A = 0;
int PIN_VALUE_V = 0;
float CURRENT_SAMPLE_SUM = 0.0;
int AVERAGE_COUNT = 0;
float AMP_AVERAGE[150];
float AVGAMP;
float AMP = 0.00;
float PWR = 0.00;
long int time1=0;
long int time2=0;
float timetotal_ntc1=0;
float timetotal_ntc2=0;
float PWR_TOTAL=0.00;

const int DC_JACK = 4;
const int PWM1 = 5;
const int PWM2 = 6;
const int DHT22_VCC = 7;
const int DHT22_DATA = 8;
const int DHT22_GND = 9;
const int NTC_VCC = 12;
const int VM = A0;
const int AM = A1;
const int NTC1 = A2;
const int NTC2 = A3;
//ARDUINO INITIALIZATION
void setup()
{
    pinMode(DC_JACK, OUTPUT);
    pinMode(PWM1, OUTPUT);
    pinMode(PWM2, OUTPUT);
    pinMode(DHT22_DATA, INPUT);
    pinMode(NTC_VCC, OUTPUT);
    pinMode(VM, INPUT);
    pinMode(AM, INPUT);
    pinMode(NTC1, INPUT);
    pinMode(NTC2, INPUT);

    digitalWrite(DC_JACK, LOW);
    digitalWrite(PWM1, LOW);
    digitalWrite(PWM2, LOW);
    digitalWrite(NTC_VCC,LOW);
    Serial.begin(9600);
    Serial.flush();
}

//LOOP TO READ SERIAL COMMANDS
void loop()
{
  int indexofpwm1=0;
  int indexofpwm2=0;
  String valueofpwm1="";
  String valueofpwm2="";
  if (PWM1_AUTO == 1){ SET_AUTO_PWM1(); }
  if (PWM2_AUTO == 1){ SET_AUTO_PWM2(); }
  GET_POWER();
    String cmd;

    if (Serial.available() > 0) {
        cmd = Serial.readStringUntil('#');
        if (cmd == "GETSTATUSDCJACK") 
        {
            Serial.print(DC_JACK_STATE);
            Serial.println("#");
        }
        else if (cmd == "SETSTATUSDCJACK_OFF") SET_DC_JACK(0);
        else if (cmd == "SETSTATUSDCJACK_ON") SET_DC_JACK(1);
        else if (cmd == "GETSTATUSPWM1") 
        {
            Serial.print(PWM1_STATE);
            Serial.println("#");
        }
        else if (cmd == "GETAUTOPWM1")
        {
          Serial.print(PWM1_AUTO);
          Serial.println("#");
        }
        else if (cmd == "SETAUTOPWM1_ON") {
          PWM1_AUTO = 1;
          Serial.print(PWM1_AUTO);
          Serial.println("#");
        }
        else if (cmd == "SETAUTOPWM1_OFF") {
          PWM1_AUTO = 0;
          Serial.print(PWM1_AUTO);
          Serial.println("#");
        }
        else if (cmd == "GETNTC1TEMP"){
          GET_NTC1();
          Serial.print(NTC1_VALUE);
          Serial.println("#");
        }
        else if (cmd == "GETNTC2TEMP"){
          GET_NTC2();
          Serial.print(NTC2_VALUE);
          Serial.println("#");
        }
        else if (cmd.substring(0,13) == "SETSTATUSPWM1") PWM1_POWER(cmd.substring((cmd.indexOf('_')+1),(cmd.indexOf('_')+4)).toInt()); 
        else if (cmd == "GETSTATUSPWM2") 
        {
            Serial.print(PWM2_STATE);
            Serial.println("#");
        }
        else if (cmd == "GETAUTOPWM2")
        {
          Serial.print(PWM2_AUTO);
          Serial.println("#");
        }
        else if (cmd == "SETAUTOPWM2_ON") {
          PWM2_AUTO = 1;
          Serial.print(PWM2_AUTO);
          Serial.println("#");
        }
        else if (cmd == "SETAUTOPWM2_OFF"){
          PWM2_AUTO = 0;
          Serial.print(PWM2_AUTO);
          Serial.println("#");
        }
        else if (cmd.substring(0,13) == "SETSTATUSPWM2") PWM2_POWER(cmd.substring((cmd.indexOf('_')+1),(cmd.indexOf('_')+4)).toInt());
        else if (cmd == "GETTEMPERATURE")
        { 
           GET_AMBIENT();
           Serial.print(TEMP);
           Serial.println("#");
        }
        else if (cmd == "GETHUMIDITY")
        {
           Serial.print(HUM_REL);
           Serial.println("#");
        }
        else if (cmd == "GETDEWPOINT")
        {
           Serial.print(DEWPOINT);
           Serial.println("#");
        }
        else if (cmd == "GETVOLTAGE")
        {
           Serial.print(VOLT);
           Serial.println("#");
        }
        else if (cmd == "GETCURRENT")
        { 
           Serial.print(AMP);
           Serial.println("#");
        }
        else if (cmd == "GETPOWER") 
        {
           Serial.print(PWR);
           Serial.println("#");
        }
        else if (cmd == "GETUSAGE")
        {
           Serial.print(PWR_TOTAL);
           Serial.println("#");
        }
//        else if (cmd =="GETALL") // USE FOR DEBUGGING 
//        {
//          Serial.print("DC Jacks state: ");
//          Serial.print(DC_JACK_STATE);
//          Serial.print(", PWM1 level :");
//          Serial.print(PWM1_STATE);
//          Serial.print(", PMW1 Auto :");
//          Serial.print(PWM1_AUTO);
//          Serial.print(", PWM2 level :");
//          Serial.print(PWM2_STATE);
//          Serial.print(", PWM2 Auto :");
//          Serial.print(PWM2_AUTO);
//          Serial.print(", NTC1 Temperature: ");
//          Serial.print(NTC1_VALUE);
//          Serial.print(", NTC2 Temperature: ");
//          Serial.print(NTC2_VALUE);
//          Serial.println();
//          Serial.print(", Ambiental temperature: ");
//          Serial.print(TEMP);
//          Serial.print(", Relative humidity: ");
//          Serial.print(HUM_REL);
//          Serial.print(", Dew point: ");
//          Serial.print(DEWPOINT);
//          Serial.print(", Voltage: ");
//          Serial.println();
//          Serial.print(VOLT);
//          Serial.print(", Current: ");
//          Serial.print(AMP);
//          Serial.print(", Power: ");
//          Serial.print(PWR);
//          Serial.print(", Total power used: ");
//          Serial.print(PWR_TOTAL);
//          Serial.println();
//          
//        }
    }
   //    Serial.println(timetotal_ntc2);
     //  Serial.println(timetotal_ntc1);
    
//    Serial.print(PIN_VALUE_V);
//    Serial.print(" - ");
//    Serial.print(VOLT);
//    Serial.print(" - ");
//    Serial.print(CURRENT_AVG);
//    Serial.print(" - ");
//    Serial.print(CURRENT_MID);
//    Serial.print(" - ");
//    Serial.print(CURRENT_TOTAL);
//    Serial.print(" = ");
//  Serial.print(AMP);
//    Serial.println();
} 

// END READ SERIAL COMMANDS


//SET/GET FUNCTIONS


//SET DC JACK STATE
void SET_DC_JACK(int state){
     if (state==0) digitalWrite(DC_JACK, LOW);
     else if (state==1) digitalWrite(DC_JACK, HIGH);
     DC_JACK_STATE = state;
     Serial.print(DC_JACK_STATE);
     Serial.println("#");
}
//END SET DC JACK STATE


//SET PWM1 POWER
  //PWM1_POWER
void PWM1_POWER(int state) {
    int pwm_pin = (state*255)/100;
    analogWrite(PWM1, pwm_pin);
    PWM1_STATE = state;
    Serial.print(PWM1_STATE);
    Serial.println("#");
}
//END SET PWM1 POWER

// SET AUTO PWM1 POWER
void SET_AUTO_PWM1(){
   if (timetotal_ntc1 > 450){
    GET_NTC1();
    if (NTC1_VALUE - TEMP < 1 && PWM1_STATE < 100){
      PWM1_STATE = PWM1_STATE + 10;
      PWM1_STATE = CORRECT_PWM(PWM1_STATE);
      SET_PWM_VALUE("PWM1",PWM1_STATE);
    }
    if (NTC1_VALUE - TEMP > 4 && PWM1_STATE > 0){
      PWM1_STATE = PWM1_STATE - 10;
      PWM1_STATE = CORRECT_PWM(PWM1_STATE);
      SET_PWM_VALUE("PWM1",PWM1_STATE);
    }
    timetotal_ntc1 = 0;
   }
   timetotal_ntc1 += 1;
}
//END SET AUTO PWM1 POWER

//GET NTC1 TEMP
void GET_NTC1()
{
 digitalWrite(NTC_VCC,HIGH);
   int ntc_read_sum = 0;
   float avg_read = 0;
   for (int i=0;i<40;i++){
    ntc_read_sum +=analogRead(NTC1);
   }
   avg_read = ntc_read_sum/40;
   digitalWrite(NTC_VCC,LOW);
   avg_read = 1023 / avg_read-1;
   avg_read = 10000 / avg_read;
   float temp_ntc1 = avg_read / 10000;
   temp_ntc1=log(temp_ntc1);
   temp_ntc1 /= 3380;
   temp_ntc1 +=1.0/(25 + 273.15);
   temp_ntc1 = (1.0/temp_ntc1) - 273.15; 
   NTC1_VALUE=temp_ntc1;
}
// END GET NTC1 TEMP

// SET AUTO PWM2 POWER
void SET_AUTO_PWM2(){
   if (timetotal_ntc2 > 450){
    GET_NTC2();
    if (NTC2_VALUE - TEMP < 1 && PWM2_STATE<100){
      PWM2_STATE = PWM2_STATE + 10;
      PWM2_STATE = CORRECT_PWM(PWM2_STATE);
      SET_PWM_VALUE("PWM2",PWM2_STATE);
    }
    if (NTC2_VALUE - TEMP > 4 && PWM2_STATE>0){
      PWM2_STATE = PWM2_STATE - 10;
      PWM2_STATE = CORRECT_PWM(PWM2_STATE);
      SET_PWM_VALUE("PWM2",PWM2_STATE);
    }
    timetotal_ntc2 = 0;
   }
   timetotal_ntc2 += 1;
}
//END SET AUTO PWM1 POWER

//GET NTC2 TEMP
void GET_NTC2()
{
 digitalWrite(NTC_VCC,HIGH);
   int ntc_read_sum = 0;
   float avg_read = 0;
   for (int i=0;i<40;i++){
    ntc_read_sum +=analogRead(NTC2);
   }
   avg_read = ntc_read_sum/40;
   digitalWrite(NTC_VCC,LOW);
   avg_read = 1023 / avg_read-1;
   avg_read = 10000 / avg_read;
   float temp_ntc = avg_read / 10000;
   temp_ntc=log(temp_ntc);
   temp_ntc /= 3380;
   temp_ntc +=1.0/(25 + 273.15);
   temp_ntc = (1.0/temp_ntc) - 273.15; 
   NTC2_VALUE=temp_ntc;
}
// END GET NTC2 TEMP


//SET PWM2 POWER
  void PWM2_POWER(int state) {
    int pwm_pin=(state*255)/100;
    analogWrite(PWM2, pwm_pin);
    PWM2_STATE = state;
    Serial.print(PWM2_STATE);
    Serial.println("#");
  }
//END SET PWM2 POWER

//MEASURE AND CALCULATE POWER USAGE
  void GET_POWER() {
    time1=millis();
    if (AVERAGE_COUNT == 150)
    {
      AVERAGE_COUNT = 0;
    }
    PIN_VALUE_V = analogRead(VM);     
    VOLT_TEMP = (PIN_VALUE_V * 5.0) / 1024.0;   
    VOLT = VOLT_TEMP / 0.092;        
    if (VOLT < 0.1)   
      {     
        VOLT=0.0;    
      }
    CURRENT_SAMPLE_SUM=0;
    for (int i=0;i<150;i++)
    {
      PIN_VALUE_A= analogRead(AM);
      CURRENT_SAMPLE_SUM = CURRENT_SAMPLE_SUM + PIN_VALUE_A;
    }
    AMP_AVERAGE[AVERAGE_COUNT] = ((2.494 - ((CURRENT_SAMPLE_SUM/150)*(5.0/1024.0)))*-1) / 0.10;
    AVGAMP=0;
    for (int c=0;c<150;c++)
    {
    AVGAMP += AMP_AVERAGE[c];
    }
    AMP = AVGAMP/150;
    if (AMP< 0.01)
    {
      AMP=0.0;
    }
    
    PWR = VOLT * AMP;
    AVERAGE_COUNT = AVERAGE_COUNT + 1;
    time2=millis();
    PWR_TOTAL=PWR_TOTAL+(PWR*((time2-time1)/3600000)); // Calculate total power used each cycle, then add to power usage. since switch has been connected.
}
//END GET POWER USAGE

//GET AMBIENT CONDITIONS
  void GET_AMBIENT() {
  digitalWrite(DHT22_GND,LOW);
  int PIN_VALUE_T = DHT.read22(DHT22_DATA);
   HUM_REL = DHT.getHumidity();
   TEMP = DHT.getTemperature();
   DEWPOINT  = (TEMP - (100 - HUM_REL) / 5);
 }
 //CORRECT PWM VALUE
 void SET_PWM_VALUE(String pwmno, int value){
  value = CORRECT_PWM(value);
  int pwm_pin = (value*255)/100;
  if (pwmno = "PWM1") analogWrite(PWM1, pwm_pin);
  if (pwmno = "PWM2") analogWrite (PWM2, pwm_pin);
 }
 int CORRECT_PWM(int state){
 if (state > 100) return 100;
 if (state < 0) return 0;
 else return state;
}
 