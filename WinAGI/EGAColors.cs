using System;
using System.Drawing;

namespace WinAGI.Engine
{
  public class EGAColors
  {
    Color[] colorEGA = new Color[16];
    public Color this[int index]
    {
      get
      {
        if (index < 0 || index > 15) {
          throw new IndexOutOfRangeException("bad color");
        }
        return colorEGA[index];
      }
      set
      {
        if (index < 0 || index > 15) {
          throw new IndexOutOfRangeException("bad color");
        }
        colorEGA[index] = value;
        //// if color for a game is changed, how to let the game object know???
        //if (agGameLoaded) {
        //  WriteGameSetting("Palette", "Color" + index, ColorText(index));
        //}
      }
    }
    public static string ColorText(Color aColor)
    {
      // converts aColor into a useful string value for storing in config files
      return "0x" + aColor.R.ToString("x2") + ", 0x" + aColor.G.ToString("x2") + ", 0x" + aColor.B.ToString("x2");
    }
    public string ColorText(int index)
    {
      // converts the egacolor for this index to string value
      if (index < 0 || index > 15) {
        throw new IndexOutOfRangeException("bad color");
      }
      return ColorText(colorEGA[index]);
    }
  }
}
