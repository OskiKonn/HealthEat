
global using HE_Vec2 = SFML.System.Vector2f;
//global using HE_Vec2u = SFML.System.Vector2u;

global using HE_RenderWindow = SFML.Graphics.RenderWindow;
global using HE_Text = SFML.Graphics.Text;
global using HE_Sprite = SFML.Graphics.Sprite;
global using HE_Texture = SFML.Graphics.Texture;
global using HE_FloatRect = SFML.Graphics.FloatRect;
global using HE_RectShape = SFML.Graphics.RectangleShape;
global using HE_Color = SFML.Graphics.Color;
global using HE_Clock = SFML.System.Clock;
global using HE_KeyBoard = SFML.Window.Keyboard;
global using HE_Key = SFML.Window.Keyboard.Key;
global using HE_Mouse = SFML.Window.Mouse;
global using HE_MouseBtn = SFML.Window.Mouse.Button;
global using HE_KeyScanCode = SFML.Window.Keyboard.Scancode;
global using HE_Vec2i = SFML.System.Vector2i;
//global using HE_Position = SFML.System.Vector2f;

global using HE_KeyEventArgs = SFML.Window.KeyEventArgs;

using System;

namespace HealthEat
{

    /// <summary>
    /// Extension methods for SFML Vector2f and Vector2i classes.
    /// </summary>
    public static class Vector2Extension
    {

        /// <summary>
        /// Calculates and returns the length (magnitude) of a 2D float vector.
        /// </summary>
        /// <param name="v">The vector instance.</param>
        /// <returns>The length of the vector.</returns>
        public static float GetLength(this HE_Vec2 v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        /// <summary>
        /// Calculates and returns the length (magnitude) of a 2D integer vector.
        /// </summary>
        /// <param name="v">The vector instance.</param>
        /// <returns>The length of the vector.</returns>
        public static float GetLength(this HE_Vec2i v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

    }

}
