namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public class CMDHSCloudAlphaWirelessSetMicMute : CMDHSCloudAlphaWirelessBase
  {
    public bool Muted { get; set; }

    public override void Execute(HyperXDevice device)
    {
      base.Execute(device);
      byte[] buffer = device.CreateBuffer();
      buffer[0] = (byte) 33;
      buffer[1] = (byte) 187;
      buffer[2] = (byte) 21;
      buffer[3] = !this.Muted ? (byte) 0 : (byte) 1;
      device.SetOutputReport(buffer);
    }
  }
}
