using System;

namespace WinAGI
{
  public class AGIPictures
  {
    public AGIPicture this[byte index]
    { get { return Col[index]; } }

    internal AGIPicture[] Col
    { get; private set; }

    public byte Max
    { get; }

    public void Clear()
    {
      // add code here
    }

    internal void LoadPicture(agSnds.LoadSound bytResNum, agSnds.LoadSound bytVol, agSnds.LoadSound lngLoc)
    {
      throw new NotImplementedException();
    }
  }
}