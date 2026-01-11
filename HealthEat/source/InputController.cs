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
            if (e.Code != HE_Key.Unknown) m_LastKey = e.Code;

            switch (e.Code)
            {
                case HE_Key.A:
                    KeyPressedEvent?.Invoke(this, new HE_KeyPressedArgs(HE_Key.A));
                    MovementEvent?.Invoke(this, new HE_MovementEventArgs(HE_MovementFlags.MoveLeft));
                    m_MovementFlags |= HE_MovementFlags.MoveRight;
                    break;

                case HE_Key.D:
                    KeyPressedEvent?.Invoke(this, new HE_KeyPressedArgs(HE_Key.D));
                    MovementEvent?.Invoke(this, new HE_MovementEventArgs(HE_MovementFlags.MoveRight));
                    m_MovementFlags |= HE_MovementFlags.MoveRight;
                    break;

                case HE_Key.W:
                    KeyPressedEvent?.Invoke(this, new HE_KeyPressedArgs(HE_Key.W));
                    MovementEvent?.Invoke(this, new HE_MovementEventArgs(HE_MovementFlags.MoveUp));
                    m_MovementFlags |= HE_MovementFlags.MoveUp;
                    break;

                case HE_Key.S:
                    KeyPressedEvent?.Invoke(this, new HE_KeyPressedArgs(HE_Key.S));
                    MovementEvent?.Invoke(this, new HE_MovementEventArgs(HE_MovementFlags.MoveDown));
                    m_MovementFlags |= HE_MovementFlags.MoveDown;
                    break;

                case HE_Key.Space:
                    KeyPressedEvent?.Invoke(this, new HE_KeyPressedArgs(HE_Key.Space));
                    MovementEvent?.Invoke(this, new HE_MovementEventArgs(HE_MovementFlags.Jump));
                    m_MovementFlags |= HE_MovementFlags.Jump;
                    break;

                case HE_Key.E:
                    KeyPressedEvent?.Invoke(this, new HE_KeyPressedArgs(HE_Key.E));
                    ActionEvent?.Invoke(this, new HE_ActionEventArgs(HE_ActionType.Interact));
                    break;

                case HE_Key.Enter:
                    KeyPressedEvent?.Invoke(this, new HE_KeyPressedArgs(HE_Key.Enter));
                    ActionEvent?.Invoke(this, new HE_ActionEventArgs(HE_ActionType.Enter));
                    break;

                case HE_Key.Backspace:
                    KeyPressedEvent?.Invoke(this, new HE_KeyPressedArgs(HE_Key.Backspace));
                    ActionEvent?.Invoke(this, new HE_ActionEventArgs(HE_ActionType.Return));
                    break;

                default:
                    KeyPressedEvent?.Invoke(this, new HE_KeyPressedArgs(e.Code));
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

            if (e.Button == HE_MouseBtn.Left)
            {
                m_Dragging = true;
                m_MousePosDragStart = m_LastPressedPos;
                m_TimeSinceLastPress = 0f;
                MousePressEvent?.Invoke(this, new HE_MouseMoveEventArgs(e.X, e.Y));
            }
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

            if ((m_TimeSinceLastPress <= m_MaxClickTimespan) && (releaseOffset <= m_MaxReleaseOffset))
            {
                HE_MouseClickedArgs args = new HE_MouseClickedArgs(m_LastPressedPos, e.Button);
                MouseClickedEvent?.Invoke(this, args);
                MouseReleaseEvent?.Invoke(this, new HE_MouseMoveEventArgs(args.X, args.Y));
                m_Dragging = false;

                #if DEBUG
                Console.WriteLine($"[InputController]: Mouse clicked at {args.X}, {args.Y} with {args.BtnClicked}");
                #endif

                return;
            }

            m_Dragging = false;
            MouseReleaseEvent?.Invoke(this, new HE_MouseMoveEventArgs(e.X, e.Y));
            #if DEBUG
            Console.WriteLine($"[InputController]: Mouse realeased at {e.X}, {e.Y}");
            #endif

        }


        public void HandleMouseMovedEvent(object? s, MouseMoveEventArgs e)
        {
            if (s == null || (s != null && (HE_RenderWindow)s != Window.ActiveWindow))
            {
                m_MouseMovingPos = new HE_Vec2i(0, 0);
                return;
            }
            if (m_Dragging)
            {
                m_TimeSinceLastPress += GameManager.GetTick;
                m_MouseMovingPos = new HE_Vec2i(e.X, e.Y);
                MouseMovedEvent?.Invoke(this, new HE_MouseMoveEventArgs(e.X, e.Y));
            }
            
        }


        public event EventHandler<HE_MouseClickedArgs>? MouseClickedEvent;
        public event EventHandler<HE_KeyPressedArgs>? KeyPressedEvent;
        public event EventHandler<HE_MovementEventArgs>? MovementEvent;
        public event EventHandler<HE_ActionEventArgs>? ActionEvent;
        public event EventHandler<HE_MouseMoveEventArgs>? MousePressEvent;
        public event EventHandler<HE_MouseMoveEventArgs>? MouseMovedEvent;
        public event EventHandler<HE_MouseMoveEventArgs>? MouseReleaseEvent;
        public bool IsDragging => m_Dragging;
        public HE_Key LastKeyPressed => m_LastKey;
        public int MovementFlags => (int)m_MovementFlags;
        public int MouseFlags => m_MouseFlags;

        private bool m_Dragging = false;
        private HE_Key m_LastKey;
        private HE_MovementFlags m_MovementFlags = 0;
        private int m_MouseFlags = 0;
        private HE_Vec2i m_LastPressedPos = new HE_Vec2i(0, 0);
        private HE_Vec2i m_MouseMovingPos = new HE_Vec2i(0, 0);
        private HE_Vec2i m_MousePosDragStart = new HE_Vec2i(0, 0);
        private HE_Vec2i m_MousePosDragEnd = new HE_Vec2i(0, 0);
        private float m_TimeSinceLastPress = 0f;
        private const float m_MaxReleaseOffset = 10.0f;
        private const float m_MaxClickTimespan = 0.2f;

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


    internal class HE_KeyPressedArgs : EventArgs
    {

        public HE_KeyPressedArgs(HE_Key key, bool mod = false)
        {
            Key = key;
            Modifier = mod;
        }

        public HE_Key Key { get; }
        public bool Modifier { get; }

    }


    internal class HE_MovementEventArgs : EventArgs
    {

        public HE_MovementEventArgs(HE_MovementFlags flag)
        {
            Action = flag;
            Tick = GameManager.GetTick;
        }

        public HE_MovementFlags Action { get; }
        public float Tick { get; }
    }


    internal class HE_MouseMoveEventArgs : EventArgs
    {

        public HE_MouseMoveEventArgs(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }


    internal class HE_ActionEventArgs : EventArgs
    {

        public HE_ActionEventArgs(HE_ActionType type)
        {
            Action = type;
        }

        public HE_ActionType Action { get; }

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


    internal enum HE_ActionType : int
    {
        Interact = 0x01,
        Enter = 0x02,
        Return = 0x04
    }
}
