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
}
}