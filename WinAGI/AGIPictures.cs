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
  }
}