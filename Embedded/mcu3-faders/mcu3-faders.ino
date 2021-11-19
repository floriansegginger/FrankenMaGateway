#include "Conceptinetics.h"

// DMX Stuff
#define DMX_NUM_CHANNELS 24
#define DMX_START_ADDRESS 1
#define DMX_SNAP_0_VALUE 7
#define DMX_SNAP_255_VALUE 230

// BUTTONS STUFF
#define BUTTONS_NUM_INPUTS 12
#define BUTTON_WAIT_COUNTER 150
#define BUTTON_WAIT_COUNTER_EXTREME 0
#define BUTTON_DMX_IGNORE_THRESHOLD 0
int button_inputs[] = {
    4,
    5,
    6,
    7,
    8,
    9,
    10,
    11,
    A0,
    A1,
    A2,
    A3
};

int button_values[BUTTONS_NUM_INPUTS];
int button_counters[BUTTONS_NUM_INPUTS];


DMX_Slave dmxIn(DMX_NUM_CHANNELS);
unsigned char dmx_values[DMX_NUM_CHANNELS];

void setup() {

    Serial.begin(115200);

    // DMX
    dmxIn.enable();
    dmxIn.setStartAddress(DMX_START_ADDRESS);
    for (int i = 0; i < DMX_NUM_CHANNELS; i++) {
        dmx_values[i] = 0;
    }

    // Buttons
    for (int i = 0; i < BUTTONS_NUM_INPUTS; i++) {
        pinMode(button_inputs[i], INPUT);
        button_values[i] = 1;
        button_counters[i] = 0;
    }

    delay(1000);
}

void loop() {
    for (int i = 0; i < DMX_NUM_CHANNELS; i++) {
        auto value = dmxIn.getChannelValue(i + DMX_START_ADDRESS);
        auto raw_value = value;


        float new_value = fmin(1, fmax(0, (float)value - DMX_SNAP_0_VALUE) / (float)DMX_SNAP_255_VALUE) * 255;
        value = (int)new_value;

        int fader_id = 0;
        if (i >= 12) {
            fader_id = i - 12;
        } else {
            fader_id = i + 12;
        }

        if (i >= BUTTONS_NUM_INPUTS) {
            if (button_values[i - BUTTONS_NUM_INPUTS] != 1 && button_counters[i - BUTTONS_NUM_INPUTS] >= BUTTON_WAIT_COUNTER) {
                if (value != dmx_values[i]) {
                    Serial.println((String)"FADER " + fader_id + " " + value);
                }
                dmx_values[i] = value;
            }
        } else {
            if (value != dmx_values[i]) {
                Serial.println((String)"FADER " + fader_id + " " + value);
            }
            dmx_values[i] = value;
        }
    }

    for (int i = 0; i < BUTTONS_NUM_INPUTS; i++) {
        int value = digitalRead(button_inputs[i]);

        if (button_values[i] != value) {
            button_counters[i] = 0;
        }
        if (button_counters[i] < BUTTON_WAIT_COUNTER) {
            button_counters[i]++;
        }

        if (value != button_values[i]) {
            if (value == 1) {
                Serial.println((String)"FADER_BUTTON DOWN " + i);
            } else {
                Serial.println((String)"FADER_BUTTON UP " + i);
            }
        }
        button_values[i] = value;
    }

    // for (int i = 0; i < DMX_NUM_CHANNELS; i++) {
    //     Serial.print(dmx_values[i]);
    //     Serial.print(" ");
    // }
    // for (int i = 0; i < BUTTONS_NUM_INPUTS; i++) {
    //     Serial.print(button_values[i]);
    //     Serial.print(" ");
    // }

    // Serial.println("");

    // delay(1);
}
