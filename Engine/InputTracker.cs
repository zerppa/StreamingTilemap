namespace Engine
{
    using System.Collections.Generic;
    using System.Numerics;

    using Veldrid;

    /// <summary>
    /// Provides access to the latest snapshot of keyboard and mouse state.
    /// </summary>
    public static class InputTracker
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
#pragma warning disable SA1401 // Fields should be private
        /// <summary>
        /// The current mouse position.
        /// </summary>
        public static Vector2 MousePosition;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore CA2211 // Non-constant fields should not be visible

        /// <summary>
        /// Keeps track of the pressed keys.
        /// </summary>
        private static readonly HashSet<Key> CurrentlyPressedKeys = new HashSet<Key>();

        /// <summary>
        /// Lists the key state changes in the most recent update.
        /// </summary>
        private static readonly HashSet<Key> NewKeysThisFrame = new HashSet<Key>();

        /// <summary>
        /// Keeps track of the pressed mouse buttons.
        /// </summary>
        private static readonly HashSet<MouseButton> CurrentlyPressedMouseButtons = new HashSet<MouseButton>();

        /// <summary>
        /// Lists the mouse button state changes in the most recent update.
        /// </summary>
        private static readonly HashSet<MouseButton> NewMouseButtonsThisFrame = new HashSet<MouseButton>();

        /// <summary>
        /// Gets the most recent snapshot.
        /// </summary>
        public static InputSnapshot FrameSnapshot { get; private set; }

        /// <summary>
        /// Gets the key state.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the key is currently pressed.</returns>
        public static bool GetKey(Key key) => CurrentlyPressedKeys.Contains(key);

        /// <summary>
        /// Gets the key state.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the key was just pressed down in the most recent update.</returns>
        public static bool GetKeyDown(Key key) => NewKeysThisFrame.Contains(key);

        /// <summary>
        /// Gets the mouse button state.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <returns><c>true</c> if the button is currently pressed.</returns>
        public static bool GetMouseButton(MouseButton button) => CurrentlyPressedMouseButtons.Contains(button);

        /// <summary>
        /// Gets the mouse button state.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <returns><c>true</c> if the button was just pressed down in the most recent update.</returns>
        public static bool GetMouseButtonDown(MouseButton button) => NewMouseButtonsThisFrame.Contains(button);

        /// <summary>
        /// Processes the snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public static void UpdateFrameInput(InputSnapshot snapshot)
        {
            FrameSnapshot = snapshot;
            NewKeysThisFrame.Clear();
            NewMouseButtonsThisFrame.Clear();

            MousePosition = snapshot.MousePosition;

            for (var i = 0; i < snapshot.KeyEvents.Count; i++)
            {
                var keyEvent = snapshot.KeyEvents[i];
                if (keyEvent.Down)
                {
                    KeyDown(keyEvent.Key);
                }
                else
                {
                    KeyUp(keyEvent.Key);
                }
            }

            for (var i = 0; i < snapshot.MouseEvents.Count; i++)
            {
                var mouseEvent = snapshot.MouseEvents[i];
                if (mouseEvent.Down)
                {
                    MouseDown(mouseEvent.MouseButton);
                }
                else
                {
                    MouseUp(mouseEvent.MouseButton);
                }
            }
        }

        /// <summary>
        /// Handles a mouse button release event.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        private static void MouseUp(MouseButton mouseButton)
        {
            CurrentlyPressedMouseButtons.Remove(mouseButton);
            NewMouseButtonsThisFrame.Remove(mouseButton);
        }

        /// <summary>
        /// Handles a mouse button press event.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        private static void MouseDown(MouseButton mouseButton)
        {
            if (CurrentlyPressedMouseButtons.Add(mouseButton))
            {
                NewMouseButtonsThisFrame.Add(mouseButton);
            }
        }

        /// <summary>
        /// Handles a key release event.
        /// </summary>
        /// <param name="key">The key.</param>
        private static void KeyUp(Key key)
        {
            CurrentlyPressedKeys.Remove(key);
            NewKeysThisFrame.Remove(key);
        }

        /// <summary>
        /// Handles a key press event.
        /// </summary>
        /// <param name="key">The key.</param>
        private static void KeyDown(Key key)
        {
            if (CurrentlyPressedKeys.Add(key))
            {
                NewKeysThisFrame.Add(key);
            }
        }
    }
}
