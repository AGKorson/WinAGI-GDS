using System;
using System.ComponentModel;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.AGILogicSourceSettings;
using static WinAGI.AGICommands;

namespace WinAGI
{
  public class AGISound : AGIResource
  {
    public AGISound() : base(AGIResType.rtSound, "NewSound")
    {
      //initialize
      //attach events
      base.PropertyChanged += ResPropChange;
      strErrSource = "WinAGI.Sound";
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
      ID = ReadSettingString(agGameProps, "Sound" + ResNum, "ID", "");
      if (ID.Length == 0)
      {
        //no properties to load; save default ID
        ID = "Sound" + ResNum;
        WriteGameSetting("Logic" + ResNum, "ID", ID, "Sounds");
        //load resource to get size
        Load();
        WriteGameSetting("Sound" + ResNum, "Size", Size.ToString());
        Unload();
      }
      else
      {
        //get description, size and other properties from wag file
        mDescription = ReadSettingString(agGameProps, "Sound" + ResNum, "Description", "");
        Size = ReadSettingLong(agGameProps, "Sound" + ResNum, "Size", -1);
      }
    }
    private void ResPropChange(object sender, AGIResPropChangedEventArgs e)
    {
      ////let's do a test
      //// increment number everytime data changes
      //Number += 1;
    }
    internal void SetSound(AGISound newSound)
    {
      throw new NotImplementedException();
    }
  }
}