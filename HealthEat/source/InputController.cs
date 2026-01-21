
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
            sm_Instance = this;

            HE_EventBus.Get.RegisterEvent<HE_KeyEvent>();
            HE_EventBus.Get.RegisterEvent<HE_MouseClickedEvent>();
            HE_EventBus.Get.RegisterEvent<HE_MouseMoveEvent>();
            HE_EventBus.Get.RegisterEvent<HE_MousePressedEvent>();
            HE_EventBus.Get.RegisterEvent<HE_MouseReleasedEvent>();

            #if HE_DEBUG
            Console.WriteLine("[InputController]: Created InputController");
            #endif
        }


        public void UpdateKeyStates()
        {
            foreach(KeyValuePair<HE_Key, bool> kvp in m_CurrKeyState)
            {
                m_PrevKeyState[kvp.Key] = kvp.Value;
                m_CurrKeyState[kvp.Key] = HE_KeyBoard.IsKeyPressed(kvp.Key);
            }
        }


        public void SetDefaultKeybinds()
        {
            m_Keybinds[HE_Action.MoveUp] = HE_Key.W;
            m_Keybinds[HE_Action.MoveDown] = HE_Key.S;
            m_Keybinds[HE_Action.MoveLeft] = HE_Key.A;
            m_Keybinds[HE_Action.MoveRight] = HE_Key.D;
            m_Keybinds[HE_Action.Jump] = HE_Key.Space;
            m_Keybinds[HE_Action.Interact] = HE_Key.E;
            m_Keybinds[HE_Action.Enter] = HE_Key.Enter;
            m_Keybinds[HE_Action.Pause] = HE_Key.Escape;
            m_Keybinds[HE_Action.Return] = HE_Key.Backspace;

            foreach (KeyValuePair<HE_Action, HE_Key> kvp in m_Keybinds)
            {
                m_CurrKeyState[kvp.Value] = false;
                m_PrevKeyState[kvp.Value] = false;
            }
        }


        public bool IsActionHeld(HE_Action action)
        {

            if (m_Keybinds.TryGetValue(action, out HE_Key key))
            {
                return m_CurrKeyState[key];
            }

            return false;
        }


        public bool IsActionTriggered(HE_Action action)
        {
            if (m_Keybinds.TryGetValue(action, out HE_Key key))
            {
                bool isDown = m_CurrKeyState[key];
                bool wasDown = m_PrevKeyState[key];

                return isDown && !wasDown;
            }

            return false;
        }


        //public HE_Vec2i GetMousePos()
        //{
        //    return HE_Mouse.GetPosition(Window.ActiveWindow);
        //}


        public void HandleKeyPress(object? s, KeyEventArgs e)
        {
            HandleKeyAction(s, e, HE_KeyAction.Pressed);
        }


        public void HandleKeyRelease(object? s, KeyEventArgs e)
        {
            HandleKeyAction(s, e, HE_KeyAction.Released);
        }


        public void HandleMousePressedEvent(object? s, MouseButtonEventArgs e)
        {
            //if (s == null || (s != null && (HE_RenderWindow)s != Window.ActiveWindow))
            //    return;

            m_LastPressedPos = new HE_Vec2i(e.X, e.Y);

            if (e.Button == HE_MouseBtn.Left)
            {
                m_Dragging = true;
                m_MousePosDragStart = m_LastPressedPos;
                m_TimeSinceLastPress = 0f;
                HE_EventBus.Get.BroadcastEvent<HE_MousePressedEvent>(new HE_MousePressedEvent(e.X, e.Y, e.Button));
            }
        }


        public void HandleMouseReleasedEvent(object? s, MouseButtonEventArgs e)
        {
            //if (s == null || (s != null && (HE_RenderWindow)s != Window.ActiveWindow))
            //{
            //    m_LastPressedPos = new HE_Vec2i(0, 0);
            //    return;
            //}

            HE_Vec2i relPos = new HE_Vec2i(e.X, e.Y);
            float releaseOffset = (relPos - m_LastPressedPos).GetLength();

            if ((m_TimeSinceLastPress <= m_MaxClickTimespan) && (releaseOffset <= m_MaxReleaseOffset))
            {
                HE_MouseClickedEvent args = new HE_MouseClickedEvent(m_LastPressedPos, e.Button);
                HE_EventBus.Get.BroadcastEvent<HE_MouseClickedEvent>(args);
                HE_EventBus.Get.BroadcastEvent<HE_MouseReleasedEvent>(new HE_MouseReleasedEvent(args.X, args.Y, e.Button));
                m_Dragging = false;

                #if HE_DEBUG
                Console.WriteLine($"[InputController]: Mouse clicked at {args.X}, {args.Y} with {args.BtnClicked}");
                #endif

                return;
            }

            m_Dragging = false;
            HE_EventBus.Get.BroadcastEvent<HE_MouseReleasedEvent>(new HE_MouseReleasedEvent(e.X, e.Y, e.Button));
            #if HE_DEBUG
            //Console.WriteLine($"[InputController]: Mouse realeased at {e.X}, {e.Y}");
            #endif

        }


        public void HandleMouseMovedEvent(object? s, MouseMoveEventArgs e)
        {
            //if (s == null || (s != null && (HE_RenderWindow)s != Window.ActiveWindow))
            //{
            //    m_MouseMovingPos = new HE_Vec2i(0, 0);
            //    return;
            //}
            if (m_Dragging)
            {
                m_TimeSinceLastPress += GameManager.GetTick;
                m_MouseMovingPos = new HE_Vec2i(e.X, e.Y);
                HE_EventBus.Get.BroadcastEvent<HE_MouseMoveEvent>(new HE_MouseMoveEvent(e.X, e.Y));
            }
            
        }


        private void HandleKeyAction(object? s, KeyEventArgs e, HE_KeyAction keyAction)
        {
            if (e.Code != HE_Key.Unknown) m_LastKey = e.Code;

            HE_EventBus.Get.BroadcastEvent<HE_KeyEvent>(new HE_KeyEvent(e.Code, keyAction, e.Control));

            #if HE_DEBUG
            //Console.WriteLine($"[InputController]: {keyAction.ToString()} key '{e.Code.ToString()}'");
            #endif
        }


        public static InputController Get => sm_Instance;

        public bool IsDragging => m_Dragging;
        public HE_Key LastKeyPressed => m_LastKey;
        //public int MovementFlags => (int)m_MovementFlags;
        public int MouseFlags => m_MouseFlags;

        private static InputController sm_Instance = null!;
        private Dictionary<HE_Action, HE_Key> m_Keybinds = new Dictionary<HE_Action, HE_Key>(9);
        private Dictionary<HE_Key, bool> m_CurrKeyState = new Dictionary<HE_Key, bool>(9);
        private Dictionary<HE_Key, bool> m_PrevKeyState = new Dictionary<HE_Key, bool>(9);
        private bool m_Dragging = false;
        private HE_Key m_LastKey;
        //private HE_MovementFlags m_MovementFlags = 0;
        private int m_MouseFlags = 0;
        private HE_Vec2i m_LastPressedPos = new HE_Vec2i(0, 0);
        private HE_Vec2i m_MouseMovingPos = new HE_Vec2i(0, 0);
        private HE_Vec2i m_MousePosDragStart = new HE_Vec2i(0, 0);
        private HE_Vec2i m_MousePosDragEnd = new HE_Vec2i(0, 0);
        private float m_TimeSinceLastPress = 0f;
        private const float m_MaxReleaseOffset = 10.0f;
        private const float m_MaxClickTimespan = 0.2f;

    }


    internal struct HE_MouseClickedEvent : IHE_EventType
    {

        public HE_MouseClickedEvent(HE_Vec2i pos, HE_MouseBtn btn)
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


    internal struct HE_KeyEvent : IHE_EventType
    {

        public HE_KeyEvent(HE_Key key, HE_KeyAction keyAction, bool mod = false)
        {
            Key = key;
            KeyAction = keyAction;
            Modifier = mod;
        }

        public HE_Key Key { get; }
        public HE_KeyAction KeyAction { get; }
        public bool Modifier { get; }

    }


    //internal struct HE_MovementEvent : IHE_EventType
    //{

    //    public HE_MovementEvent(HE_MovementFlags flag, HE_KeyAction keyAction)
    //    {
    //        Action = flag;
    //        KeyAction = keyAction;
    //    }

    //    public HE_MovementFlags Action { get; }
    //    public HE_KeyAction KeyAction { get; }
    //}


    internal struct HE_MouseMoveEvent : IHE_EventType
    {

        public HE_MouseMoveEvent(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }


    internal struct HE_MousePressedEvent : IHE_EventType
    {

        public HE_MousePressedEvent(int x, int y, HE_Mouse.Button btn)
        {
            X = x;
            Y = y;
            Button = btn;
        }

        public int X { get; }
        public int Y { get; }
        public HE_Mouse.Button Button { get; }
    }

    internal struct HE_MouseReleasedEvent : IHE_EventType
    {

        public HE_MouseReleasedEvent(int x, int y, HE_Mouse.Button btn)
        {
            X = x;
            Y = y;
            Button = btn;
        }

        public int X { get; }
        public int Y { get; }
        public HE_Mouse.Button Button { get; }
    }


    internal struct HE_ActionEvent : IHE_EventType
    {

        public HE_ActionEvent(HE_Action type)
        {
            Action = type;
        }

        public HE_Action Action { get; }

    }


    internal enum HE_Action : int
    {
        Interact = 0,
        Enter,
        Return,
        Pause,
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Jump
    }


    internal enum HE_KeyAction : uint
    {
        Pressed = 1,
        Released = 2
    }
}
