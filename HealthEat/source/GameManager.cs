using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthEat
{
    internal class GameManager
    {

        public GameManager(InputController ic)
        {
            m_InputCtrl = ic;
            m_Player = new();

            m_InputCtrl.KeyPressedEvent += KeyPressHandler;
            m_InputCtrl.ActionEvent += ActionHandler;
            m_InputCtrl.MovementEvent += MovementHandler;
            m_InputCtrl.MouseClickedEvent += MouseClickHandler;
            m_InputCtrl.MouseMovedEvent += MouseMoveHandler;
            m_InputCtrl.MousePressEvent += MousePressHandler;
            m_InputCtrl.MouseReleaseEvent += MouseReleaseHandler;

            m_Clock = new HE_Clock();

            #if DEBUG
            Console.WriteLine($"[GameManager]: GameManager created");
            #endif
        }


        public void Update()
        {
            m_Tick = m_Clock.Restart().AsSeconds();
            m_Updating = true;

            m_Updating = false;
        }


        private void SetupScene()
        {

        }


        private void KeyPressHandler(object? s, HE_KeyPressedArgs e)
        {
            #if DEBUG
            Console.WriteLine($"[GameManager]: Pressed key {e.Key}");
            #endif
        }


        private void MouseClickHandler(object? s, HE_MouseClickedArgs e)
        {
            #if DEBUG
            Console.WriteLine($"[GameManager]: Mouse clicked with {e.BtnClicked}");
            #endif
        }


        private void MousePressHandler(object? s, HE_MouseMoveEventArgs e)
        {
            #if DEBUG
            Console.WriteLine($"[GameManager]: Mouse pressed at {e.X}, {e.Y}");
            #endif
        }


        private void MouseReleaseHandler(object? s, HE_MouseMoveEventArgs e)
        {
            #if DEBUG
            Console.WriteLine($"[GameManager]: Mouse released at {e.X}, {e.Y}");
            #endif
        }


        private void MouseMoveHandler(object? s, HE_MouseMoveEventArgs e)
        {
            #if DEBUG
            //Console.WriteLine($"[GameManager]: Mouse moved");
            #endif
        }


        private void MovementHandler(object? s, HE_MovementEventArgs e)
        {
            #if DEBUG
            Console.WriteLine($"[GameManager]: Movement detected: {e.Action} at {e.Tick}");
            #endif
        }


        private void ActionHandler(object? s, HE_ActionEventArgs e)
        {
            #if DEBUG
            Console.WriteLine($"[GameManager]: Action detected: {e.Action}");
            #endif
        }


        ~GameManager()
        {
            m_Entities.Clear();
        }


        public InputController InputController => m_InputCtrl;
        public static float GetTick => m_Tick;

        private HE_Clock m_Clock;
        private InputController m_InputCtrl;
        private HashSet<Entity> m_Entities = new HashSet<Entity>(10);
        private List<Scene> m_Scenes = new List<Scene>(1);
        private Player m_Player;
        private static float m_Tick = 0f;
        private bool m_Updating = false;

    }


    internal readonly record struct HE_InteractInfo
    {

        public HE_InteractInfo(PlayerData playerStats, Entity? callingEnt = null)
        {
            PlayerStats = playerStats;
            Caller = callingEnt;
        }

        public readonly Entity? Caller = null;
        public readonly PlayerData PlayerStats;

    }


    internal readonly record struct HE_ClickInfo
    {

        public HE_ClickInfo(PlayerData playerStats)
        {
            PlayerStats = playerStats;
        }

        public readonly PlayerData PlayerStats;

    }

}
