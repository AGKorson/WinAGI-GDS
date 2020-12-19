using System;

namespace WinAGI
{
  public class AGILogics
  {
    public AGILogic this[byte index]
    { get { return Col[index]; } }

    internal AGILogic[] Col
    { get; private set; }

    public byte Max
    { get; }

    public void Clear()
    {
      // add code here
    }

    internal void LoadLogic(agPics.LoadPicture bytResNum, agPics.LoadPicture bytVol, agPics.LoadPicture lngLoc)
    {
      throw new NotImplementedException();
    }

    internal bool Exists(agPics.LoadPicture bytResNum)
    {
      throw new NotImplementedException();
    }
  }
}