#define ENCODER_DO_NOT_USE_INTERRUPTS
#include <Encoder.h>

// Encoders
Encoder enc1(4, 1); 
Encoder enc2(5, 0);
Encoder enc3(6, 2);
Encoder enc4(7, 3);

#define DEBOUNCE_WHEELS 5
#define DEBOUNCE 5

#define COUNT_WHEELS 4
Encoder wheelEncoders[] = {
	Encoder(4, 1),
	Encoder(5, 0),
	Encoder(6, 2),
	Encoder(7, 3)
};
int wheelValues[] = {0, 0, 0, 0};
int wheelButtonPins[] = {10, 16, 14, 15};
bool wheelButtons[] = {false, false, false, false};
unsigned long wheelButtonTimes[] = {0, 0, 0, 0};

#define COUNT_SPECIALS 3
int specialButtonPins[] = {A0, A1, A2};
bool specialButtons[] = {false, false ,false};
unsigned long specialTimes[] = {0, 0, 0};

void setup() {
	Serial.begin(115200);

	for (int i = 0; i < COUNT_WHEELS; i++) {
		pinMode(wheelButtonPins[i], INPUT_PULLUP);
	}

	for (int i = 0; i < COUNT_SPECIALS; i++) {
		pinMode(specialButtonPins[i], INPUT_PULLUP);
	}
}

void loop() {
	for (int i = 0; i < COUNT_WHEELS; i++) {
		int value = wheelEncoders[i].read();
		if (value != wheelValues[i]) {
			Serial.print("ENCODER ");
			Serial.print(i);
			Serial.print(" ");
			if (value != wheelValues[i]) {
				Serial.print(value - wheelValues[i]);
				Serial.println();
			}
		}
		wheelValues[i] = value;
	}

	for (int i = 0; i < COUNT_WHEELS; i++) {
		int value = digitalRead(wheelButtonPins[i]);
		if (value != wheelButtons[i]) {
			wheelButtonTimes[i] = millis();
		}
		if (millis() - wheelButtonTimes[i] > DEBOUNCE_WHEELS) {
			if (value == 0 && !wheelButtons[i]) {
				Serial.print("ENCODER_BUTTON ");
				Serial.print(i);
				Serial.println();
			}
			wheelButtons[i] = !((bool)value);
		}
	}

	for (int i = 0; i < COUNT_SPECIALS; i++) {
		int value = digitalRead(specialButtonPins[i]);
		if (value != specialButtons[i]) {
			specialTimes[i] = millis();
		}
		if (millis() - specialTimes[i] > DEBOUNCE) {
			if (value) {
				Serial.println((String)"SPECIAL_KEY UP " + i);
			} else {
				Serial.println((String)"SPECIAL_KEY DOWN " + i);
			}
			specialButtons[i] = !((bool)value);
		}
	}
}