using System;

namespace WinAGI
{
  // should this be public? or internal?
  public class AGIGameEvents
  {
    //Public Event CompileGameStatus(cStatus As ECStatus, ResType As AGIResType, ResNum As Byte, ErrString As String)
    //Public Event LoadStatus(lStatus As ELStatus, ResType As AGIResType, ResNum As Byte, ErrString As String)
    //Public Event LogCompWarning(Warning As String, LogNum As Byte)
    internal void RaiseEvent_CompileGameStatus(ECStatus cStatus, AGIResType ResType, byte ResNum, string ErrString)
    { 
      throw new NotImplementedException();
      //RaiseEvent CompileGameStatus(cStatus, ResType, ResNum, ErrString)
    }
    internal void RaiseEvent_LoadStatus(ELStatus lStatus, AGIResType ResType, byte ResNum, string ErrString)
    { 
    //RaiseEvent LoadStatus(lStatus, ResType, ResNum, ErrString)
    }

    internal void RaiseEvent_LogCompWarning(string strWarning, byte LogicNum)
    {
      //RaiseEvent LogCompWarning(strWarning, CByte(LogicNum))
    }
  }
}