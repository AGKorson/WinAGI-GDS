using System;

namespace WinAGI
{
  public class AGIWordList
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

    public bool IsDirty { get; internal set; }
    public string ResFile { get; internal set; }

    public void Unload()
    {
      // add code here
    }

    internal void Save()
    {
      throw new NotImplementedException();
    }

    internal void Load(string v)
    {
     // throw new NotImplementedException();
    }
  }
}