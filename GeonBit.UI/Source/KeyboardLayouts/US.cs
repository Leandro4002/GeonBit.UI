using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

// all keyboard layouts can be found here https://docs.microsoft.com/en-us/globalization/windows-keyboard-layouts
namespace GeonBit.KeyboardLayouts
{
    // TODO this layout could be wrong because the author doesn't have a US layout to test it correctly
    // author: LeandroSaraivaMaia (github user)

    /// <summary>
    /// US layout here: https://docs.microsoft.com/fr-fr/globalization/keyboards/kbdus_7.html
    /// </summary>
    public class US : KeyboardLayout
    {
        /// <summary>
        /// "Normal" keys combinaisons (by "normal", we mean key pressed without "alt gr" or "shift" key).
        /// </summary>
        public override Dictionary<Keys, char> NormalKeys { get { return normalKeys; } }

        /// <summary>
        /// "Shift" key combinaisons.
        /// </summary>
        public override Dictionary<Keys, char> ShiftKeys { get { return shiftKeys; } }

        /// <summary>
        /// "Alt Gr" key combinaisons.
        /// </summary>
        public override Dictionary<Keys, char> AltGrKeys { get { return altGrKeys; } }

        // normal keys
        readonly Dictionary<Keys, char> normalKeys = new Dictionary<Keys, char>()
        {
            { Keys.OemOpenBrackets,  '\u005b'},
            { Keys.OemCloseBrackets, '\u005d'},
            { Keys.OemTilde,         '\u0060'},
            { Keys.OemSemicolon,     '\u003b'},
            { Keys.OemQuotes,        '\u0027'},
            { Keys.OemBackslash,     '\u005c'},
            { Keys.OemComma,         '\u002c'},
            { Keys.OemPeriod,        '\u002e'},
            { Keys.OemMinus,         '\u002d'},
            { Keys.OemQuestion,      '\u002f'},
            { Keys.OemPlus,          '\u003d'},
        }
        // add some typical normal keys (letters and numbers)
        .Concat(typicalNormalKeys).ToDictionary(x => x.Key, x => x.Value);

        // shift keys
        readonly Dictionary<Keys, char> shiftKeys = new Dictionary<Keys, char>()
        {
            { Keys.D1,               '\u0021'},
            { Keys.D2,               '\u0040'},
            { Keys.D3,               '\u0023'},
            { Keys.D4,               '\u0024'},
            { Keys.D5,               '\u0025'},
            { Keys.D6,               '\u005e'},
            { Keys.D7,               '\u0026'},
            { Keys.D8,               '\u002a'},
            { Keys.D9,               '\u0028'},
            { Keys.D0,               '\u0029'},
            { Keys.OemOpenBrackets,  '\u007b'},
            { Keys.OemCloseBrackets, '\u007d'},
            { Keys.OemTilde,         '\u007e'},
            { Keys.OemSemicolon,     '\u003a'},
            { Keys.OemQuotes,        '\u0022'},
            { Keys.OemBackslash,     '\u007c'},
            { Keys.OemComma,         '\u003c'},
            { Keys.OemPeriod,        '\u003e'},
            { Keys.OemMinus,         '\u005f'},
            { Keys.OemQuestion,      '\u003f'},
            { Keys.OemPlus,          '\u002b'},
        }
        // add some typical shift keys (uppercase letters)
        .Concat(uppercaseLetters).ToDictionary(x => x.Key, x => x.Value);

        // alt keys
        readonly Dictionary<Keys, char> altGrKeys = new Dictionary<Keys, char>()
        {
            { Keys.D1,               '\u00a6'},
            { Keys.D2,               '\u0040'},
            { Keys.D3,               '\u0023'},
            { Keys.D4,               '\u00b0'},
            { Keys.D5,               '\u00a7'},
            { Keys.D6,               '\u00ac'},
            { Keys.D7,               '\u007c'},
            { Keys.D8,               '\u00a2'},
            { Keys.E,                '\u20ac'},
            { Keys.OemOpenBrackets,  '\u00b4'},
            { Keys.OemCloseBrackets, '\u007e'},
            { Keys.Oem8,             '\u007d'},
            { Keys.OemTilde,         '\u005d'},
            { Keys.OemSemicolon,     '\u005b'},
            { Keys.OemPipe,          '\u007b'},
            { Keys.OemBackslash,     '\u005c'},
        };
    }
}
