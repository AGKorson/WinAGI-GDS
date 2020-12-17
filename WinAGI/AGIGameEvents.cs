using System;

namespace WinAGI
{
  public class CompileGameEventArgs
  {
    //Public Event CompileGameStatus(cStatus As ECStatus, ResType As AGIResType, ResNum As Byte, ErrString As String)
    public CompileGameEventArgs(ECStatus status, AGIResType restype, byte num, string errString)
    {
      cStatus = status;
      ResType = restype;
      ResNum = num;
      ErrString = errString;
    }
    public ECStatus cStatus { get; }
    public AGIResType ResType { get; }
    public byte ResNum { get; }
    public string ErrString { get; }
  }

  public class LoadGameEventArgs
  {
    //Public Event LoadStatus(lStatus As ELStatus, ResType As AGIResType, ResNum As Byte, ErrString As String)
    public LoadGameEventArgs(ELStatus status, AGIResType restype, byte num, string errString)
    {
      lStatus = status;
      ResType = restype;
      ResNum = num;
      ErrString = errString;
    }
    public ELStatus lStatus { get; }
    public AGIResType ResType { get; }
    public byte ResNum { get; }
    public string ErrString { get; }
  }

  public class CompileLogicEventArgs
  {
    //Public Event LogCompWarning(Warning As String, LogNum As Byte)
    public CompileLogicEventArgs(string warning, byte num)
    {
      Warning = warning;
      ResNum = num;
    }
    public string Warning { get; }
    public byte ResNum { get; }
  }

  // 
  public static partial class AGIGame
  {
    // Declare the delegate.
    internal delegate void CompileGameEventHandler(object sender, CompileGameEventArgs e);
    // Declare the event.
    static internal event CompileGameEventHandler CompileGameStatus;

    // Declare the delegate.
    internal delegate void LoadGameEventHandler(object sender, LoadGameEventArgs e);
    // Declare the event.
    static internal event LoadGameEventHandler LoadGameStatus;

    // Declare the delegate.
    internal delegate void CompileLogicEventHandler(object sender, CompileLogicEventArgs e);
    // Declare the event.
    static internal event CompileLogicEventHandler CompileLogicStatus;

    static internal void Raise_CompileGameEvent(ECStatus cStatus, AGIResType ResType, byte ResNum, string ErrString)
    {
      // Raise the event in a thread-safe manner using the ?. operator.
      CompileGameStatus?.Invoke(null, new CompileGameEventArgs(cStatus, ResType, ResNum, ErrString));
    }
    static internal void Raise_LoadGameEvent(ELStatus lStatus, AGIResType ResType, byte ResNum, string ErrString)
    {
      // Raise the event in a thread-safe manner using the ?. operator.
      LoadGameStatus?.Invoke(null, new LoadGameEventArgs(lStatus, ResType, ResNum, ErrString));
    }

    static internal void Raise_LogicCompileEvent(string strWarning, byte LogicNum)
    {
      // Raise the event in a thread-safe manner using the ?. operator.
      CompileLogicStatus?.Invoke(null, new CompileLogicEventArgs(strWarning, LogicNum));
    }
  }
}