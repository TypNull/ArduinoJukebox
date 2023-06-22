#include <MeMCore.h>
#include <Arduino.h>
#include <Wire.h>
#include <SoftwareSerial.h>

const int frequency[] = { 0, 31, 33, 35, 37, 39, 41, 44, 46, 49, 52, 55, 58, 62, 65, 69, 73, 78, 82, 87, 93, 98, 104, 110, 117, 123, 131, 139, 147, 156, 165, 175, 185, 196, 208, 220, 233, 247, 262, 277, 294, 311, 330, 349, 370, 392, 415, 440, 466, 494, 523, 554, 587, 622, 659, 698, 740, 784, 831, 880, 932, 988, 1047, 1109, 1175, 1245, 1319, 1397, 1480, 1568, 1661, 1760, 1865, 1976, 2093, 2217, 2349, 2489, 2637, 2794, 2960, 3136, 3322, 3520, 3729, 3951, 4186, 4435, 4699, 4978, 0 };

MeBuzzer buzzer;
MeRGBLed rgbled_7(7, 2);


MeDCMotor motor_9(9);
MeDCMotor motor_10(10);

bool isRandom = false;
bool isPressed = false;
bool isPlaying = false;
bool isMuted = false;
bool isDancing = false;

byte bufferLength;
byte tempo = 100;
byte buffer[16];
byte savedByte = 255;

int wholenote = (60000 * 4) / (tempo * 2);

void setup() {
  Serial.begin(115200);
  pinMode(A7, INPUT);
  rgbled_7.fillPixelsBak(0, 2, 1);
  rgbled_7.setColor(0, 0, 0, 0);
  rgbled_7.show();
  Serial.println("<Arduino is ready>");
}

void addItem(byte item) {
  if (bufferLength < 16) buffer[bufferLength++] = item;
}
void removeAt(byte index) {
  if (index >= bufferLength) return;
  memmove(&buffer[index], &buffer[index + 1], bufferLength - index - 1);
  bufferLength--;
}

void proofInput() {
  if (analogRead(A7) < 10) {
    if (!isPressed) {
      isRandom = !isRandom;
      isPressed = true;
      Serial.print(isRandom ? 1 : 2);
    }
  } else
    isPressed = false;
}

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

void insertInList(byte fequence, byte length) {
  if (fequence > 0 && fequence < 100)
    addItem(fequence);
  else {
    Serial.println("Lost");
    return;
  }
  if (length == 254)
    length = Serial.read();
  if (length > 100 && length < 180) {
    addItem(length);
  } else {
    removeAt((byte)0);
    Serial.println("Lost");
  }
}

void clearBuffer() {
  while (bufferLength != 0)
    removeAt((byte)0);
}

/*void lightASecond(byte r, byte g, byte b) {
  rgbled_7.setColor(0, r, g, b);
  rgbled_7.show();
  delay(100);
  rgbled_7.setColor(0, 0, 0, 0);
  rgbled_7.show();
}*/

void _delay(int ms) {
  long endTime = millis() + ms;
  while (millis() < endTime) getSerialInput();
}

void loop() {
  if (bufferLength > 1 && isPlaying) {
    int noteDuration = wholenote;
    if (buffer[1] > 100 && buffer[1] < 125) {
      noteDuration = (wholenote) / (buffer[1] - 100);
    } else if (buffer[1] > 150 && buffer[1] < 175) {
      noteDuration = (wholenote) / (buffer[1] - 150);
      noteDuration *= 1.5;
    }
    if (!isMuted) {

      buzzer.tone(frequency[buffer[0]], noteDuration * 0.9f);
      if (isDancing)
        dance();
    } else {
      rgbled_7.setColor(0, 0, 0, 0);
      rgbled_7.show();
    }

    _delay(noteDuration);
    move(1, 0);
    removeAt((byte)0);
    removeAt((byte)0);
  } else {
    rgbled_7.setColor(0, 0, 0, 0);
    rgbled_7.show();
  }
  getSerialInput();
}

void dance() {
  rgbled_7.setColor(random(3), random(256), random(256), random(256));
  rgbled_7.show();
  move(random(5), 50 / 100.0 * 255 * (random(-2, 2)));
}

void move(int direction, int speed) {
  int leftSpeed = 0;
  int rightSpeed = 0;
  if (direction == 1) {
    leftSpeed = speed;
    rightSpeed = speed;
  } else if (direction == 2) {
    leftSpeed = -speed;
    rightSpeed = -speed;
  } else if (direction == 3) {
    leftSpeed = -speed;
    rightSpeed = speed;
  } else if (direction == 4) {
    leftSpeed = speed;
    rightSpeed = -speed;
  }
  motor_9.run((9) == M1 ? -(leftSpeed) : (leftSpeed));
  motor_10.run((10) == M1 ? -(rightSpeed) : (rightSpeed));
}
