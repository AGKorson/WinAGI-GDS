using System.ComponentModel;

namespace WinAGI
{
  public class AGILogic : AGIResource
  {
    public override void Load()
    {
      base.Load();
    }
    public override void Unload()
    {
      base.Unload();
    }
     public AGILogic()
    {
      //initialize
      //attach events
      base.PropertyChanged += ResPropChange;
    }

    public uint CompiledCRC
    { get; internal set; }

    private void ResPropChange(object sender, AGIResPropChangedEventArgs e)
    {
      ////let's do a test
      //// increment number everytime data changes
      //Number += 1;
    }
  }
}