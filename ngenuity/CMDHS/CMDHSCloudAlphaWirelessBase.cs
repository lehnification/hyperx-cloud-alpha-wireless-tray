namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public abstract class CMDHSCloudAlphaWirelessBase : HXCommandBase
  {
    public bool IsDongle(HyperXDevice device) => device.ProductID == (ushort) 5955 || device.ProductID == (ushort) 5989 || device.ProductID == (ushort) 2445;
  }
}
