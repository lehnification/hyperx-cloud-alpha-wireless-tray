namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public class CMDHSCloudAlphaWirelessGetMicMute : CMDHSCloudAlphaWirelessBase
  {
    public bool? Muted { get; set; }

    public override void Execute(HyperXDevice device)
    {
      base.Execute(device);
      byte[] buffer = device.CreateBuffer();
      buffer[0] = (byte) 33;
      buffer[1] = (byte) 187;
      buffer[2] = (byte) 10;
      device.SetOutputReport(buffer);
      byte[] inputReport = device.GetInputReport();
      if (inputReport == null || (int) inputReport[2] != (int) buffer[2])
        return;
      this.Muted = new bool?(inputReport[3] == (byte) 1);
      HXCommandHandler handler = this.Handler;
      if (handler == null)
        return;
      handler((HXCommandBase) this, (object) this.Muted);
    }
  }
}
