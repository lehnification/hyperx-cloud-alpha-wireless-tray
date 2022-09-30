using NGenuity2.Devices.Headset.DTS;

namespace NGenuity2.Devices.Headset.CloudAlphaWireless
{
  public class CMDHSCloudAlphaWirelessAPO : CMDHSCloudAlphaWirelessBase
  {
    public CMDHSCloudAlphaWirelessAPO(HXCommandHandler handler)
    {
      this.Force = true;
      this.Handler = handler;
    }

    public DTSAPOCommand Action { get; set; }

    public object Info { get; set; }

    public override void Execute(HyperXDevice device)
    {
      base.Execute(device);
      HXCommandHandler handler = this.Handler;
      if (handler == null)
        return;
      handler((HXCommandBase) this, this.Info);
    }
  }
}
