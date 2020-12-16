namespace WinAGI
{
  public class AGIInventoryObjects
  {
    private bool mLoaded = false;
    private bool mInGame = false;
    public bool Loaded
    { get => mLoaded; }

    public bool InGame
    {
      get => mInGame;
      internal set { mInGame = value; }
    }
    public void Unload()
    {
      // add code here
    }
  }

}