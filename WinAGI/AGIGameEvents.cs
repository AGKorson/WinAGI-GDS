using System;

namespace WinAGI
{
  public class CompileGameEventArgs
  {
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
    internal static event CompileGameEventHandler CompileGameStatus;

    // Declare the delegate.
    internal delegate void LoadGameEventHandler(object sender, LoadGameEventArgs e);
    // Declare the event.
    internal static event LoadGameEventHandler LoadGameStatus;

    // Declare the delegate.
    internal delegate void CompileLogicEventHandler(object sender, CompileLogicEventArgs e);
    // Declare the event.
    internal static event CompileLogicEventHandler CompileLogicStatus;

    internal static void Raise_CompileGameEvent(ECStatus cStatus, AGIResType ResType, byte ResNum, string ErrString)
    {
      // Raise the event in a thread-safe manner using the ?. operator.
      CompileGameStatus?.Invoke(null, new CompileGameEventArgs(cStatus, ResType, ResNum, ErrString));
    }
    internal static void Raise_LoadGameEvent(ELStatus lStatus, AGIResType ResType, byte ResNum, string ErrString)
    {
      // Raise the event in a thread-safe manner using the ?. operator.
      LoadGameStatus?.Invoke(null, new LoadGameEventArgs(lStatus, ResType, ResNum, ErrString));
    }

    internal static void Raise_LogicCompileEvent(string strWarning, byte LogicNum)
    {
      // Raise the event in a thread-safe manner using the ?. operator.
      CompileLogicStatus?.Invoke(null, new CompileLogicEventArgs(strWarning, LogicNum));
    }
  }
}