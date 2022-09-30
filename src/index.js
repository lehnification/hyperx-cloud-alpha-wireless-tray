const { app, BrowserWindow, Menu, Tray} = require('electron')
const path = require('path')
const Store = require('electron-store');
const CloudAlphaWireless = require('./assets/js/classes/CloudAlphaWireless.js')

const store = new Store();
let tray = null
let icons = []
let headset = null;

if (require('electron-squirrel-startup')) {
  app.quit()
}

app.whenReady().then(() => {
  populateIcons()
  tray = new Tray(icons['icon'])
  tray.setToolTip("Disconnected");

  const contextMenu = Menu.buildFromTemplate([
    { label: 'Quit', type: 'normal', click: () => {
      headset.stop()
      app.quit()
    }}
  ])
  
  tray.setContextMenu(contextMenu)

  initConfig()
  run(icons)
});

function run() {
  let updateDelay = store.get('updateDelay')
  headset = new CloudAlphaWireless(tray, icons, updateDelay)
  headset.runStatusUpdaterInterval()
  headset.runListener()
}

function initConfig() {
  if (store.get('updateDelay') === undefined) {
    store.set('updateDelay', 15);
  }
}

function populateIcons() {
  icons['icon'] = path.join(__dirname, 'assets/img/icon.ico')
  icons['0'] = path.join(__dirname, 'assets/img/0.ico')
  icons['17'] = path.join(__dirname, 'assets/img/17.ico')
  icons['33'] = path.join(__dirname, 'assets/img/33.ico')
  icons['50'] = path.join(__dirname, 'assets/img/50.ico')
  icons['67'] = path.join(__dirname, 'assets/img/67.ico')
  icons['83'] = path.join(__dirname, 'assets/img/83.ico')
  icons['100'] = path.join(__dirname, 'assets/img/100.ico')
  icons['charging_low'] = path.join(__dirname, 'assets/img/charging_low.ico')
  icons['charging_high'] = path.join(__dirname, 'assets/img/charging_high.ico')  
}