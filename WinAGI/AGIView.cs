using System;
using System.ComponentModel;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.AGILogicSourceSettings;
using static WinAGI.AGICommands;

namespace WinAGI
{
  public class AGIView : AGIResource
  {
    public AGIView() : base(AGIResType.rtView, "NewView")
    {
      //initialize
      //attach events
      base.PropertyChanged += ResPropChange;
      strErrSource = "WinAGI.View";
      //set default resource data
      Data = new RData(0);// ();
    }
    internal void InGameInit(byte ResNum, sbyte VOL, int Loc)
    {
      //this internal function adds this resource to a game, setting its resource 
      //location properties, and reads properties from the wag file

      //set up base resource
      base.InitInGame(ResNum, VOL, Loc);

      //if first time loading this game, there will be nothing in the propertyfile
      ID = ReadSettingString(agGameProps, "View" + ResNum, "ID", "");
      if (ID.Length == 0)
      {
        //no properties to load; save default ID
        ID = "View" + ResNum;
        WriteGameSetting("Logic" + ResNum, "ID", ID, "Views");
        //load resource to get size
        Load();
        WriteGameSetting("View" + ResNum, "Size", Size.ToString());
        Unload();
      }
      else
      {
        //get description, size and other properties from wag file
        mDescription = ReadSettingString(agGameProps, "View" + ResNum, "Description", "");
        Size = ReadSettingLong(agGameProps, "View" + ResNum, "Size", -1);
      }
    }
    private void ResPropChange(object sender, AGIResPropChangedEventArgs e)
    {
      ////let's do a test
      //// increment number everytime data changes
      //Number++;
    }
    internal void SetView(AGIView newView)
    {
      throw new NotImplementedException();
    }
  }
}