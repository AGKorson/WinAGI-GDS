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

    internal void LoadPicture(byte bytResNum, byte bytVol, int lngLoc)
    {
      throw new NotImplementedException();
    }

    internal bool Exists(byte bytResNum)
    {
      throw new NotImplementedException();
    }
  }
}