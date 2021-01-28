using System;
using System.Linq;
using static WinAGI.Engine.AGICommands;
using static WinAGI.Engine.AGITestCommands;
using static WinAGI.Engine.ArgTypeEnum;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.WinAGI;
using static WinAGI.Common.WinAGI;

namespace WinAGI.Engine
{
  // should this be public? or internal?
  public class AGILogicSourceSettings
  {
    private bool agElseAsGoto;
    private bool agShowMsgs;
    private bool agSpecialSyntax;
    public LogicErrorLevel ErrorLevel
    { get; set; }
    public string ArgTypePrefix(byte index)
    {
      if (index > 8)
      {
        throw new IndexOutOfRangeException("subscript out of range");
      }
      return agArgTypPref[index];
    }
    public string ArgTypeName(byte index)
    {
      if (index > 8)
      {
        throw new IndexOutOfRangeException("subscript out of range");
      }
      return agArgTypName[index];
    }
    public void SetIgnoreWarning(int WarningNumber, bool NewVal)
    {
      //validate index
      if (WarningNumber < 5001 || WarningNumber > 5000 + WARNCOUNT)
      {
        throw new IndexOutOfRangeException("subscript out of range");
      }
      agNoCompWarn[WarningNumber - 5000] = NewVal;
    }
    public bool IgnoreWarning(int WarningNumber)
    {
      //validate index
      if (WarningNumber < 5001 || WarningNumber > 5000 + WARNCOUNT)
      {
        throw new IndexOutOfRangeException("subscript out of range");
      }
      return agNoCompWarn[WarningNumber - 5000];
    }
    public TDefine[] ReservedDefines(ArgTypeEnum ArgType)
    {
      //returns the reserved defines that match this argtype as an array of defines
      //NOT the same as reporting by //group// (which is used for saving changes to resdef names)
      int i;
      TDefine[] tmpDefines = Array.Empty<TDefine>();
      switch (ArgType)
      {
        case atNum:
          //return all numerical reserved defines
          tmpDefines = new TDefine[44];
          for (i = 0; i <= 4; i++)
          {
            tmpDefines[i] = agEdgeCodes[i];
          }
          for (i = 0; i <= 8; i++)
          {
            tmpDefines[i + 5] = agEgoDir[i];
          }
          for (i = 0; i <= 4; i++)
          {
            tmpDefines[i + 14] = agVideoMode[i];
          }
          for (i = 0; i <= 8; i++)
          {
            tmpDefines[i + 19] = agCompType[i];
          }
          for (i = 0; i <= 15; i++)
          {
            tmpDefines[i + 28] = agResColor[i];
          }

          tmpDefines[44].Name = agResDef[4].Name;
          //if a game is not loaded
          if (!agGameLoaded)
          {
            tmpDefines[44].Value = "0";
          }
          else
          {
            tmpDefines[44].Value = agInvObj.Count.ToString();
          }
          break;
        case atVar:
          //return all variable reserved defines
          tmpDefines = new TDefine[26];
          tmpDefines = agResVar;
          break;
        case atFlag:
          //return all flag reserved defines
          tmpDefines = new TDefine[17];
          tmpDefines = agResFlag;
          break;
        case atMsg:
          //none
          tmpDefines = new TDefine[0];
          break;
        case atSObj:
          //return all screen object reserved defines
          tmpDefines = new TDefine[0];
          tmpDefines[0].Name = agResDef[0].Name;
          tmpDefines[0].Value = agResDef[0].Value;
          tmpDefines[0].Type = atSObj;
          break;
        case atIObj:
          //none
          tmpDefines = new TDefine[0];
          break;
        case atStr:
          //one
          tmpDefines = new TDefine[0];
          tmpDefines[0].Name = agResDef[5].Name;
          tmpDefines[0].Value = agResDef[5].Value;
          tmpDefines[0].Type = atStr;
          break;
        case atWord:
          //none
          tmpDefines = new TDefine[0];
          break;
        case atCtrl:
          //none
          tmpDefines = new TDefine[0];
          break;
        case atDefStr:
          //return all reserved string defines
          tmpDefines = new TDefine[2];
          tmpDefines[0].Name = agResDef[1].Name;
          tmpDefines[0].Value = "\"" + GameVersion + "\"";
          tmpDefines[0].Type = atDefStr;
          tmpDefines[1].Name = agResDef[2].Name;
          tmpDefines[1].Value = "\"" + GameAbout + "\"";
          tmpDefines[1].Type = atDefStr;
          tmpDefines[2].Name = agResDef[3].Name;
          if (agGameLoaded)
          {
            tmpDefines[2].Value = "\"" + GameID + "\"";
          }
          else
          {
            tmpDefines[2].Value = "\"\"";
          }
          tmpDefines[2].Type = atDefStr;
          break;
        case atVocWrd:
          //none
          tmpDefines = new TDefine[0];
          break;
      }
      //return the defines
      return tmpDefines;
    }
    public TDefine[] ResDefByGrp(int Group)
    {
      //this returns the reserved defines by their //group// instead by by variable type
      switch (Group)
      {
        case 1: //var
          return agResVar;
        case 2: //flag
          return agResFlag;
        case 3: //edgecodes
          return agEdgeCodes;
        case 4: //direction
          return agEgoDir;
        case 5: //vidmode
          return agVideoMode;
        case 6: //comp type
          return agCompType;
        case 7: //colors
          return agResColor;
        case 8: //other
          return agResDef;
        default:
          //raise error
          throw new IndexOutOfRangeException("bad form");
      }
    }
    public void ResDef(int DefType, int DefIndex, string DefName)
    {
      //this property lets user update a reserved define name;
      //it is up to calling procedure to make sure there are no conflicts
      //if the define value doesn't match an actual reserved item, error is raised

      //type is a numeric value that maps to the six different types(catgories) of reserved defines
      switch (DefType)
      {
        case 1: //variable
          //value must be 0-26
          if (DefIndex < 0 || DefIndex > 27)
          {
            //raise error
            throw new IndexOutOfRangeException("bad form");
          }
          //change the resvar name
          agResVar[DefIndex].Name = DefName;
          break;
        case 2: //flag
          //value must be 0-17
          if (DefIndex < 0 || DefIndex > 17)
          {
            //raise error
            throw new IndexOutOfRangeException("bad form");
          }
          //change the resflag name
          agResFlag[DefIndex].Name = DefName;
          break;
        case 3: //edgecode
          //value must be 0-4
          if (DefIndex < 0 || DefIndex > 4)
          {
            //raise error
            throw new IndexOutOfRangeException("bad form");
          }
          //change the edgecode name
          agEdgeCodes[DefIndex].Name = DefName;
          break;
        case 4: //direction
          //value must be 0-8
          if (DefIndex < 0 || DefIndex > 8)
          {
            //raise error
            throw new IndexOutOfRangeException("bad form");
          }
          //change the direction name
          agEgoDir[DefIndex].Name = DefName;
          break;
        case 5: //vidmode
          //value must be 0-4
          if (DefIndex < 0 || DefIndex > 4)
          {
            //raise error
            throw new IndexOutOfRangeException("bad form");
          }
          //change the vidmode name
          agVideoMode[DefIndex].Name = DefName;
          break;
        case 6: //comptypes
          //value must be 0-8
          if (DefIndex < 0 || DefIndex > 8)
          {
            //raise error
            throw new IndexOutOfRangeException("bad form");
          }
          //change the comptype name
          agCompType[DefIndex].Name = DefName;
          break;
        case 7: //color
          //value must be 0-15
          if (DefIndex < 0 || DefIndex > 15)
          {
            //raise error
            throw new IndexOutOfRangeException("bad form");
          }
          //change the color resdef name
          agResColor[DefIndex].Name = DefName;
          break;
        case 8: //other
          //value must be 0-5
          if (DefIndex < 0 || DefIndex > 5)
          {
            //raise error
            throw new IndexOutOfRangeException("bad form");
          }
          //change the other-resdef name
          agResDef[DefIndex].Name = DefName;
          break;
        default:
          //error!
          throw new IndexOutOfRangeException("bad form");
      }
    }
    public void ResetResDefines()
    {
      AssignReservedDefines();
    }
    public bool ReservedAsText { get { return agResAsText; } set { agResAsText = value; } }
    public string SourceExt
    {
      get
      {
        return agSrcExt;
      }
      set
      {
        // must be non-zero length
        if (value.Length == 0)
        {
          throw new Exception("non-blank not allowed");
        }
        //must start with a period
        if (value[0] != '.')
        {
          agSrcExt = "." + value;
        }
        else
        {
          agSrcExt = value;
        }
      }
    }
    public bool UseReservedNames
    {
      //if true, predefined variables and flags are used during compilation
      get
      {
        return agUseRes;
      }
      set
      {
        agUseRes = value;
        if (agGameLoaded)
        {
          WriteGameSetting("General", "UseResNames", agUseRes.ToString());
        }

      }
    }
    public bool SpecialSyntax
    {
      get
      {
        return agSpecialSyntax;
      }
      set
      {
        agSpecialSyntax = value;
      }
    }
    public bool ElseAsGoto
    {
      get
      {
        return agElseAsGoto;
      }
      set
      {
        agElseAsGoto = value;
      }
    }
    public bool ShowAllMessages
    {
      get
      {
        return agShowMsgs;
      }
      set
      {
        agShowMsgs = value;
      }
    }
    public bool ValidateResDefs()
    {
      //makes sure reserved defines are OK- replace any bad defines with their defaults
      // if all are OK returns true; if one or more are bad, returns false

      // assume OK
      bool retval = true;
      int i;
      //step through all vars
      for (i = 0; i < agResVar.Length; i++)
      {
        if (!ValidateName(agResVar[i]))
        {
          agResVar[i].Name = agResVar[i].Default;
          retval = false;
        }
      }

      //step through all flags
      for (i = 0; i < agResFlag.Length; i++)
      {
        if (!ValidateName(agResFlag[i]))
        {
          agResFlag[i].Name = agResFlag[i].Default;
          retval = false;
        }
      }

      //step through all edgecodes
      for (i = 0; i < agEdgeCodes.Length; i++)
      {
        if (!ValidateName(agEdgeCodes[i]))
        {
          agEdgeCodes[i].Name = agEdgeCodes[i].Default;
          retval = false;
        }
      }

      //step through all directions
      for (i = 0; i < agEgoDir.Length; i++)
      {
        if (!ValidateName(agEgoDir[i]))
        {
          agEgoDir[i].Name = agEgoDir[i].Default;
          retval = false;
        }
      }

      //step through all vidmodes
      for (i = 0; i < agVideoMode.Length; i++)
      {
        if (!ValidateName(agVideoMode[i]))
        {
          agVideoMode[i].Name = agVideoMode[i].Default;
          retval = false;
        }
      }

      //step through all comp types
      for (i = 0; i < agCompType.Length; i++)
      {
        if (!ValidateName(agCompType[i]))
        {
          agCompType[i].Name = agCompType[i].Default;
          retval = false;
        }
      }

      //step through all colors
      for (i = 0; i < agResColor.Length; i++)
      {
        if (!ValidateName(agResColor[i]))
        {
          agResColor[i].Name = agResColor[i].Default;
          retval = false;
        }
      }

      //step through all other defines
      for (i = 0; i < agResDef.Length; i++)
      {
        if (!ValidateName(agResDef[i]))
        {
          agResDef[i].Name = agResDef[i].Default;
          retval = false;
        }
      }
      // ok; return true
      return true;
    }
    internal bool ValidateName(TDefine TestDef)
    {
      //validates if a reserved define name is agreeable or not
      //returns TRUE if ok, FALSE if not
      int i;
      TDefine[] tmpDefines;
      //get name to test
      string NewDefName = TestDef.Name;
      //if already at default, just exit
      if (TestDef.Name == TestDef.Default)
      {
        return true;
      }
      //if no name,
      if (NewDefName.Length == 0)
      {
        return false;
      }
      //name cant be numeric
      if (IsNumeric(NewDefName))
      {
        return false;
      }
      //check against regular commands
      for (i = 0; i < agCmds.Length; i++)
      {
        if (NewDefName.Equals(agCmds[i].Name, StringComparison.OrdinalIgnoreCase))
        {
          return false;
        }
      }
      //check against test commands
      for (i = 0; i < agTestCmds.Length; i++)
      {
        if (NewDefName.Equals(agTestCmds[i].Name, StringComparison.OrdinalIgnoreCase))
        {
          return false;
        }
      }
      //check against keywords
      if (NewDefName.Equals("if", StringComparison.OrdinalIgnoreCase) || NewDefName.Equals("else", StringComparison.OrdinalIgnoreCase) || NewDefName.Equals("goto", StringComparison.OrdinalIgnoreCase))
      {
        return false;
      }
      // if the name starts with any of these letters
      if ("vfmoiswc".Any(NewDefName.ToLower().StartsWith))
      {
        //if rest of string is numeric
        if (IsNumeric(Right(NewDefName, NewDefName.Length - 1)))
        // can't have a name that's a valid marker
        {
          return false;
        }
      }
      //check against reserved variables (skip if it's this variable)
      tmpDefines = ReservedDefines( ArgTypeEnum.atVar);
      for (i = 0; i < tmpDefines.Length; i++)
      {
        if (tmpDefines[i].Value != TestDef.Value)
        {
          if (NewDefName.Equals(tmpDefines[i].Name, StringComparison.OrdinalIgnoreCase))
          {
            return false;
          }
        }
      }
      //check against reserved flags (skip if it's this flag)
      tmpDefines = ReservedDefines(atFlag);
      for (i = 0; i < tmpDefines.Length; i++)
      {
        if (tmpDefines[i].Value != TestDef.Value)
        {
          if (NewDefName.Equals(tmpDefines[i].Name, StringComparison.OrdinalIgnoreCase))
          {
            return false;
          }
        }
      }
      //check against reserved numbers (skip if it's this number)
      tmpDefines = ReservedDefines(atNum);
      for (i = 0; i < tmpDefines.Length; i++)
      {
        if (tmpDefines[i].Value != TestDef.Value)
        {
          if (NewDefName.Equals(tmpDefines[i].Name, StringComparison.OrdinalIgnoreCase))
          {
            return false;
          }
        }
      }
      tmpDefines = ReservedDefines(atSObj);
      for (i = 0; i < tmpDefines.Length; i++)
      {
        if (tmpDefines[i].Value != TestDef.Value)
        {
          if (NewDefName.Equals(tmpDefines[i].Name, StringComparison.OrdinalIgnoreCase))
          {
            return false;
          }
        }
      }
      //check against reserved strings (skip if it's this string)
      tmpDefines = ReservedDefines(atDefStr);
      for (i = 0; i < tmpDefines.Length; i++)
      {
        if (tmpDefines[i].Value != TestDef.Value)
        {
          if (NewDefName.Equals(tmpDefines[i].Name, StringComparison.OrdinalIgnoreCase))
          {
            return false;
          }
        }
      }
      //check name against improper character list
      for (i = 1; i < NewDefName.Length; i++)
      {
        if ((CTRL_CHARS + " !\"#$%&'()*+,-/:;<=>?@[\\]^`{|}~").Any(NewDefName.Contains))
        {
          // bad
          return false;
        }
      }
      //must be OK!
      return true;
    }
  }
}