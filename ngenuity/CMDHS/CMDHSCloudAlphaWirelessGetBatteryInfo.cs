namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public class CMDHSCloudAlphaWirelessGetBatteryInfo : CMDHSCloudAlphaWirelessBase
  {
    public int? Battery { get; set; }

    public override void Execute(HyperXDevice device)
    {
      base.Execute(device);
      byte[] buffer = device.CreateBuffer();
      buffer[0] = (byte) 33;
      buffer[1] = (byte) 187;
      buffer[2] = (byte) 11;
      device.SetOutputReport(buffer);
      byte[] inputReport = device.GetInputReport();
      if (inputReport == null || (int) inputReport[2] != (int) buffer[2])
        return;
      this.Battery = new int?((int) inputReport[3]);
      HXCommandHandler handler = this.Handler;
      if (handler == null)
        return;
      handler((HXCommandBase) this, (object) this.Battery);
    }
  }
}
