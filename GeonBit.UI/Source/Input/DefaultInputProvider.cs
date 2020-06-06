#region File Description
//-----------------------------------------------------------------------------
// Helper utility that implements default mouse and keyboard input for GeonBit.UI.
// You can create your own mouse/keyboard inputs to replace this.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------
#endregion
using GeonBit.KeyboardLayouts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace GeonBit.UI
{
    /// <summary>
    /// Implement Mouse Input and Keyboard Input for GeonBit.UI + provide some helpful utils you can use externally.
    /// This is the object we provide to GeonBit.UI by default, if no other input providers were set by user.
    /// </summary>
    public class DefaultInputProvider : IMouseInput, IKeyboardInput
    {
        // store current & previous keyboard states so we can detect key release
        KeyboardState _newKeyboardState;
        KeyboardState _oldKeyboardState;

        // store current & previous mouse states so we can detect key release and diff
        MouseState _newMouseState;
        MouseState _oldMouseState;

        // store old and new mouse position so we can get diff
        Vector2 _newMousePos;
        Vector2 _oldMousePos;

        // store current frame gametime
        GameTime _currTime;

        // store all Key values in lookup array
        Keys[] _allKeyValues;

        /// <summary>An artificial "lag" after a key is pressed when typing text input, to prevent mistake duplications.</summary>
        public float KeysTypeCooldown = 0.6f;

        // last character input key
        char _currCharacterInput = '\0';

        // last key that was pressed
        Keys _currInputKey = Keys.None;

        // true or false if the user is holding the key down
        bool _isHoldingKey;

        // keyboard input cooldown for textual input
        float _keyboardInputCooldown = 0f;

        // true when a new keyboard key is pressed
        bool _newKeyIsPressed = false;

        // current capslock state
        bool _capslock = false;

        // the keyboard layout when non has been set
        Type defaultKeyboardLayout = typeof(US);

        /// <summary>
        /// The keyboard layout used when typing. (US, french, etc.).
        /// </summary>
        public KeyboardLayout keyboardLayout;

        /// <summary>
        /// Current mouse wheel value.
        /// </summary>
        public int MouseWheel { get; private set; }

        /// <summary>
        /// Mouse wheel change sign (eg 0, 1 or -1) since last frame.
        /// </summary>
        public int MouseWheelChange { get; private set; }

        /// <summary>
        /// Create the input helper.
        /// </summary>
        public DefaultInputProvider()
        {
            _allKeyValues = (Keys[])System.Enum.GetValues(typeof(Keys));

            // init keyboard states
            _newKeyboardState = _oldKeyboardState;

            // init mouse states
            _newMouseState = _oldMouseState;
            _newMousePos = new Vector2(_newMouseState.X, _newMouseState.Y);

            // set default keyboardLayout
            keyboardLayout = (KeyboardLayout)Activator.CreateInstance(defaultKeyboardLayout);

            // call first update to get starting positions
            Update(new GameTime());
        }

        /// <summary>
        /// Current frame game time.
        /// </summary>
        public GameTime CurrGameTime
        {
            get { return _currTime; }
        }

        /// <summary>
        /// Update current states.
        /// If used outside GeonBit.UI, this function should be called first thing inside your game 'Update()' function,
        /// and before you make any use of this class.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        public void Update(GameTime gameTime)
        {
            // store game time
            _currTime = gameTime;

            // store previous states
            _oldMouseState = _newMouseState;
            _oldKeyboardState = _newKeyboardState;

            // get new states
            _newMouseState = Mouse.GetState();
            _newKeyboardState = Keyboard.GetState();

            // get mouse position
            _oldMousePos = _newMousePos;
            _newMousePos = new Vector2(_newMouseState.X, _newMouseState.Y);

            // get mouse wheel state
            int prevMouseWheel = MouseWheel;
            MouseWheel = _newMouseState.ScrollWheelValue;
            MouseWheelChange = System.Math.Sign(MouseWheel - prevMouseWheel);

            // update capslock state
            if (_newKeyboardState.IsKeyDown(Keys.CapsLock) && !_oldKeyboardState.IsKeyDown(Keys.CapsLock))
            {
                _capslock = !_capslock;
            }

            // decrease keyboard input cooldown time
            if (_keyboardInputCooldown > 0f)
            {
                _newKeyIsPressed = false;
                _keyboardInputCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // if current text input key is no longer down
            if (_isHoldingKey && !_newKeyboardState.IsKeyDown(_currInputKey))
            {
                _isHoldingKey = false;
            }

            // send key-down events
            foreach (var key in _allKeyValues)
            {
                if (_newKeyboardState.IsKeyDown(key) && !_oldKeyboardState.IsKeyDown(key))
                {
                    OnKeyPressed(key);
                }
            }
        }

        /// <summary>
        /// Move the cursor to be at the center of the screen.
        /// </summary>
        /// <param name="pos">New mouse position.</param>
        public void UpdateMousePosition(Vector2 pos)
        {
            // move mouse position back to center
            Mouse.SetPosition((int)pos.X, (int)pos.Y);
            _newMousePos = _oldMousePos = pos;
        }

        /// <summary>
        /// Calculate and return current cursor position transformed by a matrix.
        /// </summary>
        /// <param name="transform">Matrix to transform cursor position by.</param>
        /// <returns>Cursor position with optional transform applied.</returns>
        public Vector2 TransformMousePosition(Matrix? transform)
        {
            var newMousePos = _newMousePos;
            if (transform != null)
            {
                return Vector2.Transform(newMousePos, transform.Value) - new Vector2(transform.Value.Translation.X, transform.Value.Translation.Y);
            }
            return newMousePos;
        }

        /// <summary>
        /// Called every time a keyboard key is pressed (called once on the frame key was pressed).
        /// </summary>
        /// <param name="key">Key code that is being pressed on this frame.</param>
        protected void OnKeyPressed(Keys key)
        {
            NewKeyTextInput(key);
        }

        /// <summary>
        /// This update the character the user currently type down, for text input.
        /// This function called whenever a new key is pressed down, and becomes the current input
        /// until it is released.
        /// </summary>
        /// <param name="key">Key code that is being pressed down on this frame.</param>
        private void NewKeyTextInput(Keys key)
        {
            // when key has just been pressed, set cooldown, which key has been pressed and a boolean to tell that a key is pressed
            _keyboardInputCooldown = KeysTypeCooldown;
            _currInputKey = key;
            _newKeyIsPressed = true;
            _isHoldingKey = true;

            // set "shift", "alt gr" and "control" key state for current user interface
            bool isShiftDown = UserInterface.Active.isShiftDown = _newKeyboardState.IsKeyDown(Keys.LeftShift) || _newKeyboardState.IsKeyDown(Keys.RightShift);
            bool isControlDown = UserInterface.Active.isControlDown = _newKeyboardState.IsKeyDown(Keys.LeftControl) || _newKeyboardState.IsKeyDown(Keys.RightControl);
            bool isAltGrDown = UserInterface.Active.isAltGrDown = _newKeyboardState.IsKeyDown(Keys.RightAlt);

            // if the pressed key is a control key, set current char input to empty and return
            if (UserInterface.ControlKeys.Contains(_currInputKey))
            {
                _currCharacterInput = '\0';
                return;
            }

            // we do not allow to enter a key with alt gr and shift pressed down (this is the normal behaviour on most text editors)
            if (isAltGrDown && isShiftDown) return;

            // if the pressedKey is a letter and we didn't press alt gr, bypass the "shift" validation
            // to set uppercase accordingly with "caps lock" and "shift" state
            if (key >= Keys.A && key <= Keys.Z && !isAltGrDown)
            {
                bool isUpperCase = (isShiftDown) ? !_capslock : _capslock;

                if (isUpperCase && keyboardLayout.ShiftKeys.ContainsKey(key))
                {
                    _currCharacterInput = keyboardLayout.ShiftKeys[key];
                }
                else if (keyboardLayout.NormalKeys.ContainsKey(key))
                {
                    _currCharacterInput = keyboardLayout.NormalKeys[key];
                }

                return;
            }

            // normal keys
            if (!isShiftDown && !isAltGrDown && keyboardLayout.NormalKeys.ContainsKey(key))
            {
                _currCharacterInput = keyboardLayout.NormalKeys[key];
                return;
            }
            // shift keys
            else if (isShiftDown && keyboardLayout.ShiftKeys.ContainsKey(key))
            {
                _currCharacterInput = keyboardLayout.ShiftKeys[key];
                return;
            }
            // alt gr keys
            else if (isAltGrDown && keyboardLayout.AltGrKeys.ContainsKey(key))
            {
                _currCharacterInput = keyboardLayout.AltGrKeys[key];
                return;
            }

            // unhandled character
            _currCharacterInput = '\0';
        }

        /// <summary>
        /// Get textual input from keyboard.
        /// This also handles keyboard cooldown, to make it feel like windows-input.
        /// </summary>
        /// <returns>The char after text input applied on it. If the input is invalid, return null.</returns>
        public char? GetTextInput()
        {
            // if need to skip due to cooldown time
            if (!_newKeyIsPressed && _keyboardInputCooldown > 0f) return null;

            // if the user is not holding the key, don't send char
            if (!_isHoldingKey) return null;

            return _currCharacterInput;
        }

        /// <summary>
        /// Getter for the currently pressed key.
        /// </summary>
        /// <returns>The currently pressed key.</returns>
        public Keys GetInputKey() => _currInputKey;

        /// <summary>
        /// Set a new keyboard layout.
        /// </summary>
        /// <param name="keyboardLayout">The new keyboard layout.</param>
        public void SetKeyboardLayout(KeyboardLayout keyboardLayout)
        {
            this.keyboardLayout = keyboardLayout;
        }

        /// <summary>
        /// Get current mouse poisition.
        /// </summary>
        public Vector2 MousePosition
        {
            get { return _newMousePos; }
        }

        /// <summary>
        /// Get mouse position change since last frame.
        /// </summary>
        /// <return>Mouse position change as a 2d vector.</return>
        public Vector2 MousePositionDiff
        {
            get { return _newMousePos - _oldMousePos; }
        }

        /// <summary>
        /// Check if a given mouse button is down.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button is down.</return>
        public bool MouseButtonDown(MouseButton button = MouseButton.Left)
        {
            return GetMouseButtonState(button) == ButtonState.Pressed;
        }

        /// <summary>
        /// Return if any of mouse buttons is down.
        /// </summary>
        /// <returns>True if any mouse button is currently down.</returns>
        public bool AnyMouseButtonDown()
        {
            return MouseButtonDown(MouseButton.Left) ||
                MouseButtonDown(MouseButton.Right) ||
                MouseButtonDown(MouseButton.Middle);
        }

        /// <summary>
        /// Check if a given mouse button was released in current frame.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button was released in this frame.</return>
        public bool MouseButtonReleased(MouseButton button = MouseButton.Left)
        {
            return GetMouseButtonState(button) == ButtonState.Released && GetMousePreviousButtonState(button) == ButtonState.Pressed;
        }

        /// <summary>
        /// Return if any mouse button was released this frame.
        /// </summary>
        /// <returns>True if any mouse button was released.</returns>
        public bool AnyMouseButtonReleased()
        {
            return MouseButtonReleased(MouseButton.Left) ||
                MouseButtonReleased(MouseButton.Right) ||
                MouseButtonReleased(MouseButton.Middle);
        }

        /// <summary>
        /// Check if a given mouse button was pressed in current frame.
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button was pressed in this frame.</return>
        public bool MouseButtonPressed(MouseButton button = MouseButton.Left)
        {
            return GetMouseButtonState(button) == ButtonState.Pressed && GetMousePreviousButtonState(button) == ButtonState.Released;
        }

        /// <summary>
        /// Return if any mouse button was pressed in current frame.
        /// </summary>
        /// <returns>True if any mouse button was pressed in current frame..</returns>
        public bool AnyMouseButtonPressed()
        {
            return MouseButtonPressed(MouseButton.Left) ||
                MouseButtonPressed(MouseButton.Right) ||
                MouseButtonPressed(MouseButton.Middle);
        }

        /// <summary>
        /// Check if a given mouse button was just clicked (eg released after being pressed down)
        /// </summary>
        /// <param name="button">Mouse button to check.</param>
        /// <return>True if given mouse button is clicked.</return>
        public bool MouseButtonClick(MouseButton button = MouseButton.Left)
        {
            return GetMouseButtonState(button) == ButtonState.Released && GetMousePreviousButtonState(button) == ButtonState.Pressed;
        }

        /// <summary>
        /// Return if any of mouse buttons was clicked this frame.
        /// </summary>
        /// <returns>True if any mouse button was clicked.</returns>
        public bool AnyMouseButtonClicked()
        {
            return
                MouseButtonClick(MouseButton.Left) ||
                MouseButtonClick(MouseButton.Right) ||
                MouseButtonClick(MouseButton.Middle);
        }

        /// <summary>
        /// Return the state of a mouse button (up / down).
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>Mouse button state.</returns>
        private ButtonState GetMouseButtonState(MouseButton button = MouseButton.Left)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return _newMouseState.LeftButton;
                case MouseButton.Right:
                    return _newMouseState.RightButton;
                case MouseButton.Middle:
                    return _newMouseState.MiddleButton;
            }
            return ButtonState.Released;
        }

        /// <summary>
        /// Return the state of a mouse button (up / down), in previous frame.
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>Mouse button state.</returns>
        private ButtonState GetMousePreviousButtonState(MouseButton button = MouseButton.Left)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return _oldMouseState.LeftButton;
                case MouseButton.Right:
                    return _oldMouseState.RightButton;
                case MouseButton.Middle:
                    return _oldMouseState.MiddleButton;
            }
            return ButtonState.Released;
        }

        /// <summary>
        /// Check if a given keyboard key is down.
        /// </summary>
        /// <param name="key">Key button to check.</param>
        /// <return>True if given key button is down.</return>
        public bool IsKeyDown(Keys key)
        {
            return _newKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Check if a given keyboard key was previously pressed down and now released in this frame.
        /// </summary>
        /// <param name="key">Key button to check.</param>
        /// <return>True if given key button was just released.</return>
        public bool IsKeyReleased(Keys key)
        {
            return _oldKeyboardState.IsKeyDown(key) &&
                   _newKeyboardState.IsKeyUp(key);
        }
    }
}
