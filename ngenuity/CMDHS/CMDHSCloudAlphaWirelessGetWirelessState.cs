namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public class CMDHSCloudAlphaWirelessGetWirelessState : CMDHSCloudAlphaWirelessBase
  {
    public bool Connected { get; private set; }

    public override void Execute(HyperXDevice device)
    {
      base.Execute(device);
      if (!this.IsDongle(device))
      {
        this.Connected = true;
      }
      else
      {
        byte[] buffer = device.CreateBuffer();
        buffer[0] = (byte) 33;
        buffer[1] = (byte) 187;
        buffer[2] = (byte) 3;
        device.SetOutputReport(buffer);
        byte[] inputReport = device.GetInputReport();
        if (inputReport == null || (int) inputReport[2] != (int) buffer[2])
          return;
        this.Connected = inputReport[3] == (byte) 2;
      }
    }
  }
}
