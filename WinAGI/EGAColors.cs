using System;
using System.Drawing;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// Represents the sixteen EGA colors used in AGI as ARGB equivalent Color
    /// values. 
    /// </summary>
    [Serializable]
    public class EGAColors {
        #region Local Members
        Color[] colorList = new Color[16];
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the EGA colors to the AGI default values.
        /// </summary>
        public EGAColors() {
            // reset colors to default Sierra AGI values.
            colorList[0] = Color.FromArgb(0, 0, 0);           // 000000 = black
            colorList[1] = Color.FromArgb(0, 0, 0xAA);        // 0000AA = blue
            colorList[2] = Color.FromArgb(0, 0xAA, 0);        // 00AA00 = green
            colorList[3] = Color.FromArgb(0, 0xAA, 0xAA);     // 00AAAA = cyan
            colorList[4] = Color.FromArgb(0xAA, 0, 0);        // AA0000 = red
            colorList[5] = Color.FromArgb(0xAA, 0, 0xAA);     // AA00AA = magenta
            colorList[6] = Color.FromArgb(0xAA, 0x55, 0);     // AA5500 = brown
            colorList[7] = Color.FromArgb(0xAA, 0xAA, 0xAA);  // AAAAAA = light gray
            colorList[8] = Color.FromArgb(0x55, 0x55, 0x55);  // 555555 = dark gray
            colorList[9] = Color.FromArgb(0x55, 0x55, 0xFF);  // 5555FF = light blue
            colorList[10] = Color.FromArgb(0, 0xFF, 0x55);    // 00FF55 = light green
            colorList[11] = Color.FromArgb(0x55, 0xFF, 0xFF); // 55FFFF = light cyan
            colorList[12] = Color.FromArgb(0xFF, 0x55, 0x55); // FF5555 = light red
            colorList[13] = Color.FromArgb(0xFF, 0x55, 0xFF); // FF55FF = light magenta
            colorList[14] = Color.FromArgb(0xFF, 0xFF, 0x55); // FFFF55 = yellow
            colorList[15] = Color.FromArgb(0xFF, 0xFF, 0xFF); // FFFFFF = white
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the color for this EGA color index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Color this[int index] {
            get {
                if (index < 0 || index > 15) {
                    throw new IndexOutOfRangeException("bad color");
                }
                return colorList[index];
            }
            set {
                if (index < 0 || index > 15) {
                    throw new IndexOutOfRangeException("bad color");
                }
                colorList[index] = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts the color value of the specified color to a comma delimited
        /// string of its rgb components as hex values.
        /// </summary>
        /// <param name="aColor"></param>
        /// <returns></returns>
        public static string ColorText(Color aColor) {
            // Converts a color into a useful string value for storing in configuration files.
            return "0x" + aColor.R.ToString("x2") + ", 0x" + aColor.G.ToString("x2") + ", 0x" + aColor.B.ToString("x2");
        }

        /// <summary>
        /// Converts the EGAColor for this index to a comma separated
        /// string of hex values for the four color components.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public string ColorText(int index) {
            if (index < 0 || index > 15) {
                throw new IndexOutOfRangeException("bad color");
            }
            return ColorText(colorList[index]);
        }

        /// <summary>
        /// Converts the EGAColor for this index to a comma separated
        /// string of hex values for the four color components.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public string ColorText(AGIColorIndex index) {
            if (index < 0 || index > AGIColorIndex.White) {
                throw new IndexOutOfRangeException("bad color");
            }
            return ColorText(colorList[(int)index]);
        }

        /// <summary>
        /// Copies the source palette to an identical new palette.
        /// </summary>
        /// <param name="palette"></param>
        /// <returns></returns>
        public EGAColors Clone() {
            EGAColors retval = new();
            for (int i = 0; i < 16; i++) {
                retval[i] = colorList[i];
            }
            return retval;
        }
        #endregion
    }
}
