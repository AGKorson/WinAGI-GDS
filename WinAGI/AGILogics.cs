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
  }
}