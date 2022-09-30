const HID = require("node-hid");

module.exports = class CloudAlphaWireless {
  static VENDOR_ID = 1008;
  static PRODUCT_ID = 2445;

  constructor(tray, icons, updateDelay, debug = false) {
    this.tray = tray;
    this.icons = icons;
    this.updateDelay = updateDelay;
    this.debug = debug;
    this.status = {
      battery: null,
      batteryHours: null,
      connected: false,
      muted: false,
      charging: false,
      monitoring: false,
    };
    this.init();
  }

  init() {
    const platform = process.platform;
    if (platform == "win32" || platform == "win64") {
      HID.setDriverType("libusb");
    }

    this.devices = HID.devices().filter(
      (d) =>
        d.vendorId === CloudAlphaWireless.VENDOR_ID &&
        d.productId === CloudAlphaWireless.PRODUCT_ID
    );

    this.device = this.devices.find((d) => d.usagePage === 65347);
  }

  runStatusUpdaterInterval() {
    this.updateInterval = setInterval(() => {
      this.sendBatteryUpdateBuffer();
    }, this.updateDelay * 60 * 1000);
  }

  runListener() {
    this.devices.map((info) => {
      info.device = new HID.HID(info.path);

      info.device.on("error", (err) => console.log(err));
      info.device.on("data", (data) => {
        if (this.debug) {
          var byteValues = "";
          for (let byte of data.slice(0, 10)) {
            byteValues += byte + " ";
          }
          console.log(
            new Date(),
            `length: ${data.length} \n`,
            data,
            "\n",
            byteValues
          );
        }
        this.handleStatusUpdates(data);
      });
    });
  }

  stop() {
    this.devices.map((info) => {
      info.device.close();
    });
    clearInterval(this.updateInterval);
  }

  handleStatusUpdates(buffer) {
    if (buffer[0] != 33 || buffer[1] != 187) {
      return;
    }

    if (buffer[2] == 3 || buffer[2] == 36) {
      this.status.connected = buffer[3] == 2;
    } else if (this.status.connected == false) {
      this.sendConnectionUpdateBuffer();
    }

    if (buffer[2] == 5 || buffer[2] == 34) {
      this.status.monitoring = buffer[3] == 1;
    }

    if (buffer[2] == 10 || buffer[2] == 35) {
      this.status.muted = buffer[3] == 1;
    }

    if (buffer[2] == 12 || buffer[2] == 38) {
      this.status.charging = buffer[3] == 1;
    }

    if (buffer[2] == 11 || buffer[2] == 37) {
      this.status.battery = buffer[3];
      this.status.batteryHours = 300 * (buffer[3] / 100);
    }
    this.handleTrayIcon();
    this.handleTrayToolTip();
    if (this.debug) {
      console.log(this.status);
    }
  }

  handleTrayIcon() {
    if (!this.status.connected) {
      this.tray.setImage(this.icons["icon"]);
    }

    let batteryPercentage = this.status.battery;
    if (this.status.charging) {
      if (batteryPercentage < 50) {
        this.tray.setImage(this.icons["charging_low"]);
      } else {
        this.tray.setImage(this.icons["charging_high"]);
      }
    }

    if (this.status.connected && !this.status.charging) {
      if (batteryPercentage < 17) {
        this.tray.setImage(this.icons["0"]);
      } else if (batteryPercentage < 33) {
        this.tray.setImage(this.icons["33"]);
      } else if (batteryPercentage < 50) {
        this.tray.setImage(this.icons["50"]);
      } else if (batteryPercentage < 67) {
        this.tray.setImage(this.icons["67"]);
      } else if (batteryPercentage < 83) {
        this.tray.setImage(this.icons["83"]);
      } else if (batteryPercentage > 83) {
        this.tray.setImage(this.icons["100"]);
      }
    }
  }

  handleTrayToolTip() {
    var status;
    if (this.status.connected) {
      status = "Connected";
    } else if (this.status.charging) {
      status = "Charging";
    } else {
      status = "Disconnected";
    }

    const str = `Status: ${status}
   Battery: ${this.status.battery}% (${this.status.batteryHours}h)
   Muted: ${this.status.muted}
   Monitoring: ${this.status.monitoring}`;
    this.tray.setToolTip(str);
  }

  sendBatteryUpdateBuffer() {
    const buffer = Buffer.from([
      0x21, //33
      0xbb, //187
      0x0b, //11
    ]);
    this.sendBuffer(buffer);
  }

  sendConnectionUpdateBuffer() {
    const buffer = Buffer.from([
      0x21, //33
      0xbb, //187
      0x03, //3
    ]);
    this.sendBuffer(buffer);
  }

  sendBuffer(buffer) {
    let headset = new HID.HID(this.device.path);
    try {
      headset.write(buffer);
      headset.close();
    } catch (e) {
      console.error(e);
    }
  }
};
