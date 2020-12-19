using System;

namespace WinAGI
{
  public class AGISounds
  {
    public AGISound this[byte index]
    { get { return Col[index]; } }

    internal AGISound[] Col
    { get; private set; }

    public byte Max
    { get; }

    public void Clear()
    {
      // add code here
    }

    internal void LoadSound(agViews.LoadView bytResNum, agViews.LoadView bytVol, agViews.LoadView lngLoc)
    {
      throw new NotImplementedException();
    }

    internal bool Exists(agViews.LoadView bytResNum)
    {
      throw new NotImplementedException();
    }
  }
}