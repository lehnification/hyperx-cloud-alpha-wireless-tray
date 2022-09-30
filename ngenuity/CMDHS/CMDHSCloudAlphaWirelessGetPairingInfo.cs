using NGenuity2.Common;

namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public class CMDHSCloudAlphaWirelessGetPairingInfo : CMDHSCloudAlphaWirelessBase
  {
    public int PairID { get; set; }

    public override void Execute(HyperXDevice device)
    {
      base.Execute(device);
      byte[] buffer = device.CreateBuffer();
      buffer[0] = (byte) 33;
      buffer[1] = (byte) 187;
      buffer[2] = (byte) 4;
      if (this.IsDongle(device))
        buffer[2] = (byte) 13;
      device.SetOutputReport(buffer);
      byte[] inputReport = device.GetInputReport();
      if (inputReport == null || (int) inputReport[2] != (int) buffer[2])
        return;
      this.PairID = Utils.BytesToInt(inputReport, 5);
    }
  }
}
