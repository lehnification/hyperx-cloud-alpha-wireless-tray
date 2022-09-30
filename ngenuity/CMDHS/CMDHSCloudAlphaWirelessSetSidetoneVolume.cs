namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public class CMDHSCloudAlphaWirelessSetSidetoneVolume : CMDHSCloudAlphaWirelessBase
  {
    public int Volume { get; set; }

    public override void Execute(HyperXDevice device)
    {
      base.Execute(device);
      byte[] buffer = device.CreateBuffer();
      buffer[0] = (byte) 33;
      buffer[1] = (byte) 187;
      buffer[2] = (byte) 17;
      device.SetOutputReport(buffer);
    }
  }
}
