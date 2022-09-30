using NGenuity2.Common;
using NGenuity2.Devices.Headset.CloudAlphaWireless;
using NGenuity2.Devices.Headset.DTS;
using NGenuity2.Model;
using NGenuity2.Model.Headset;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace NGenuity2.Devices.Headset
{
  public class HeadsetCloudAlphaWireless : 
    HeadsetDTSHeadset<CMDHSCloudAlphaWirelessBase, CMDHSCloudAlphaWirelessShowLighting>
  {
    private Timer _timer;
    private bool _initing;
    private int _init_state;
    private const int INIT_STATE_BATTERY = 1;
    private const int INIT_STATE_CHARGE_STATUS = 2;
    private const int INIT_STATE_SIDETONE = 4;
    private const int INIT_STATE_MIC = 8;
    private const int INIT_STATE_AUTOPOWER = 16;
    private const int INIT_STATE_VOICE_PROMPT = 32;

    public override byte[] CreateBuffer() => new byte[this.Device.OutputReportByteLength];

    public override byte[] GetInputReport()
    {
      if (this.Device != null)
      {
        try
        {
          Thread.Sleep(20);
          return this.Device.ReadInputReport((byte) 33, 1000);
        }
        catch (Exception ex)
        {
        }
      }
      return (byte[]) null;
    }

    protected override void OnDeviceInputReportReceived(byte[] buffer)
    {
      base.OnDeviceInputReportReceived(buffer);
      if (buffer[0] != (byte) 33 || buffer[1] != (byte) 187)
        return;
      if (buffer[2] == (byte) 7)
      {
        this.AutoPowerOff = (int) buffer[3];
        this.CheckInitState(16);
      }
      if (buffer[2] == (byte) 9)
      {
        this.VoicePrompt = buffer[3] == (byte) 1;
        this.CheckInitState(32);
      }
      int num = (int) buffer[2];
      if (buffer[2] == (byte) 34 || buffer[2] == (byte) 5)
      {
        this.OnAudioDeviceMuted(AudioDeviceType.Sidetone, buffer[3] == (byte) 0, true);
        this.CheckInitState(4);
      }
      if (buffer[2] == (byte) 35 || buffer[2] == (byte) 10)
      {
        this.OnAudioDeviceMuted(AudioDeviceType.Microphone, buffer[3] == (byte) 1, true);
        this.CheckInitState(8);
      }
      if (buffer[2] == (byte) 36 && buffer[3] == (byte) 2 != this.Connected)
      {
        this.Connected = buffer[3] == (byte) 2;
        this.Sleeping = !this.Connected;
        if (this.Connected)
          this.GetBasicInfo();
      }
      if (buffer[2] == (byte) 37 || buffer[2] == (byte) 11)
      {
        this.Battery = (int) buffer[3];
        this.CheckInitState(1);
        this.CheckBatteryNotification();
      }
      if (buffer[2] != (byte) 38 && buffer[2] != (byte) 12)
        return;
      switch (buffer[3])
      {
        case 0:
          this.ChargingStatus = ChargingStatus.NoCharging;
          break;
        case 1:
          this.ChargingStatus = ChargingStatus.WireCharging;
          break;
      }
      this.CheckInitState(2);
    }

    public override bool UpgradeMode
    {
      get => base.UpgradeMode || this.ProductNeedUpdate;
      protected set => base.UpgradeMode = value;
    }

    public HeadsetCloudAlphaWireless()
    {
      this.Model = HyperXDeviceModel.Headset_CloudAlphaWireless;
      this.Engine.DeviceModel = this.Model;
      this.LowBatteryThreshold = 10;
    }

    protected override void InitDevice()
    {
      base.InitDevice();
      CMDHSCloudAlphaWirelessGetWirelessState getWirelessState = new CMDHSCloudAlphaWirelessGetWirelessState();
      getWirelessState.Execute((HyperXDevice) this);
      this.Connected = getWirelessState.Connected;
      this.Sleeping = !getWirelessState.Connected;
      CMDHSCloudAlphaWirelessGetPairingInfo wirelessGetPairingInfo = new CMDHSCloudAlphaWirelessGetPairingInfo();
      wirelessGetPairingInfo.Execute((HyperXDevice) this);
      this.PairID = wirelessGetPairingInfo.PairID;
      CMDHSCloudAlphaWirelessGetAutoShutdown wirelessGetAutoShutdown = new CMDHSCloudAlphaWirelessGetAutoShutdown();
      wirelessGetAutoShutdown.Execute((HyperXDevice) this);
      if (wirelessGetAutoShutdown.Duration.HasValue)
        this.AutoPowerOff = (int) wirelessGetAutoShutdown.Duration.Value;
      if (!getWirelessState.Connected)
        return;
      this.GetBasicInfo();
    }

    private void OnCommandExecuted(HXCommandBase cmd, object info)
    {
      if (cmd is CMDHSCloudAlphaWirelessGetBatteryInfo wirelessGetBatteryInfo && wirelessGetBatteryInfo.Battery.HasValue)
      {
        this.Battery = wirelessGetBatteryInfo.Battery.Value;
        this.CheckInitState(1);
        this.CheckBatteryNotification();
      }
      if (cmd is CMDHSCloudAlphaWirelessGetChargeStatus wirelessGetChargeStatus && wirelessGetChargeStatus.Status.HasValue)
      {
        this.ChargingStatus = wirelessGetChargeStatus.Status.Value;
        this.CheckInitState(2);
      }
      bool? nullable;
      if (cmd is CMDHSCloudAlphaWirelessGetSidetoneStatus getSidetoneStatus && getSidetoneStatus.On.HasValue)
      {
        nullable = getSidetoneStatus.On;
        this.SidetoneMuted = !nullable.Value;
        this.CheckInitState(4);
      }
      if (cmd is CMDHSCloudAlphaWirelessGetMicMute wirelessGetMicMute)
      {
        nullable = wirelessGetMicMute.Muted;
        if (nullable.HasValue)
        {
          nullable = wirelessGetMicMute.Muted;
          this.MicrophoneMuted = nullable.Value;
          this.CheckInitState(8);
        }
      }
      if (cmd is CMDHSCloudAlphaWirelessGetAutoShutdown wirelessGetAutoShutdown && wirelessGetAutoShutdown.Duration.HasValue)
      {
        this.AutoPowerOff = (int) wirelessGetAutoShutdown.Duration.Value;
        this.CheckInitState(16);
      }
      if (cmd is CMDHSCloudAlphaWirelessGetVoicePromptStatus voicePromptStatus)
      {
        nullable = voicePromptStatus.On;
        if (nullable.HasValue)
        {
          nullable = voicePromptStatus.On;
          this.VoicePrompt = nullable.Value;
          this.CheckInitState(32);
        }
      }
      if (!(cmd is CMDHSCloudAlphaWirelessAPO alphaWirelessApo))
        return;
      this.HandleAPOCommand(alphaWirelessApo.Action, info);
    }

    private void CheckInitState(int state)
    {
      if (!this._initing)
        return;
      this._init_state |= state;
      if (this._init_state != 63)
        return;
      this._initing = false;
      HyperXCenter.Center.OnDeviceUpdated((HyperXDevice) this);
    }

    private void GetBasicInfo()
    {
      this._initing = true;
      this._init_state = 0;
      HXCommandCollection<CMDHSCloudAlphaWirelessBase> cmd1 = new HXCommandCollection<CMDHSCloudAlphaWirelessBase>();
      cmd1.Handler = new HXCommandHandler(this.OnCommandExecuted);
      CMDHSCloudAlphaWirelessGetBatteryInfo cmd2 = new CMDHSCloudAlphaWirelessGetBatteryInfo();
      cmd2.Handler = new HXCommandHandler(this.OnCommandExecuted);
      cmd1.AddCommand((CMDHSCloudAlphaWirelessBase) cmd2);
      CMDHSCloudAlphaWirelessGetChargeStatus cmd3 = new CMDHSCloudAlphaWirelessGetChargeStatus();
      cmd3.Handler = new HXCommandHandler(this.OnCommandExecuted);
      cmd1.AddCommand((CMDHSCloudAlphaWirelessBase) cmd3);
      CMDHSCloudAlphaWirelessGetSidetoneStatus cmd4 = new CMDHSCloudAlphaWirelessGetSidetoneStatus();
      cmd4.Handler = new HXCommandHandler(this.OnCommandExecuted);
      cmd1.AddCommand((CMDHSCloudAlphaWirelessBase) cmd4);
      CMDHSCloudAlphaWirelessGetMicMute cmd5 = new CMDHSCloudAlphaWirelessGetMicMute();
      cmd5.Handler = new HXCommandHandler(this.OnCommandExecuted);
      cmd1.AddCommand((CMDHSCloudAlphaWirelessBase) cmd5);
      CMDHSCloudAlphaWirelessGetAutoShutdown cmd6 = new CMDHSCloudAlphaWirelessGetAutoShutdown();
      cmd6.Handler = new HXCommandHandler(this.OnCommandExecuted);
      cmd1.AddCommand((CMDHSCloudAlphaWirelessBase) cmd6);
      CMDHSCloudAlphaWirelessGetVoicePromptStatus cmd7 = new CMDHSCloudAlphaWirelessGetVoicePromptStatus();
      cmd7.Handler = new HXCommandHandler(this.OnCommandExecuted);
      cmd1.AddCommand((CMDHSCloudAlphaWirelessBase) cmd7);
      this.AddCommand(cmd1);
    }

    public override void OpenDevice(string deviceId)
    {
      this.DeviceID = deviceId;
      this.AddCommand((CMDHSCloudAlphaWirelessBase) new CMDHSCloudAlphaWirelessAPO(new HXCommandHandler(this.OnCommandExecuted))
      {
        Action = DTSAPOCommand.Initialize
      });
      this.SendDiscordCertificateDeviceNotificationAsync();
      this.Connected = false;
      this.Sleeping = true;
      base.OpenDevice(deviceId);
      if (!this.IsOpened)
        return;
      this.OpenNotificationTunnel();
      this.UpdateID();
      this.SetupDevice();
    }

    public override void ApplyBasicSettings()
    {
      base.ApplyBasicSettings();
      this.ApplyVolumeSettings();
      this.ApplyMicrophoneSettings();
      this.ApplySidetoneSettings();
      this.ApplySurroundSettings();
      this.ApplyGamechatSettings();
      this.ApplyKeyAssignments();
    }

    public override void ApplyPreset(Preset preset)
    {
      base.ApplyPreset(preset);
      this.ApplyBasicSettings();
      if (preset == null)
        return;
      EqualizerPreset equalizerPreset = preset.Headset.Equalizers.FirstOrDefault<EqualizerPreset>((Func<EqualizerPreset, bool>) (o => o.Selected));
      if (equalizerPreset == null)
        this.ApplyEQBands((List<EQSetting>) null);
      else
        this.ApplyEQBands(equalizerPreset.Bands);
    }

    public override void ApplyGameEQ(KnownGames game) => base.ApplyGameEQ(game);

    public override void ChangeEQStatus(bool enabled)
    {
      base.ChangeEQStatus(enabled);
      this.AddCommand((CMDHSCloudAlphaWirelessBase) new CMDHSCloudAlphaWirelessAPO(new HXCommandHandler(this.OnCommandExecuted))
      {
        Action = DTSAPOCommand.EnableEQ,
        Info = (object) enabled
      });
    }

    public override void ApplySurroundSettings()
    {
      base.ApplySurroundSettings();
      Preset preset = (Preset) null;
      lock (this)
        preset = this.Preset;
      if (preset == null)
        return;
      bool flag = preset.Headset.DSPMode != 0;
      this.SurroundSound = flag;
      this.AddCommand((CMDHSCloudAlphaWirelessBase) new CMDHSCloudAlphaWirelessAPO(new HXCommandHandler(this.OnCommandExecuted))
      {
        Action = DTSAPOCommand.EnableSurroundSound,
        Info = (object) flag
      });
    }

    public override void ChangeAutoPowerOff(int autoPowerOff)
    {
      base.ChangeAutoPowerOff(autoPowerOff);
      this.AddCommand((CMDHSCloudAlphaWirelessBase) new CMDHSCloudAlphaWirelessSetAutoShutdown()
      {
        Duration = (byte) autoPowerOff
      });
    }

    public override void ApplySidetoneSettings()
    {
      base.ApplySidetoneSettings();
      Preset safePreset = this.GetSafePreset();
      if (safePreset == null || this.SidetoneMuted == safePreset.Headset.SidetoneMuted)
        return;
      this.SidetoneMuted = safePreset.Headset.SidetoneMuted;
      this.AddCommand((CMDHSCloudAlphaWirelessBase) new CMDHSCloudAlphaWirelessSetSidetoneStatus()
      {
        On = !safePreset.Headset.SidetoneMuted
      });
    }

    public override void ChangeVoicePrompt(bool enabled)
    {
      base.ChangeVoicePrompt(enabled);
      this.AddCommand((CMDHSCloudAlphaWirelessBase) new CMDHSCloudAlphaWirelessSetVoicePromptStatus()
      {
        On = enabled
      });
    }

    public override void ChangeSpatialStatus(bool enabled)
    {
      base.ChangeSpatialStatus(enabled);
      this.SetSpatialEnabled(enabled);
    }

    public override void ApplyEQSettings(List<EQSetting> settings) => this.ApplyEQBands(settings);

    public override void PreStop()
    {
      this.AddCommand((CMDHSCloudAlphaWirelessBase) new CMDHSCloudAlphaWirelessAPO(new HXCommandHandler(this.OnCommandExecuted))
      {
        Action = DTSAPOCommand.Deinitialize
      });
      base.PreStop();
    }

    public override void EnterPairMode() => base.EnterPairMode();

    private void TimerTicked(object state)
    {
      if (!this.Connected)
        return;
      CMDHSCloudAlphaWirelessGetBatteryInfo cmd1 = new CMDHSCloudAlphaWirelessGetBatteryInfo();
      cmd1.Handler = new HXCommandHandler(this.OnCommandExecuted);
      this.AddCommand((CMDHSCloudAlphaWirelessBase) cmd1);
      CMDHSCloudAlphaWirelessGetChargeStatus cmd2 = new CMDHSCloudAlphaWirelessGetChargeStatus();
      cmd2.Handler = new HXCommandHandler(this.OnCommandExecuted);
      this.AddCommand((CMDHSCloudAlphaWirelessBase) cmd2);
    }

    public override void Start()
    {
      base.Start();
      this._timer = new Timer(new TimerCallback(this.TimerTicked), (object) null, new TimeSpan(0L), new TimeSpan(0, 0, 30));
    }

    public override void Stop(bool waitUntilStopped)
    {
      base.Stop(waitUntilStopped);
      this._timer?.Dispose();
      this._timer = (Timer) null;
    }

    private void ApplyEQBands(List<EQSetting> settings)
    {
      if (settings == null || settings.Count < 10)
      {
        settings = new List<EQSetting>();
        settings.Add(new EQSetting()
        {
          BandID = 0,
          Frequency = 32,
          Gain = 0.0f
        });
        settings.Add(new EQSetting()
        {
          BandID = 1,
          Frequency = 64,
          Gain = 0.0f
        });
        settings.Add(new EQSetting()
        {
          BandID = 2,
          Frequency = 125,
          Gain = 0.0f
        });
        settings.Add(new EQSetting()
        {
          BandID = 3,
          Frequency = 250,
          Gain = 0.0f
        });
        settings.Add(new EQSetting()
        {
          BandID = 4,
          Frequency = 500,
          Gain = 0.0f
        });
        settings.Add(new EQSetting()
        {
          BandID = 5,
          Frequency = 1000,
          Gain = 0.0f
        });
        settings.Add(new EQSetting()
        {
          BandID = 6,
          Frequency = 2000,
          Gain = 0.0f
        });
        settings.Add(new EQSetting()
        {
          BandID = 7,
          Frequency = 4000,
          Gain = 0.0f
        });
        settings.Add(new EQSetting()
        {
          BandID = 8,
          Frequency = 8000,
          Gain = 0.0f
        });
        settings.Add(new EQSetting()
        {
          BandID = 9,
          Frequency = 16000,
          Gain = 0.0f
        });
      }
      int[] numArray = new int[settings.Count];
      foreach (EQSetting setting in settings)
      {
        if (setting.BandID >= 10)
          return;
        numArray[setting.BandID] = (int) ((double) setting.Gain * 100.0);
      }
      this.AddCommand((CMDHSCloudAlphaWirelessBase) new CMDHSCloudAlphaWirelessAPO(new HXCommandHandler(this.OnCommandExecuted))
      {
        Action = DTSAPOCommand.UpdateEQ,
        Info = (object) numArray
      });
    }

    public override List<string> CheckDrivers()
    {
      List<DriverInfo> driverInfoList = new List<DriverInfo>();
      string installedLocation = Utils.InstalledLocation;
      this.AddDriverInfo(driverInfoList, Path.Combine(installedLocation, "Assets\\Native\\AudioDTS\\hyperxuac_caw.inf"));
      this.AddDriverInfo(driverInfoList, Path.Combine(installedLocation, "Assets\\Native\\AudioDTS\\dtshpxv2_hyperx_ext.inf"));
      this.AddDriverInfo(driverInfoList, Path.Combine(installedLocation, "Assets\\Native\\AudioDTS\\dtsapo4xhpxv2x64.inf"));
      return this.CheckDrivers(driverInfoList);
    }

    protected override bool InstallDTSDriver(List<string> drivers)
    {
      base.InstallDTSDriver(drivers);
      bool flag = false;
      string installedLocation = Utils.InstalledLocation;
      try
      {
        string driverPathes = "DEVID=" + this.DeviceID + " ";
        drivers.ForEach((Action<string>) (o => driverPathes = driverPathes + "\"" + o + "\" "));
        using (Process process = new Process())
        {
          process.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NGenuityDriverInstaller.exe");
          process.StartInfo.Arguments = driverPathes;
          process.StartInfo.UseShellExecute = true;
          process.StartInfo.CreateNoWindow = false;
          process.StartInfo.Verb = "runas";
          process.Start();
          process.WaitForExit();
          Console.WriteLine(string.Format("{0}", (object) process.ExitCode));
          flag = process.ExitCode == 0;
          process.Close();
        }
      }
      catch (Exception ex)
      {
      }
      if (flag)
        NotificationCenter.PopMessage(Settings.Instance.GetDeviceName(this.DeviceID, HyperXDeviceUtils.GetDeviceTitle((HyperXDevice) this)), Utils.GetResourceString("RebootComputerToApplyDTSEffect"));
      return flag;
    }
  }
}
