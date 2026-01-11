using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthEat
{

    internal class InputController
    {

        public InputController()
        {
            #if DEBUG
            Console.WriteLine("[InputController]: Created InputController");
            #endif
        }


        public int GetInputFlags()
        {
            return (int)m_MovementFlags | m_MouseFlags;
        }


        public HE_Vec2i GetMousePos()
        {
            return HE_Mouse.GetPosition(Window.ActiveWindow);
        }


        public void HandleKbInput(object? s, KeyEventArgs e)
        {
            switch (e.Code)
            {
                case HE_Key.A:
                    m_MovementFlags |= HE_MovementFlags.MoveLeft;
                    break;

                case HE_Key.D:
                    m_MovementFlags |= HE_MovementFlags.MoveRight;
                    break;

                case HE_Key.W:
                    m_MovementFlags |= HE_MovementFlags.MoveUp;
                    break;

                case HE_Key.S:
                    m_MovementFlags |= HE_MovementFlags.MoveDown;
                    break;

                case HE_Key.Space:
                    m_MovementFlags |= HE_MovementFlags.Jump;
                    break;

                default:
                    break;
            }

            #if DEBUG
            Console.WriteLine($"[InputController]: Pressed key '{e.Code.ToString()}'");
            #endif
        }


        public void HandleMousePressedEvent(object? s, MouseButtonEventArgs e)
        {
            if (s == null || (s != null && (HE_RenderWindow)s != Window.ActiveWindow))
                return;

            m_LastPressedPos = new HE_Vec2i(e.X, e.Y);
        }


        public void HandleMouseReleasedEvent(object? s, MouseButtonEventArgs e)
        {
            if (s == null || (s != null && (HE_RenderWindow)s != Window.ActiveWindow))
            {
                m_LastPressedPos = new HE_Vec2i(0, 0);
                return;
            }

            HE_Vec2i relPos = new HE_Vec2i(e.X, e.Y);
            float releaseOffset = (relPos - m_LastPressedPos).GetLength();

            if (releaseOffset > m_MaxReleaseOffset)
            {
                m_LastPressedPos = new HE_Vec2i(0, 0);
                return;
            }

            HE_MouseClickedArgs args = new HE_MouseClickedArgs(m_LastPressedPos, e.Button);

            MouseClicked?.Invoke(this, args);

            #if DEBUG
            Console.WriteLine($"[InputController]: Mouse clicked at {args.X}, {args.Y} with {args.BtnClicked}");
            #endif
        }


        public event EventHandler<HE_MouseClickedArgs> MouseClicked;
        public bool IsDragging => m_Dragging;
        public HE_Key LastKeyPressed => m_LastKey;
        public int MovementFlags => (int)m_MovementFlags;
        public int MouseFlags => m_MouseFlags;

        private bool m_Dragging = false;
        private HE_Key m_LastKey;
        private HE_MovementFlags m_MovementFlags = 0;
        private int m_MouseFlags = 0;
        private HE_Vec2i m_LastPressedPos = new HE_Vec2i(0, 0);
        private const float m_MaxReleaseOffset = 10.0f;

    }


    internal class HE_MouseClickedArgs : EventArgs
    {

        public HE_MouseClickedArgs(HE_Vec2i pos, HE_MouseBtn btn)
        {
            ClickPos = pos;
            BtnClicked = btn;
            X = pos.X;
            Y = pos.Y;
        }

        public HE_Vec2i ClickPos { get; }
        public HE_MouseBtn BtnClicked { get; }
        public int X { get; }
        public int Y { get; }

    }


    internal enum HE_MovementFlags : int
    {
        NoMove = 0x00,
        MoveLeft = 0x01,
        MoveRight = 0x02,
        MoveUp = 0x04,
        MoveDown = 0x08,
        Jump = 0x10
    }


    internal enum HE_MouseFlags : int
    {
        LeftClicked = 0x20,
        RightClicked = 0x40,
        MiddleClicked = 0x80
    }
}
