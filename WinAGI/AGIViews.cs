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
  }
}