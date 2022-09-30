namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public class CMDHSCloudAlphaWirelessSetAutoShutdown : CMDHSCloudAlphaWirelessBase
  {
    public byte Duration { get; set; }

    public override void Execute(HyperXDevice device)
    {
      base.Execute(device);
      byte[] buffer = device.CreateBuffer();
      buffer[0] = (byte) 33;
      buffer[1] = (byte) 187;
      buffer[2] = (byte) 18;
      buffer[3] = this.Duration;
      device.SetOutputReport(buffer);
    }
  }
}
