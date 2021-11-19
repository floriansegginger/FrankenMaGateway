#define PIN_L1  9
#define PIN_L2  10
#define PIN_L3  16
#define PIN_L4  14
#define PIN_L5  15
#define PIN_L6  A0
#define PIN_L7  A1
#define PIN_L8  A2
#define PIN_L9  A3

#define PIN_C1  8
#define PIN_C2  7
#define PIN_C3  6
#define PIN_C4  5
#define PIN_C5  4
#define PIN_C6  3
#define PIN_C7  2
#define PIN_C8  1
#define PIN_C9  0

#define COUNT_CONTROL 9
#define COUNT_INPUT 9

int control_pins[] = {
  PIN_C1,
  PIN_C2,
  PIN_C3,
  PIN_C4,
  PIN_C5,
  PIN_C6,
  PIN_C7,
  PIN_C8,
  PIN_C9
};

int input_pins[] = {
  PIN_L1,
  PIN_L2,
  PIN_L3,
  PIN_L4,
  PIN_L5,
  PIN_L6,
  PIN_L7,
  PIN_L8,
  PIN_L9
};

bool pressed[COUNT_CONTROL][COUNT_INPUT];

void setup() {
  // put your setup code here, to run once:
  pinMode(PIN_L1, INPUT_PULLUP);
  pinMode(PIN_L2, INPUT_PULLUP);
  pinMode(PIN_L3, INPUT_PULLUP);
  pinMode(PIN_L4, INPUT_PULLUP);
  pinMode(PIN_L5, INPUT_PULLUP);
  pinMode(PIN_L6, INPUT_PULLUP);
  pinMode(PIN_L7, INPUT_PULLUP);
  pinMode(PIN_L8, INPUT_PULLUP);
  pinMode(PIN_L9, INPUT_PULLUP);

  pinMode(PIN_C1, OUTPUT);
  pinMode(PIN_C2, OUTPUT);
  pinMode(PIN_C3, OUTPUT);
  pinMode(PIN_C4, OUTPUT);
  pinMode(PIN_C5, OUTPUT);
  pinMode(PIN_C6, OUTPUT);
  pinMode(PIN_C7, OUTPUT);
  pinMode(PIN_C8, OUTPUT);
  pinMode(PIN_C9, OUTPUT);

  Serial.begin(115200);
}

void loop() {
  for (int i = 0; i < COUNT_CONTROL; i++) {
    for (int j = 0; j < COUNT_CONTROL; j++) {
      if (i == j) {
        digitalWrite(control_pins[j], LOW);
      } else {
        digitalWrite(control_pins[j], HIGH);
      }
    }
    for (int n = 0; n < COUNT_INPUT; n++) {
      int val = digitalRead(input_pins[n]);
      if (val != pressed[i][n]) {
        if (val) {
          Serial.println((String)"KEY UP " + n + " " + i);
        } else {
          Serial.println((String)"KEY DOWN " + n + " " + i);
        }
      } 
      pressed[i][n] = (bool)val;
    }
    // delay(5);
  }
}
