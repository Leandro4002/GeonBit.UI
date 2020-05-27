#region File Description
//-----------------------------------------------------------------------------
// TextInput are entities that allow the user to type in free text using the keyboard.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using GeonBit.UI.Entities.TextValidators;
using System;

namespace GeonBit.UI.Entities
{

    /// <summary>
    /// A textbox that allow users to put in free text.
    /// </summary>
    [System.Serializable]
    public class TextInput : PanelBase
    {
        /// <summary>
        /// Static ctor.
        /// </summary>
        static TextInput()
        {
            Entity.MakeSerializable(typeof(TextInput));
        }

        // current text value
        string _value = string.Empty;

        // current caret position (-1 is last character).
        int _caret = -1;

        // width of the carret (in pixels)
        const int CARET_WIDTH = 2;

        /// <summary>The Paragraph object showing current text value.</summary>
        public Paragraph TextParagraph;

        /// <summary>A placeholder paragraph to show when text input is empty.</summary>
        public Paragraph PlaceholderParagraph;

        /// <summary>If false, it will only allow one line input.</summary>
        protected bool _multiLine = false;

        /// <summary>
        /// Set / get multiline mode.
        /// </summary>
        public bool Multiline
        {
            get { return _multiLine; }
            set { if (_multiLine != value) { _multiLine = value; UpdateMultilineState(); } }
        }

        // scrollbar to use if text height exceed the input box size
        VerticalScrollbar _scrollbar;

        /// <summary>
        /// If provided, will automatically put this value whenever the user leave the input box and its empty.
        /// </summary>
        public string ValueWhenEmpty = null;

        // current caret animation step
        float _caretAnim = 0f;

        /// <summary>
        /// Get the actual visibility of the caret.
        /// </summary>
        /// <returns>True or false if the carret is visible.</returns>
        public bool IsCaretCurrentlyVisible {
            get
            {
                if (!IsFocused) {
                    return false;
                } else {
                    return _caretAnim < 0 || (int)_caretAnim % 2 == 0;
                }
            }
        }

        /// <summary>
        /// When the position of the carret change, this is the number of seconds to wait before starting to blink again.
        /// It allows the user to see more easily where the caret is.
        /// </summary>
        public static float caretBlinkAfterMoveDelay = 2f;

        /// <summary>Text to show when there's no input. Note that this text will be drawn with PlaceholderParagraph, and not TextParagraph.</summary>
        string _placeholderText = string.Empty;

        /// <summary>Set to any number to limit input by characters count.
        /// Note: this will only take effect when user insert input, if you set value programmatically it will be ignored.</summary>
        public int CharactersLimit = 0;

        /// <summary>If true, will limit max input length to fit textbox size.
        /// Note: this will only take effect when user insert input, if you set value programmatically it will be ignored.</summary>
        public bool LimitBySize = false;

        /// <summary>
        /// If provided, hide input and replace it with the given character.
        /// This is useful for stuff like password input field.
        /// </summary>
        public char? HideInputWithChar;

        /// <summary>Default styling for the text input itself. Note: loaded from UI theme xml file.</summary>
        new public static StyleSheet DefaultStyle = new StyleSheet();

        /// <summary>Default style for paragraph that show current value.</summary>
        public static StyleSheet DefaultParagraphStyle = new StyleSheet();

        /// <summary>Default style for the placeholder paragraph.</summary>
        public static StyleSheet DefaultPlaceholderStyle = new StyleSheet();

        /// <summary>How fast to blink caret when text input is selected.</summary>
        public static float CaretBlinkingSpeed = 2f;

        /// <summary>The actual displayed text, after wordwrap and other processing. 
        /// note: only the text currently visible by scrollbar.</summary>
        string _actualDisplayText = string.Empty;

        /// <summary>List of validators to apply on text input.</summary>
        public List<ITextValidator> Validators = new List<ITextValidator>();

        /// <summary>
        /// Create the text input.
        /// </summary>
        /// <param name="multiline">If true, text input will accept multiple lines.</param>
        /// <param name="size">Input box size.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        /// <param name="skin">TextInput skin, eg which texture to use.</param>
        public TextInput(bool multiline, Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null, PanelSkin skin = PanelSkin.ListBackground) :
            base(size, skin, anchor, offset)
        {
            // set multiline mode
            _multiLine = multiline;

            // update default style
            UpdateStyle(DefaultStyle);
            
            // default size of multiline text input is 4 times bigger
            if (multiline)
            {
                SetStyleProperty(StylePropertyIds.DefaultSize, new DataTypes.StyleProperty(EntityDefaultSize * new Vector2(1, 4)));
            }

            // set limit by size - default true in single-line, default false in multi-line
            LimitBySize = !_multiLine;

            // set default mosue event to change cursor when hovering text inputs (show IBeam)
            OnMouseEnter = (Entity entity) => { UserInterface.Active.SetCursor(CursorType.IBeam); };
            OnMouseLeave = (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Default); };

            if (!UserInterface.Active._isDeserializing)
            {

                // create paragraph to show current value
                TextParagraph = UserInterface.DefaultParagraph(string.Empty, Anchor.TopLeft);
                TextParagraph.UpdateStyle(DefaultParagraphStyle);
                TextParagraph._hiddenInternalEntity = true;
                TextParagraph.Identifier = "_TextParagraph";
                AddChild(TextParagraph, true);

                // create the placeholder paragraph
                PlaceholderParagraph = UserInterface.DefaultParagraph(string.Empty, Anchor.TopLeft);
                PlaceholderParagraph.UpdateStyle(DefaultPlaceholderStyle);
                PlaceholderParagraph._hiddenInternalEntity = true;
                PlaceholderParagraph.Identifier = "_PlaceholderParagraph";
                AddChild(PlaceholderParagraph, true);

                // update multiline related stuff
                UpdateMultilineState();

                // if the default paragraph type is multicolor, disable it for input
                RichParagraph colorTextParagraph = TextParagraph as RichParagraph;
                if (colorTextParagraph != null)
                {
                    colorTextParagraph.EnableStyleInstructions = false;
                }
            }
        }

        /// <summary>
        /// Update after multiline state was changed.
        /// </summary>
        private void UpdateMultilineState()
        {
            // we are now multiline
            if (_multiLine)
            {
                _scrollbar = new VerticalScrollbar(0, 0, Anchor.CenterRight, offset: new Vector2(-8, 0));
                _scrollbar.Value = 0;
                _scrollbar.Visible = false;
                _scrollbar._hiddenInternalEntity = true;
                _scrollbar.Identifier = "__inputScrollbar";
                AddChild(_scrollbar, false);
            }
            // we are not multiline
            else
            {
                if (_scrollbar != null)
                {
                    _scrollbar.RemoveFromParent();
                    _scrollbar = null;
                }
            }

            // set default wrap words state
            TextParagraph.WrapWords = _multiLine;
            PlaceholderParagraph.WrapWords = _multiLine;
            TextParagraph.Anchor = PlaceholderParagraph.Anchor = _multiLine ? Anchor.TopLeft : Anchor.CenterLeft;
        }

        /// <summary>
        /// Special init after deserializing entity from file.
        /// </summary>
        internal protected override void InitAfterDeserialize()
        {
            base.InitAfterDeserialize();

            // set main text paragraph
            TextParagraph = Find("_TextParagraph") as Paragraph;
            TextParagraph._hiddenInternalEntity = true;

            // set scrollbar
            _scrollbar = Find<VerticalScrollbar>("__inputScrollbar");
            if (_scrollbar != null)
                _scrollbar._hiddenInternalEntity = true;

            // set placeholder paragraph
            PlaceholderParagraph = Find("_PlaceholderParagraph") as Paragraph;
            PlaceholderParagraph._hiddenInternalEntity = true;

            // recalc dest rects
            UpdateMultilineState();
        }

        /// <summary>
        /// Create the text input with default size.
        /// </summary>
        /// <param name="multiline">If true, text input will accept multiple lines.</param>
        /// <param name="anchor">Position anchor.</param>
        /// <param name="offset">Offset from anchor position.</param>
        public TextInput(bool multiline, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
           this(multiline, USE_DEFAULT_SIZE, anchor, offset)
        {
        }

        /// <summary>
        /// Create default single-line text input.
        /// </summary>
        public TextInput() : this(false)
        {
        }

        /// <summary>
        /// Is the text input a natrually-interactable entity.
        /// </summary>
        /// <returns>True.</returns>
        override internal protected bool IsNaturallyInteractable()
        {
            return true;
        }

        /// <summary>
        /// Text to show when there's no input using the placeholder style.
        /// </summary>
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set { _placeholderText = _multiLine ? value : value.Replace("\n", string.Empty); }
        }

        /// <summary>
        /// Current input text value.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                value = value ?? string.Empty;
                _value = _multiLine ? value : value.Replace("\n", string.Empty);
                FixCaretPosition();
            }
        }

        /// <summary>
        /// Move scrollbar to show caret position.
        /// </summary>
        public void ScrollToCaret()
        {
            // skip if no scrollbar
            if (_scrollbar == null)
            {
                return;
            }

            // make sure caret position is legal
            if (_caret >= _value.Length)
            {
                _caret = -1;
            }

            // if caret is at end of text jump to it
            if (_caret == -1)
            {
                _scrollbar.Value = (int)_scrollbar.Max;
            }
            // if not try to find the right pos
            else
            {
                // get how many lines can fit in the textbox
                int linesFit = _destRectInternal.Height / (int)(System.Math.Max(TextParagraph.GetCharacterActualSize().Y, 1));

                TextParagraph.Text = _value;
                TextParagraph.CalcTextActualRectWithWrap();

                // get caret position
                string[] linesToScan = TextParagraph.GetProcessedText().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                Paragraph.LineType[] processedTextLinesCodes = TextParagraph.ProcessedTextLinesTypes;
                Point caretPosition = CalculateCaretPositionForMultiline(linesToScan, processedTextLinesCodes, _caret);

                // find current line
                Vector2 charSize = TextParagraph.GetCharacterActualSize();
                int currentLine = (int)(caretPosition.Y / charSize.Y);

                // reposition the scrollbar
                // if the carret is before the textInput dest rect
                if (currentLine - _scrollbar.Value < 0)
                {
                    _scrollbar.Value = currentLine;
                }
                // if the carret is after the textInput dest rect
                else if (currentLine - _scrollbar.Value >= linesFit)
                {
                    _scrollbar.Value = currentLine - linesFit + 1;
                }
            }
        }

        /// <summary>
        /// Current cursor, eg where we are about to put next character.
        /// Set to -1 to jump to end.
        /// </summary>
        public int Caret
        {
            get { return _caret; }
            set { _caret = value; }
        }

        /// <summary>
        /// Current scrollbar position.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public int ScrollPosition
        {
            get { return _scrollbar != null ? _scrollbar.Value : 0; }
            set { if (_scrollbar != null) _scrollbar.Value = value; }
        }

        /// <summary>
        /// Move caret to the end of text.
        /// </summary>
        /// <param name="scrollToCaret">If true, will also scroll to show caret position.</param>
        public void ResetCaret(bool scrollToCaret)
        {
            Caret = -1;
            if (scrollToCaret)
            {
                ScrollToCaret();
            }
        }

        /// <summary>
        /// Prepare the input paragraph for display.
        /// </summary>
        /// <param name="usePlaceholder">If true, will use the placeholder text. Else, will use the real input text.</param>
        /// <returns>Processed text that will actually be displayed on screen.</returns>
        protected string PrepareInputTextForDisplay(bool usePlaceholder)
        {
            // set main text when hidden with password char
            if (HideInputWithChar != null)
            {
                TextParagraph.Text = new string(HideInputWithChar.Value, _value.Length);
            }
            // set main text for regular text input
            else
            {
                TextParagraph.Text = _value;
            }

            // update placeholder text
            PlaceholderParagraph.Text = _placeholderText;

            // get current paragraph and prepare to draw
            Paragraph currParagraph = usePlaceholder ? PlaceholderParagraph : TextParagraph;
            TextParagraph.UpdateDestinationRectsIfDirty();

            // get text to display
            return currParagraph.GetProcessedText();
        }

        /// <summary>
        /// Handle mouse click event.
        /// TextInput override this function to handle picking caret position.
        /// </summary>
        override protected void DoOnClick()
        {
            // first call base DoOnClick
            base.DoOnClick();

            // check if hit paragraph
            if (_value.Length > 0)
            {
                Vector2 charSize = TextParagraph.GetCharacterActualSize();

                // get relative position
                Vector2 actualParagraphPos = new Vector2(_destRectInternal.Location.X, _destRectInternal.Location.Y);
                Vector2 relativeOffset = GetMousePos(-actualParagraphPos);

                // offset for half of a character because the caret is between characters
                relativeOffset.X += charSize.X / 2;

                // calc caret position
                int x = (int)(relativeOffset.X / charSize.X);
                _caret = x;

                // if multiline, take line into the formula
                if (_multiLine)
                {
                    // get the whole processed text
                    TextParagraph.Text = _value;
                    TextParagraph.CalcTextActualRectWithWrap();
                    string processedValueText = TextParagraph.GetProcessedText();

                    // calc y position and add scrollbar value to it
                    int y = (int)(relativeOffset.Y / charSize.Y) + _scrollbar.Value;

                    // break actual text into lines
                    List<string> lines = new List<string>(processedValueText.Split('\n'));
                    for (int i = 0; i < y && i < lines.Count; ++i)
                    {
                        _caret += lines[i].Length + 1;
                    }
                }

                // if override text length reset caret
                if (_caret >= _value.Length)
                {
                    _caret = -1;
                }
            }
            // if don't click on the paragraph itself, reset caret position.
            else
            {
                _caret = -1;
            }

            PauseCaretBlink();
        }

        /// <summary>
        /// Calculate the carret position for multiline textInput. 
        /// </summary>
        /// <param name="processedTextLinesCodes">The processed text lines (text with word wrap and word break)</param>
        /// <param name="linesType">Contains the type of each line (normal, word wrap and word break)</param>
        /// <param name="caret">caret position</param>
        /// <returns></returns>
        private Point CalculateCaretPositionForMultiline(string[] processedTextLinesCodes, Paragraph.LineType[] linesType, int caret)
        {
            int oldNumOfCharacter = 0, numOfCharacter = 0;
            int currentLine = 0;
            bool loopBroken = false;
            caret = (caret == -1 ? _value.Length : caret);

            // we iterate trought each line to find the caret position
            foreach (string line in processedTextLinesCodes)
            {
                oldNumOfCharacter = numOfCharacter;

                int currentLineLenght = line.Length;
                switch (linesType[currentLine])
                {
                    case Paragraph.LineType.Normal: break; // nothing happens in this line
                    case Paragraph.LineType.WordWrap: currentLineLenght--; break; // word wraped to next line
                    case Paragraph.LineType.WordBroken: currentLineLenght -= (TextParagraph.AddHyphenWhenBreakWord ? 3 : 2); break; // word broken into pieces
                }

                numOfCharacter += currentLineLenght;

                // when we found in which line the caret is located, we break the loop
                if (caret - currentLine - numOfCharacter <= 0)
                {
                    loopBroken = true;
                    break;
                }
                currentLine++;
            }

            int localCaret = caret - oldNumOfCharacter - currentLine;

            // if we didn't break the loop, it means that the caret is at the last character of text.
            // when it happens, it is because the text finishes with a space, so there is no word wrap/break and the position is incorrect.
            if (!loopBroken) localCaret = 0;

            // recalculate caret position
            Vector2 charSize = TextParagraph.GetCharacterActualSize();
            Point carretPosition = new Point();
            carretPosition.X = (int)(charSize.X * localCaret);
            carretPosition.Y = (int)(currentLine * charSize.Y);
            return carretPosition;
        }

        /// <summary>
        /// Draw the entity.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw on.</param>
        /// <param name="phase">The phase we are currently drawing.</param>
        override protected void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
        {
            // call base draw function to draw the panel part
            base.DrawEntity(spriteBatch, phase);

            // get which paragraph we currently show - real or placeholder
            bool showPlaceholder = !(IsFocused || _value.Length > 0);
            Paragraph currParagraph = showPlaceholder ? PlaceholderParagraph : TextParagraph;

            // get actual processed string
            _actualDisplayText = PrepareInputTextForDisplay(showPlaceholder);

            // init some value for carret calculation
            Vector2 charSize = TextParagraph.GetCharacterActualSize();
            int caretHeight = (int)charSize.Y;
            Rectangle caretDstRect = new Rectangle(0, 0, CARET_WIDTH, caretHeight);

            if (_multiLine)
            {

                // if visible, calculate carret position for multiline TextInput
                if (IsCaretCurrentlyVisible)
                {
                    string[] linesToScan = _actualDisplayText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    Paragraph.LineType[] processedTextLinesCodes = TextParagraph.ProcessedTextLinesTypes;

                    caretDstRect.Location = CalculateCaretPositionForMultiline(linesToScan, processedTextLinesCodes, _caret);
                    caretDstRect.Location += TextParagraph._actualDestRect.Location;
                }

                // at which line the text is starting to be shown
                int scrollBarLineOffset = 0;

                // handle scrollbar visibility and max
                if (_actualDisplayText != null && _destRectInternal.Height > 0)
                {
                    // get how many lines can fit in the textbox and how many lines display text actually have
                    int linesFit = _destRectInternal.Height / (int)(System.Math.Max(currParagraph.GetCharacterActualSize().Y, 1));
                    int linesInText = _actualDisplayText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length;
                    
                    // if there are more lines than can fit, show scrollbar and manage scrolling:
                    if (linesInText > linesFit)
                    {
                        // fix paragraph width to leave room for the scrollbar
                        float prevWidth = currParagraph.Size.X;
                        currParagraph.Size = new Vector2(_destRectInternal.Width / GlobalScale - 20, 0);
                        if (currParagraph.Size.X != prevWidth)
                        {
                            // update size and re-calculate lines in text
                            _actualDisplayText = PrepareInputTextForDisplay(showPlaceholder);
                            linesInText = _actualDisplayText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length;
                        }

                        // set scrollbar max and steps
                        _scrollbar.Max = (uint)System.Math.Max(linesInText - linesFit, 2);
                        _scrollbar.StepsCount = _scrollbar.Max;
                        _scrollbar.Visible = true;

                        // update text to fit scrollbar. first, rebuild the text with just the visible segment
                        List<string> lines = new List<string>(_actualDisplayText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                        int from = System.Math.Min(_scrollbar.Value, lines.Count - 1);
                        int size = System.Math.Min(linesFit, lines.Count - from);
                        lines = lines.GetRange(from, size);
                        _actualDisplayText = string.Join("\n", lines);
                        currParagraph.Text = _actualDisplayText;

                        // get at which line the text is starting to be shown
                        scrollBarLineOffset = from;
                    }
                    // if no need for scrollbar make it invisible
                    else
                    {
                        currParagraph.Size = Vector2.Zero;
                        _scrollbar.Visible = false;
                    }
                }

                if (IsCaretCurrentlyVisible)
                {
                    // move the carret so it corresponds to the scroll position
                    caretDstRect.Y -= (int)charSize.Y * scrollBarLineOffset;
                }

            } else {
                // if visible, calculate carret position for single line TextInput
                if (IsCaretCurrentlyVisible)
                {
                    caretDstRect.X = (int)(TextParagraph._actualDestRect.X + charSize.X * (_caret == -1 ? _value.Length : _caret) - CARET_WIDTH / 2);
                    caretDstRect.Y = TextParagraph._actualDestRect.Y;
                }
            }

            // set placeholder and main text visibility based on current value
            TextParagraph.Visible = !showPlaceholder;
            PlaceholderParagraph.Visible = showPlaceholder;

            // if visible and in the visible area, draw carret at calculated position
            bool carretIsInVisibleArea = caretDstRect.Y >= _destRectInternal.Y && caretDstRect.Y < _destRectInternal.Y + _destRectInternal.Height;
            if (IsCaretCurrentlyVisible && ((_multiLine && carretIsInVisibleArea) || !_multiLine))
            {
                caretDstRect.X -= CARET_WIDTH / 2;
                Texture2D blankTexture = DrawUtils.GetBlankWhiteTexture(spriteBatch.GraphicsDevice);
                spriteBatch.Draw(blankTexture, caretDstRect, DrawUtils.rectangle_1x1, TextParagraph.FillColor);
            }
        }

        /// <summary>
        /// Validate current text input after change (usually addition of text).
        /// </summary>
        /// <param name="newVal">New text value, to check validity.</param>
        /// <param name="oldVal">Previous text value, before the change.</param>
        /// <returns>True if new input is valid, false otherwise.</returns>
        private bool ValidateInput(ref string newVal, string oldVal)
        {
            // if new characters were added, and we now exceed characters limit, revet to previous value.
            if (CharactersLimit != 0 &&
                newVal.Length > CharactersLimit)
            {
                newVal = oldVal;
                return false;
            }

            // if not multiline and got line break, revet to previous value
            if (!_multiLine && newVal.Contains("\n"))
            {
                newVal = oldVal;
                return false;
            }

            // if set to limit by size make sure we don't exceed it
            if (LimitBySize)
            {
                // prepare display
                PrepareInputTextForDisplay(false);

                // get main paragraph actual size
                Rectangle textSize = TextParagraph.GetActualDestRect();

                // if multiline, compare heights
                if (_multiLine && textSize.Height >= _destRectInternal.Height)
                {
                    newVal = oldVal;
                    return false;
                }
                // if single line, compare widths
                else if (textSize.Width >= _destRectInternal.Width)
                {
                    newVal = oldVal;
                    return false;
                }
            }

            // if got here we iterate over additional validators
            foreach (var validator in Validators)
            {
                if (!validator.ValidateText(ref newVal, oldVal))
                {
                    newVal = oldVal;
                    return false;
                }
            }

            // looks good!
            return true;
        }

        /// <summary>
        /// Make sure caret position is currently valid and in range.
        /// </summary>
        private void FixCaretPosition()
        {
            if (_caret < -1) { _caret = 0; }
            if (_caret >= _value.Length || _value.Length == 0) { _caret = -1; }
        }

        /// <summary>
        /// Reset the caret animation by setting _caretAnim = 1.
        /// See the condition to draw carret in DrawEntity
        /// </summary>
        private void PauseCaretBlink()
        {
            _caretAnim = -caretBlinkAfterMoveDelay;
        }

        /// <summary>
        /// Called every frame before update.
        /// TextInput implement this function to get keyboard input and also to animate caret timer.
        /// </summary>
        override protected void DoBeforeUpdate()
        {
            // animate caret
            _caretAnim += (float)UserInterface.Active.CurrGameTime.ElapsedGameTime.TotalSeconds * CaretBlinkingSpeed;

            // if focused, and got character input in this frame..
            if (IsFocused)
            {
                // validate caret position
                FixCaretPosition();

                // get user input
                char? currCharacterInput = KeyboardInput.GetTextInput();

                // if input char is valid
                if (currCharacterInput != null)
                {
                    // init some values to handle user input
                    string oldVal = _value;
                    int newCaretPos = (_caret == -1 ? _value.Length : _caret);

                    // get caret position
                    string[] processedTextLines = PrepareInputTextForDisplay(false).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    Paragraph.LineType[] processedTextLinesCodes = TextParagraph.ProcessedTextLinesTypes;
                    Point caretPosition = CalculateCaretPositionForMultiline(processedTextLines, processedTextLinesCodes, _caret);

                    // find current line
                    Vector2 charSize = TextParagraph.GetCharacterActualSize();
                    int currentLine = (int)(caretPosition.Y / charSize.Y);
                    int charPositionInCurrentLine = (int)(caretPosition.X / charSize.X);

                    // handle special key press (control character like space or delete) and normal keys
                    switch (currCharacterInput)
                    {
                        case (char)SpecialChars.Backspace:
                            newCaretPos--;
                            if (newCaretPos < _value.Length && newCaretPos >= 0 && _value.Length > 0) _value = _value.Remove(newCaretPos, 1);
                            break;
                        case (char)SpecialChars.Delete:
                            if (newCaretPos < _value.Length && _value.Length > 0) _value = _value.Remove(newCaretPos, 1);
                            break;
                        case (char)SpecialChars.ArrowLeft: if (--newCaretPos < 0) { newCaretPos = 0; } break;
                        case (char)SpecialChars.ArrowRight: if (++newCaretPos > _value.Length) { newCaretPos = _value.Length; } break;
                        case (char)SpecialChars.ArrowUp:
                            if (currentLine != 0)
                            {
                                // go at begining of the line
                                newCaretPos -= charPositionInCurrentLine;

                                // go before newline character
                                newCaretPos--;

                                // get the number of character needed to be removed
                                int delta = processedTextLines[currentLine - 1].Length - charPositionInCurrentLine;
                                switch (processedTextLinesCodes[currentLine - 1])
                                {
                                    case Paragraph.LineType.WordWrap: delta--; break; // word wraped to next line
                                    case Paragraph.LineType.WordBroken: delta -= (TextParagraph.AddHyphenWhenBreakWord ? 3 : 2); break; // word broken into pieces
                                }

                                // if the line before has more characters than current carret index, correct carret position
                                if (delta > 0) newCaretPos -= delta;
                            }
                            break;
                        case (char)SpecialChars.ArrowDown:
                            if (currentLine != processedTextLines.Length - 1)
                            {
                                // go at the end of the line
                                newCaretPos += processedTextLines[currentLine].Length - charPositionInCurrentLine;
                                switch (processedTextLinesCodes[currentLine])
                                {
                                    case Paragraph.LineType.WordWrap: newCaretPos--; break; // word wraped to next line
                                    case Paragraph.LineType.WordBroken: newCaretPos -= (TextParagraph.AddHyphenWhenBreakWord ? 3 : 2); break; // word broken into pieces
                                }

                                // we go after the newline character
                                newCaretPos++;

                                // get number of actual char of the next line (remove word wrap and word break characters)
                                int numberOfActualCharInNextLine = processedTextLines[currentLine + 1].Length;
                                switch (processedTextLinesCodes[currentLine + 1])
                                {
                                    case Paragraph.LineType.WordWrap: numberOfActualCharInNextLine--; break; // word wraped to next line
                                    case Paragraph.LineType.WordBroken: numberOfActualCharInNextLine -= (TextParagraph.AddHyphenWhenBreakWord ? 3 : 2); break; // word broken into pieces
                                }

                                // if the next line has less characters the the index of the caret,
                                // we put the caret at the end of the next line. Otherwise we calculate the position
                                if (numberOfActualCharInNextLine > charPositionInCurrentLine) newCaretPos += charPositionInCurrentLine;
                                else newCaretPos += numberOfActualCharInNextLine;
                            }
                            break;
                        default: _value = _value.Insert(newCaretPos++, currCharacterInput.ToString()); break;
                    }

                    // if carret move or text change, make it stay a little longer on screen before blinking again
                    if (_caret != newCaretPos || _value != oldVal) PauseCaretBlink();

                    // update caret position
                    _caret = newCaretPos;

                    // if value changed:
                    if (_value != oldVal)
                    {
                        // if new characters were added and input is now illegal, revert to previous value
                        if (!ValidateInput(ref _value, oldVal))
                        {
                            _value = oldVal;
                        }

                        // call change event
                        if (_value != oldVal)
                        {
                            DoOnValueChange();
                        }

                        // after change, scroll to caret
                        ScrollToCaret();

                        // fix caret position
                        FixCaretPosition();
                    }
                }
            }

            // call base do-before-update
            base.DoBeforeUpdate();
        }

        /// <summary>
        /// Called every time this entity is focused / unfocused.
        /// </summary>
        override protected void DoOnFocusChange()
        {
            // call base on focus change
            base.DoOnFocusChange();
            
            // check if need to set default value
            if (ValueWhenEmpty != null && Value.Length == 0)
            {
                Value = ValueWhenEmpty;
            }
        }
    }
}
