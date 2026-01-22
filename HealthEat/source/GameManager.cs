using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthEat
{
    /// <summary>
    /// Main game manager class that coordinates all game systems.
    /// Handles window creation, input, level management, and the main game loop.
    /// </summary>
    internal class GameManager
    {

        /// <summary>
        /// Initializes the GameManager with input controller, window, and level supervisor.
        /// </summary>
        public GameManager()
        {
            m_InputCtrl = new InputController();
            m_InputCtrl.SetDefaultKeybinds();
            //m_PlayerCtrl = new PlayerController();
            WindowSpecification winSpec = new WindowSpecification()
            {
                title = "HealthEat",
                width = 1024u,
                height = 720u,
                resizable = false,
                VSync = false
            };

            m_GameWindow = new Window(winSpec);

            HE_EventBus.Get.SubscribeToEventForce<HE_ExitEvent>(Quit);
            HE_EventBus.Get.SubscribeToEvent<HE_KeyEvent>(KeyHandler);
            //HE_EventBus.Get.SubscribeToEvent<HE_KeyEvent>(ActionHandler);
            //HE_EventBus.Get.SubscribeToEvent<HE_KeyEvent> MovementHandler;
            HE_EventBus.Get.SubscribeToEvent<HE_MouseClickedEvent>(MouseClickHandler);
            HE_EventBus.Get.SubscribeToEvent<HE_MouseMoveEvent>(MouseMoveHandler);
            HE_EventBus.Get.SubscribeToEvent<HE_MousePressedEvent>(MousePressHandler);
            HE_EventBus.Get.SubscribeToEvent<HE_MouseReleasedEvent>(MouseReleaseHandler);

            m_GameSupervisors.GameManager = this;
            m_GameSupervisors.LevelSupervisor = new LevelSupervisor(this);

            m_Clock = new HE_Clock();
            SetupScene();

            #if HE_DEBUG
            Console.WriteLine($"[GameManager]: GameManager created");
            #endif
        }

        /// <summary>
        /// Runs the main game loop. Handles events, updates game state, and renders frames.
        /// </summary>
        public void Run()
        {
            //m_GameSupervisors.LevelSupervisor.LoadLevel("Test level");

            while (m_GameWindow.IsOpen)
            {
                m_Tick = m_Clock.Restart().AsSeconds();
                m_Updating = true;
                m_GameWindow.HandleEvents();
                InputController.Get.UpdateKeyStates();
                
                // Check for pause/escape key
                if (InputController.Get.IsActionTriggered(HE_Action.Pause))
                {
                    m_GameSupervisors.LevelSupervisor.LoadLevel(new MenuLevel(m_GameSupervisors.LevelSupervisor));
                }
                
                m_GameSupervisors.LevelSupervisor.Update(m_Tick);
                m_GameSupervisors.LevelSupervisor.Draw(m_GameWindow);
                m_GameWindow.Update();
                //m_PlayerCtrl.Update(m_Tick);
                m_Updating = false;
            }
        }


        /// <summary>
        /// Handles the quit event. Closes the current level and game window.
        /// </summary>
        /// <param name="ev">The exit event that triggered this method.</param>
        public void Quit(HE_ExitEvent ev)
        {
            m_GameSupervisors.LevelSupervisor.CloseLevel();
            m_GameWindow.Close();
        }


        private void SetupScene()
        {
            m_GameSupervisors.LevelSupervisor.LoadLevel(new MenuLevel(m_GameSupervisors.LevelSupervisor));
        }


        private void KeyHandler(HE_KeyEvent e)
        {
            if (m_Updating) return;

            #if HE_DEBUG
            //Console.WriteLine($"[GameManager]: {e.KeyAction} key {e.Key}");
            #endif
        }


        private void MouseClickHandler(HE_MouseClickedEvent e)
        {
            if (m_Updating) return;

            #if HE_DEBUG
            //Console.WriteLine($"[GameManager]: Mouse clicked with {e.BtnClicked}");
            #endif
        }


        private void MousePressHandler(HE_MousePressedEvent e)
        {
            if (m_Updating) return;

            #if HE_DEBUG
            //Console.WriteLine($"[GameManager]: Mouse pressed at {e.X}, {e.Y}");
            #endif
        }


        private void MouseReleaseHandler(HE_MouseReleasedEvent e)
        {
            if (m_Updating) return;

            #if HE_DEBUG
            Console.WriteLine($"[GameManager]: Mouse released at {e.X}, {e.Y}");
            #endif
        }


        private void MouseMoveHandler(HE_MouseMoveEvent e)
        {
            if (m_Updating) return;
            
            #if HE_DEBUG
            //Console.WriteLine($"[GameManager]: Mouse moved");
            #endif
        }


        //private void MovementHandler(HE_MovementEvent e)
        //{
        //    if (m_Updating) return;

        //    //m_PlayerCtrl.UpdateFlags(e);

        //}


        private void ActionHandler(HE_ActionEvent e)
        {
            if (m_Updating) return;

            #if HE_DEBUG
            Console.WriteLine($"[GameManager]: Action detected: {e.Action}");
            #endif
        }


        ~GameManager()
        {
            
        }


        /// <summary>
        /// Gets the input controller instance.
        /// </summary>
        public InputController InputController => m_InputCtrl;
        //public Player GetPlayer => m_Player;
        /// <summary>
        /// Gets the current frame time (delta time) in seconds.
        /// </summary>
        public static float GetTick => m_Tick;

        private HE_Clock m_Clock;
        private InputController m_InputCtrl;
        private Window m_GameWindow;
        //private PlayerController m_PlayerCtrl;
        private HE_GameSupervisors m_GameSupervisors;
        private static float m_Tick = 0f;
        private bool m_Updating = false;

    }


    /// <summary>
    /// Container structure holding references to game supervisors.
    /// </summary>
    internal record struct HE_GameSupervisors
    {
        /// <summary>
        /// Reference to the game manager instance.
        /// </summary>
        public GameManager GameManager;
        /// <summary>
        /// Reference to the level supervisor instance.
        /// </summary>
        public LevelSupervisor LevelSupervisor;
    }

    /// <summary>
    /// Event type that signals the application should exit.
    /// </summary>
    internal readonly struct HE_ExitEvent : IHE_EventType
    {
        /// <summary>
        /// Default constructor for exit event.
        /// </summary>
        public HE_ExitEvent() { }
    }
}
