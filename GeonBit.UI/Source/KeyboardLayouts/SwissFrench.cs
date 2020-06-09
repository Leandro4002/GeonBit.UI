using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

// all keyboard layouts can be found here https://docs.microsoft.com/en-us/globalization/windows-keyboard-layouts
namespace GeonBit.KeyboardLayouts
{
    /// <summary>
    /// swiss-french layout here: https://docs.microsoft.com/fr-fr/globalization/keyboards/kbdsf_2.html
    /// </summary>
    public class SwissFrench : KeyboardLayout
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

        /// <summary>
        /// "Normal" dead keys combinaisons (by "normal", we mean key pressed without "alt gr" or "shift" key).
        /// </summary>S
        public override Keys[] NormalDeadKeys { get { return normalDeadKeys; } }

        /// <summary>
        /// "Shift" dead key combinaisons.
        /// </summary>
        public override Keys[] ShiftDeadKeys { get { return shiftDeadKeys; } }

        /// <summary>
        /// "Alt Gr" dead key combinaisons.
        /// </summary>
        public override Keys[] AltGrDeadKeys { get { return altGrDeadKeys; } }

        // normal keys
        readonly Dictionary<Keys, char> normalKeys = new Dictionary<Keys, char>()
        {
            { Keys.OemOpenBrackets,  '\u0027'},
            { Keys.OemCloseBrackets, '\u005e'},
            { Keys.OemTilde,         '\u00a8'},
            { Keys.OemPipe,          '\u00e0'},
            { Keys.Oem8,             '\u0024'},
            { Keys.OemSemicolon,     '\u00e8'},
            { Keys.OemQuotes,        '\u00e9'},
            { Keys.OemBackslash,     '\u003c'},
            { Keys.OemQuestion,      '\u00a7'},
            { Keys.OemComma,         '\u002c'},
            { Keys.OemPeriod,        '\u002e'},
            { Keys.OemMinus,         '\u002d'},
        }
        // add some typical normal keys (letters and numbers)
        .Concat(typicalNormalKeys).ToDictionary(x => x.Key, x => x.Value);

        // shift keys
        readonly Dictionary<Keys, char> shiftKeys = new Dictionary<Keys, char>()
        {
            { Keys.D1,               '\u002b'},
            { Keys.D2,               '\u0022'},
            { Keys.D3,               '\u002a'},
            { Keys.D4,               '\u00e7'},
            { Keys.D5,               '\u0025'},
            { Keys.D6,               '\u0026'},
            { Keys.D7,               '\u002f'},
            { Keys.D8,               '\u0028'},
            { Keys.D9,               '\u0029'},
            { Keys.D0,               '\u003d'},
            { Keys.OemOpenBrackets,  '\u003f'},
            { Keys.OemCloseBrackets, '\u0060'},
            { Keys.Oem8,             '\u00a3'},
            { Keys.OemTilde,         '\u0021'},
            { Keys.OemSemicolon,     '\u00fc'},
            { Keys.OemPipe,          '\u00e4'},
            { Keys.OemQuotes,        '\u00f6'},
            { Keys.OemBackslash,     '\u003e'},
            { Keys.OemComma,         '\u003b'},
            { Keys.OemPeriod,        '\u003a'},
            { Keys.OemMinus,         '\u005f'},
            { Keys.OemQuestion,      '\u00b0'},
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

        readonly Keys[] normalDeadKeys = new Keys[] {
            Keys.OemCloseBrackets, // circumflex
            Keys.OemTilde,         // diaresis
        };

        readonly Keys[] shiftDeadKeys = new Keys[] {
            Keys.OemCloseBrackets, // grave accent
        };

        readonly Keys[] altGrDeadKeys = new Keys[] {
            Keys.OemCloseBrackets, // tilde
            Keys.OemOpenBrackets,  // acute accent
        };
    }
}
