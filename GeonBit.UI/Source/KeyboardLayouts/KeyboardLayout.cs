using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

// all keyboard layouts can be found here https://docs.microsoft.com/en-us/globalization/windows-keyboard-layouts
namespace GeonBit.KeyboardLayouts
{
    /// <summary>
    /// Base class for all keyboard layouts.
    /// </summary>
    public abstract class KeyboardLayout
    {
        /// <summary>
        /// List of supported accents and their correspondant unicode "combine" character.
        /// "Combine" characters are the same but they are meant to be used as dead keys.
        /// Here is the list of unicode combining diacritical marks: http://www.fileformat.info/info/unicode/block/combining_diacritical_marks/list.htm
        /// </summary>
        static readonly Dictionary<char, char> supportedAccents = new Dictionary<char, char>
        {
            { '`', '\u0300' },
            { '´', '\u0301' },
            { '^', '\u0302' },
            { '~', '\u0303' },
            { '¨', '\u0308' },
        };

        /// <summary>
        /// Get a letter without accent and a accent and combine them.
        /// For example: a+^=â, e+`=è
        /// </summary>
        /// <param name="letterChar">The letter character.</param>
        /// <param name="accentChar">The accent character.</param>
        /// <returns>The combined char. If invalid char is provided, returns letterChar param</returns>
        public static char CreateAccentChar(char letterChar, char accentChar)
        {
            // validate letter character
            if (!((letterChar >= 'a' && letterChar <= 'z') || (letterChar >= 'A' && letterChar <= 'Z'))) return letterChar;

            // validate accent character
            if (!supportedAccents.ContainsKey(accentChar)) return letterChar;

            // combine letter and accent
            string combinedLetters = "" + letterChar + supportedAccents[accentChar];
            combinedLetters = combinedLetters.Normalize(System.Text.NormalizationForm.FormC);

            // returns the last char of the combined letters. If the combinaisons is possible (ex. "À"), it returns the right char,
            // if the combinaision is not possible (ex. "Z`"), it returns the accent char.
            return combinedLetters[0];
        }


        /// <summary>
        /// Those "normal" keys are the most common (letters and numbers).
        /// (By "normal", we mean key pressed without "alt gr" or "shift" key).
        /// </summary>
        protected static readonly Dictionary<Keys, char> typicalNormalKeys = new Dictionary<Keys, char>()
        {
            { Keys.A,       '\u0061'}, { Keys.B,       '\u0062'}, { Keys.C,       '\u0063'}, { Keys.D,       '\u0064'},
            { Keys.E,       '\u0065'}, { Keys.F,       '\u0066'}, { Keys.G,       '\u0067'}, { Keys.H,       '\u0068'},
            { Keys.I,       '\u0069'}, { Keys.J,       '\u006a'}, { Keys.K,       '\u006b'}, { Keys.L,       '\u006c'},
            { Keys.M,       '\u006d'}, { Keys.N,       '\u006e'}, { Keys.O,       '\u006f'}, { Keys.P,       '\u0070'},
            { Keys.Q,       '\u0071'}, { Keys.R,       '\u0072'}, { Keys.S,       '\u0073'}, { Keys.T,       '\u0074'},
            { Keys.U,       '\u0075'}, { Keys.V,       '\u0076'}, { Keys.W,       '\u0077'}, { Keys.X,       '\u0078'},
            { Keys.Y,       '\u0079'}, { Keys.Z,       '\u007a'},

            { Keys.D0,      '\u0030'}, { Keys.D1,      '\u0031'}, { Keys.D2,      '\u0032'}, { Keys.D3,      '\u0033'},
            { Keys.D4,      '\u0034'}, { Keys.D5,      '\u0035'}, { Keys.D6,      '\u0036'}, { Keys.D7,      '\u0037'},
            { Keys.D8,      '\u0038'}, { Keys.D9,      '\u0039'},

            { Keys.NumPad0, '\u0030'}, { Keys.NumPad1, '\u0031'}, { Keys.NumPad2, '\u0032'}, { Keys.NumPad3, '\u0033'},
            { Keys.NumPad4, '\u0034'}, { Keys.NumPad5, '\u0035'}, { Keys.NumPad6, '\u0036'}, { Keys.NumPad7, '\u0037'},
            { Keys.NumPad8, '\u0038'}, { Keys.NumPad9, '\u0039'},

            { Keys.Divide, '\u002f'}, { Keys.Multiply, '\u002a'}, { Keys.Add, '\u002b'}, { Keys.Subtract, '\u002d'}, { Keys.Decimal, '\u002e'}
        };


        /// <summary>
        /// Uppercase letters combinaisons.
        /// </summary>
        protected static readonly Dictionary<Keys, char> uppercaseLetters = new Dictionary<Keys, char>()
        {
            { Keys.A, '\u0041'}, { Keys.B, '\u0042'}, { Keys.C, '\u0043'}, { Keys.D, '\u0044'},
            { Keys.E, '\u0045'}, { Keys.F, '\u0046'}, { Keys.G, '\u0047'}, { Keys.H, '\u0048'},
            { Keys.I, '\u0049'}, { Keys.J, '\u004a'}, { Keys.K, '\u004b'}, { Keys.L, '\u004c'},
            { Keys.M, '\u004d'}, { Keys.N, '\u004e'}, { Keys.O, '\u004f'}, { Keys.P, '\u0050'},
            { Keys.Q, '\u0051'}, { Keys.R, '\u0052'}, { Keys.S, '\u0053'}, { Keys.T, '\u0054'},
            { Keys.U, '\u0055'}, { Keys.V, '\u0056'}, { Keys.W, '\u0057'}, { Keys.X, '\u0058'},
            { Keys.Y, '\u0059'}, { Keys.Z, '\u005a'},
        };

        /// <summary>
        /// "Normal" keys combinaisons (by "normal", we mean key pressed without "alt gr" or "shift" key).
        /// </summary>S
        public abstract Dictionary<Keys, char> NormalKeys { get; }

        /// <summary>
        /// "Shift" key combinaisons.
        /// </summary>
        public abstract Dictionary<Keys, char> ShiftKeys { get; }

        /// <summary>
        /// "Alt Gr" key combinaisons.
        /// </summary>
        public abstract Dictionary<Keys, char> AltGrKeys { get; }

        /// <summary>
        /// "Normal" dead keys combinaisons (by "normal", we mean key pressed without "alt gr" or "shift" key).
        /// </summary>S
        public virtual Keys[] NormalDeadKeys { get; }

        /// <summary>
        /// "Shift" dead key combinaisons.
        /// </summary>
        public virtual Keys[] ShiftDeadKeys { get; }

        /// <summary>
        /// "Alt Gr" dead key combinaisons.
        /// </summary>
        public virtual Keys[] AltGrDeadKeys { get; }
    }
}
