changeDefaultAudio(connected) {
    if (connected) {
      // TODO Change Audio to Headset
    } else {
      //changeAudioToSpeaker
    }
  }

  executeSoundChange() {}


  sendMuteStatusBuffer() {
    const buffer = Buffer.from([
      0x21, //33
      0xbb, //187
      0x15, //21
      0x01, //1
    ]);
    const bufferUnMute = Buffer.from([
      0x21, //33
      0xbb, //187
      0x15, //21
      0x00, //0
    ]);
    
    if(this.status.muted) {
      buffer = b
    }
    
  }

  sendMonitoringStatusBuffer() {
    const buffer = Buffer.from([
      0x21, //33
      0xbb, //187
      0x10, //16
    ]);
    this.sendBuffer(buffer);
  }
