using System;

namespace WinAGI
{
  public class AGIViews
  {
    public AGIView this[byte index]
    { get { return Col[index]; } }

    internal AGIView[] Col
    { get; private set; }

    public byte Max
    { get; }

    public void Clear()
    {
      // add code here
    }

    internal void LoadView(byte bytResNum, byte bytVol, int lngLoc)
    {
      throw new NotImplementedException();
    }

    internal bool Exists(byte bytResNum)
    {
      throw new NotImplementedException();
    }
  }
}