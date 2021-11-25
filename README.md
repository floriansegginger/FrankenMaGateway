# FrankenMA Gateway

This is a small software solution to create an Arduino-based fader / button wing that connects to the GrandMA2 software.

My console has 3 Arduinos all connectd to a USB hub, and they are sending commands via the Serial port.

## Embedded
This directory contains all the embedded C++ code that runs inside the console.

My console has 3 arduinos:
* **mcu1-buttons**: All the "GrandMA" buttons such as Clear, Please, Store, etc. This code is designed to run on a board that has a grid of keys, similar to how keyboards actually work.
* **mcu2-wheels**: This arduino controls the 4 encoder wheels as well as 3 "special buttons" (Tools, Setup, Backup)
* **mcu3-faders**: This arduino is designed to be connected to whatever lighting console with an Arduino DMX shield. My build is also "stealing" button inputs from the console to act as the Exec 1 keys.

## FrankenMAGateway
This is a C# + WPF application designed to connect to the Arduinos via multiple serial ports. It merges commands from all the serial ports and connects to GrandMA2 via Telnet.

You can download an exe here if you don't want / can't compile it yourself: https://github.com/floriansegginger/FrankenMaGateway/releases/tag/1.0.0

You should modify the `Settings.json` file for your setup, more specifically PortNames and DmxUniverse

### How it's done
* **Hard keys (Clear, Please, etc.)**: Using LUA commands. Example `Lua "gma.canbus.hardkey(XXX,YYY,false)"`
* **Encoder wheels**: Using LUA commands. Example `Lua "gma.canbus.encoder(XXX,YYY,pressed)`
* **Faders**: Using the `FADER XX.YY AT ZZ` command
* **Exec buttons**: Using the `DMX XX.YYY AT ZZ` and remotes set up in GrandMA showfile.

