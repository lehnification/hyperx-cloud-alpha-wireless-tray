namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public class CMDHSCloudAlphaWirelessGetChargeStatus : CMDHSCloudAlphaWirelessBase
  {
    public ChargingStatus? Status { get; set; }

    public override void Execute(HyperXDevice device)
    {
      base.Execute(device);
      byte[] buffer = device.CreateBuffer();
      buffer[0] = (byte) 33;
      buffer[1] = (byte) 187;
      buffer[2] = (byte) 12;
      device.SetOutputReport(buffer);
      byte[] inputReport = device.GetInputReport();
      if (inputReport == null || (int) inputReport[2] != (int) buffer[2])
        return;
      switch (inputReport[3])
      {
        case 0:
          this.Status = new ChargingStatus?(ChargingStatus.NoCharging);
          break;
        case 1:
          this.Status = new ChargingStatus?(ChargingStatus.WireCharging);
          break;
      }
      HXCommandHandler handler = this.Handler;
      if (handler == null)
        return;
      handler((HXCommandBase) this, (object) this.Status);
    }
  }
}
