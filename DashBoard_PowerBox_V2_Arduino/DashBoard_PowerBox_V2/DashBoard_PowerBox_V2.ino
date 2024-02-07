/* Written by: Florin Dumitrescu
 *  for ascom_powerbox_and_ambientconditions built with
 *  Arduino NANO V3
 *  January 2024
 *  DashBoard Arduino code
 */
//INCLUDE LIBRARIES

#include <DHTStable.h>;
DHTStable DHT;





//CODE VARIABLES
int DC_JACK_STATE = 0;
int PWM1_STATE = 0;
int PWM2_STATE = 0;
float NTC1_VALUE = 0;
int PWM_AUTO = 0;
float NTC2_VALUE = 0;
float TEMP = 0.0;
int HUM_REL = 0;
float HUM_ABS = 0.0;
float DEWPOINT = 0.0;
float DP_OFFSET = 2;
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
float timetotal_ntc=0;
double PWR_TOTAL_S=0.00;
float PWR_TOTAL=0.00;
float NTC_DELAY;
float ACS_resolution;

//CONSTANTS  --- SET THESE TO YOUR MEASURED VALUES!
const int ACS_Variant = 20;
const int ntc_beta = 3380;
const int ntc_ohm  = 10000;
const int ref_ohm1 = 10080;
const int ref_ohm2 = 10000;
const float ACS_Bias = 2.494;

//PINS
const int DC_JACK = 4;
const int PWM1 = 5;
const int PWM2 = 6;
const int DHT22_VCC = 2;
const int DHT22_DATA = 3;
const int NTC_VCC = 7;
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
    pinMode(DHT22_VCC, OUTPUT);
    pinMode(NTC_VCC, OUTPUT);
    pinMode(VM, INPUT);
    pinMode(AM, INPUT);
    pinMode(NTC1, INPUT);
    pinMode(NTC2, INPUT);

    digitalWrite(DC_JACK, LOW);
    digitalWrite(DHT22_VCC, HIGH);
    digitalWrite(PWM1, LOW);
    digitalWrite(PWM2, LOW);
    digitalWrite(NTC_VCC,LOW);
    Serial.begin(9600);
    Serial.flush();
    switch (ACS_Variant){
    case 5: 
    ACS_resolution = 0.185;
    break;
    case 20: 
    ACS_resolution = 0.100;
    break;
    case 30:
    ACS_resolution = 0.066;
    break;
    }
}

//LOOP TO READ SERIAL COMMANDS
void loop()
{
  RUN_AUTO_PWM();
  GET_POWER();
  GET_NTC();
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
        else if (cmd == "GETNTC1") 
        {
          Serial.print(NTC1_VALUE);
            Serial.println("#");
        }
        else if (cmd == "GETNTC2") 
        {
          Serial.print(NTC2_VALUE);
            Serial.println("#");
        }
        else if (cmd == "GETAUTOPWM") 
        {
          Serial.print(PWM_AUTO);
            Serial.println("#");
        }
        else if (cmd == "SETAUTOPWM_ON") 
        {
            PWM_AUTO = 1;
            Serial.print(PWM_AUTO);
            Serial.println("#");
        }
        else if (cmd == "SETAUTOPWM_OFF") 
        {
          PWM_AUTO = 0;
          Serial.print(PWM_AUTO);
            Serial.println("#");
        }
        else if (cmd.substring(0,13) == "SETSTATUSPWM1") SET_PWM_POWER(1,cmd.substring((cmd.indexOf('_')+1),(cmd.indexOf('_')+4)).toInt()); 
        else if (cmd == "GETSTATUSPWM2") 
        {
            Serial.print(PWM2_STATE);
            Serial.println("#");
        }
        else if (cmd.substring(0,13) == "SETSTATUSPWM2") SET_PWM_POWER(2,cmd.substring((cmd.indexOf('_')+1),(cmd.indexOf('_')+4)).toInt());
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
    }
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

//SET PWM POWER
void SET_PWM_POWER(int pwmno,int state) {
    int pwm_pin = (state*255)/100;
    if (pwmno == 1)
    {
    analogWrite(PWM1, pwm_pin);
    PWM1_STATE = state;
    Serial.print(PWM1_STATE);
    Serial.println("#");
    }
    else if (pwmno == 2)
    {
    analogWrite(PWM2, pwm_pin);
    PWM2_STATE = state;
    Serial.print(PWM2_STATE);
    Serial.println("#");
    }
}
//END SET PWM POWER

// RUN PWM AUTO 
void RUN_AUTO_PWM() {
   if (HUM_REL>50) DP_OFFSET = ((HUM_REL/10)-5);

   if(PWM_AUTO == 1) // PWM1 AUTO
   {
    if (timetotal_ntc > 1023)
     {
     if (NTC1_VALUE > -40) {
     if (NTC1_VALUE - (DEWPOINT + DP_OFFSET) < 2 && PWM1_STATE < 100)
     {
       PWM1_STATE = PWM1_STATE + 5;
       PWM1_STATE = CORRECT_PWM(PWM1_STATE);
        SET_PWM_VALUE(1,PWM1_STATE);
      }
     if (NTC1_VALUE - (DEWPOINT + DP_OFFSET) > 4 && PWM1_STATE > 0)
      {
        PWM1_STATE = PWM1_STATE - 5;
        PWM1_STATE = CORRECT_PWM(PWM1_STATE);
        SET_PWM_VALUE(1,PWM1_STATE);
      }
     }
     if (NTC2_VALUE > -40) {
     if (NTC2_VALUE - (DEWPOINT + DP_OFFSET) < 2 && PWM2_STATE<100)
     {
       PWM2_STATE = PWM2_STATE + 5;
        PWM2_STATE = CORRECT_PWM(PWM2_STATE);
        SET_PWM_VALUE(2,PWM2_STATE);
     }
     if (NTC2_VALUE - (DEWPOINT + DP_OFFSET) > 4 && PWM2_STATE>0)
     {
       PWM2_STATE = PWM2_STATE - 5;
       PWM2_STATE = CORRECT_PWM(PWM2_STATE);
       SET_PWM_VALUE(2,PWM2_STATE);
     }
     }
        timetotal_ntc = 0;
     }
      timetotal_ntc += 1;
   }
   else if (PWM_AUTO == 0) timetotal_ntc = 0;
}
//END SET AUTO PWM POWER

//GET NTC TEMP
void GET_NTC(){
   if (NTC_DELAY == 250){
   digitalWrite(NTC_VCC,HIGH);
   int ntc1_read_sum = 0;
   int ntc2_read_sum = 0;
   float avg1_read = 0;
   float avg2_read = 0;
   for (int i=0;i<40;i++)
   {
    ntc1_read_sum +=analogRead(NTC1);
    delay(1);
    ntc2_read_sum +=analogRead(NTC2);
   }
   avg1_read = ntc1_read_sum/40;
   avg2_read = ntc2_read_sum/40;
   digitalWrite(NTC_VCC,LOW);
   avg1_read = (1023 / avg1_read)-1;
   avg2_read = (1023 / avg2_read)-1;
   avg1_read = ntc_ohm / avg1_read;
   avg2_read = ntc_ohm / avg2_read;
   float temp1_ntc = avg1_read / ref_ohm1 ;
   float temp2_ntc = avg2_read/ ref_ohm2;
   temp1_ntc=log(temp1_ntc);
   temp1_ntc /= ntc_beta;
   temp1_ntc +=1.0/(25 + 273.15);
   temp1_ntc = (1.0/temp1_ntc) - 273.15; 
   if (isnan(temp1_ntc)) temp1_ntc = -40;
   temp2_ntc=log(temp2_ntc);
   temp2_ntc /= ntc_beta;
   temp2_ntc +=1.0/(25 + 273.15);
   temp2_ntc = (1.0/temp2_ntc) - 273.15; 
   if (isnan(temp2_ntc))temp2_ntc = -40;
   NTC1_VALUE=temp1_ntc - 1;
   NTC2_VALUE=temp2_ntc - 1;
   NTC_DELAY = 0;
   }
NTC_DELAY+=1;
}
// END GET NTC TEMP

//MEASURE AND CALCULATE POWER USAGE
  void GET_POWER() {
    time1=millis();
    if (AVERAGE_COUNT == 150) AVERAGE_COUNT = 0;
    float VOLTAGE_SAMPLE_SUM=0;
    for (int v=0;v<150;v++) 
    {
      PIN_VALUE_V =  analogRead(VM);
      VOLTAGE_SAMPLE_SUM +=PIN_VALUE_V;     
    }
    VOLTAGE_SAMPLE_SUM /=150;
    VOLT_TEMP = (VOLTAGE_SAMPLE_SUM * 5.0) / 1024.0;   
    VOLT = VOLT_TEMP / 0.0982;        
    if (VOLT < 0.1) VOLT=0.0;    
    CURRENT_SAMPLE_SUM=0;
    for (int i=0;i<150;i++)
    {
      PIN_VALUE_A= analogRead(AM);
      CURRENT_SAMPLE_SUM = CURRENT_SAMPLE_SUM + PIN_VALUE_A;
    }
    AMP_AVERAGE[AVERAGE_COUNT] = (ACS_Bias - ((CURRENT_SAMPLE_SUM/150)*(5.0/1024.0))) / ACS_resolution;
    AVGAMP=0;
    for (int c=0;c<150;c++) AVGAMP += AMP_AVERAGE[c];
    AMP = AVGAMP/150;
    if (AMP< 0.01) AMP=0.0;
    PWR = VOLT * AMP;
    AVERAGE_COUNT = AVERAGE_COUNT + 1;
    time2=millis();
    PWR_TOTAL_S = PWR_TOTAL_S + ((PWR*(time2-time1))/1000); // total power in W*s
    PWR_TOTAL = PWR_TOTAL_S/3600;  // Total power used in W*h                                   
}
//END GET POWER USAGE

//GET AMBIENT CONDITIONS
  void GET_AMBIENT() {
  int PIN_VALUE_T = DHT.read22(DHT22_DATA);
   HUM_REL = DHT.getHumidity();
   TEMP = DHT.getTemperature();
   DEWPOINT  = (TEMP - (100 - HUM_REL) / 5);
 }
 //CORRECT PWM VALUE
 void SET_PWM_VALUE(int pwmno, int value){
  value = CORRECT_PWM(value);
  int pwm_pin = (value*255)/100;
  if (pwmno == 1) analogWrite (PWM1, pwm_pin);
  if (pwmno == 2) analogWrite (PWM2, pwm_pin);
 }
 int CORRECT_PWM(int state){
 if (state > 100) return 100;
 if (state < 0) return 0;
 else return state;
}
 
