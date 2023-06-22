# Arduino Jukebox
## _An Arduino jukebox for the buzzer_

<img align="right" src="https://github.com/Meyn1/ArduinoJukebox/blob/fb8abed79ec5bae5f405fef367865005b90603e3/ArduinoJukebox/logo.png" alt="logo" width="200"/>

A modern application that can connect to a ble arduino module and play songs trough a buzzer.
Example is for an MakeBot/MBot. Start, stop, mute, next, previous, random, dance function.
All songs based on https://github.com/robsoncouto/arduino-songs


## Features
- Scan devices 
- Play songs
- Play random songs
- Disconnect
- Mute
- Customisable button

## How to use

1. Download release: Install or portable version.
2. Download and install .Net 6 Framework [Here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
3. Unzip the folder and Copy the songs to your document directory.
4. Open the application.

Here is the Home Menu:
![off](https://user-images.githubusercontent.com/70847870/210411190-4f1ddb70-4357-4003-b413-37e12512288c.png)
![on](https://user-images.githubusercontent.com/70847870/210411558-14107b51-67f5-429b-99ec-3bb05f78641b.png)

Play and Shuffle songs.


Here is the Settings View. The Button in the top right corner can change the view.
![settings](https://user-images.githubusercontent.com/70847870/210412074-cd1280ed-2379-430c-99e2-d136dc152f8d.png)
- Select and connect to a device
- Change the song path

On application side:
- Send Byte = 3: Buffer is full please wait.

On arduino side:
- Send Byte = 200: New song will be played
- Send Byte = 201: Play song
- Send Byte = 202: Pause song
- Send Byte = 203: Mute song
- Send Byte = 204: Unmute song
- Send Byte = 205: On Customisable
- Send Byte = 206: Off Customisable
- Send Byte = 206: Customisable
- Send Byte > 100 < 180: Length of the played song
- Send Byte < 100: Frequescy of the song

```c++
void getSerialInput() {
  while (Serial.available() > 0) {
    byte readByte = Serial.read();
    if(tempo==-1){
      tempo = readByte*2;
      continue;
    }
    switch (readByte) {
      case 200:
        clearBuffer();
        tempo = Serial.read() * 2;
        if(tempo!= -1)
        isPlaying = true;
        break;
      case 201:
        isPlaying = true;
        break;
      case 202:
        isPlaying = false;
        break;
      case 203:
        isMuted = true;
        break;
      case 204:
        isMuted = false;
        break;
      case 205:
        isDancing = true;
        break;
      case 206:
        isDancing = false;
      rgbled_7.setColor(0, 0, 0, 0);
      rgbled_7.show();
        break;
      default:
        if (savedByte == 255 && Serial.available() > 0) {
            insertInList(readByte, 254);
        } else {
          if (savedByte == 255) {
            savedByte = readByte;
          } else {
            insertInList(savedByte, readByte);
            savedByte = 255;
          }
        }

        break;
    }
    if (bufferLength >= 8) {
      Serial.print(3);
    }
  }
  proofInput();
}
```
## License

MIT
