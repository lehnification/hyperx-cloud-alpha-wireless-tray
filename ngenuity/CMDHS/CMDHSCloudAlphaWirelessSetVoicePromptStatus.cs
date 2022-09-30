namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public class CMDHSCloudAlphaWirelessSetVoicePromptStatus : CMDHSCloudAlphaWirelessBase
  {
    public bool On { get; set; }

    public override void Execute(HyperXDevice device)
    {
      base.Execute(device);
      byte[] buffer = device.CreateBuffer();
      buffer[0] = (byte) 33;
      buffer[1] = (byte) 187;
      buffer[2] = (byte) 19;
      buffer[3] = !this.On ? (byte) 0 : (byte) 1;
      device.SetOutputReport(buffer);
    }
  }
}
