
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFG = SFML.Graphics;
using SFW = SFML.Window;
using SFS = SFML.System;
using SFML.Window;


namespace HealthEat
{
    /// <summary>
    /// Structure containing window creation parameters.
    /// </summary>
    internal struct WindowSpecification
    {
        /// <summary>
        /// Window width in pixels. Default is 800.
        /// </summary>
        public uint width = 800;
        /// <summary>
        /// Window height in pixels. Default is 600.
        /// </summary>
        public uint height = 600;
        /// <summary>
        /// Window title text. Can be null.
        /// </summary>
        public string? title;
        /// <summary>
        /// Whether to enable vertical synchronization. Default is false.
        /// </summary>
        public bool VSync = false;
        /// <summary>
        /// Whether the window can be resized. Default is true.
        /// </summary>
        public bool resizable = true;

        public WindowSpecification() { }

        public WindowSpecification(string title)
        {
            this.title = title;
        }

        public WindowSpecification(uint width, uint height, string? title)
        {
            this.width = width;
            this.height = height;
            
            if (!string.IsNullOrEmpty(title))
            {
                this.title = title;
            }
        }
    }

    /// <summary>
    /// Wrapper class for SFML RenderWindow.
    /// Manages window creation, event handling, and rendering.
    /// </summary>
    internal class Window
    {

        /// <summary>
        /// Initializes a new window with default specifications.
        /// </summary>
        /// <param name="controller">The input controller to use (deprecated).</param>
        public Window(InputController controller) : this(new WindowSpecification()) { }

        /// <summary>
        /// Initializes a new window with the specified specifications.
        /// </summary>
        /// <param name="winSpec">The window specifications to use.</param>
        public Window(WindowSpecification winSpec)
        {
            m_WinSpec = winSpec;
            //m_Clock = new HE_Clock();
            //m_InputCtrl = controller;
            //m_InputCtrl = ic;

            SFW.VideoMode videoMode = new SFW.VideoMode(m_WinSpec.width, m_WinSpec.height);
            SFW.Styles winStyles = SFW.Styles.Titlebar | SFW.Styles.Close;

            if (m_WinSpec.resizable)
                winStyles |= SFW.Styles.Resize;

            m_RenderWindow = new SFG.RenderWindow(videoMode, m_WinSpec.title, winStyles);
            m_RenderWindow.SetVerticalSyncEnabled(m_WinSpec.VSync);
            m_View = m_RenderWindow.GetView();
            SubscribeEventHandlers();

        }

        ~Window()
        {
            m_RenderWindow.Dispose();
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        public void Close()
        {
            m_RenderWindow.Close();
        }

        /// <summary>
        /// Handles all pending window events and clears the render buffer.
        /// </summary>
        public void HandleEvents()
        {
            // m_Tick = m_Clock.Restart().AsSeconds();
            m_RenderWindow.Clear(HE_Color.White);
            m_RenderWindow.DispatchEvents();
        }

        /// <summary>
        /// Displays the rendered frame to the window.
        /// </summary>
        public void Update()
        {
            // HandleEvents();
            //Renderer.Get.Render();
            m_RenderWindow.Display();
        }

        /// <summary>
        /// Test method for rendering (used for debugging).
        /// </summary>
        public void Test()
        {
            m_RenderWindow.Clear(HE_Color.White);
            

            m_RenderWindow.Display();
        }

        private void OnResize(object? sender, SFW.SizeEventArgs se)
        {
            m_View = new SFG.View(new SFG.FloatRect(0, 0, se.Width, se.Height));
            m_RenderWindow.SetView(m_View);
        }

        private void SubscribeEventHandlers()
        {
            m_RenderWindow.Closed += (object? s, EventArgs e) => Close();
            m_RenderWindow.Resized += OnResize;
            m_RenderWindow.KeyPressed += InputController.Get.HandleKeyPress;
            m_RenderWindow.KeyReleased += InputController.Get.HandleKeyRelease;
            m_RenderWindow.MouseButtonPressed += InputController.Get.HandleMousePressedEvent;
            m_RenderWindow.MouseButtonReleased += InputController.Get.HandleMouseReleasedEvent;
            m_RenderWindow.MouseMoved += InputController.Get.HandleMouseMovedEvent;
            m_RenderWindow.MouseWheelScrolled += InputController.Get.HandleMouseScrollEvent;
            // m_InputCtrl.MovementEvent += (object? s, HE_MovementEventArgs e) => { Console.WriteLine($"Movement: {e.Action}"); };
        }


        /// <summary>
        /// Gets whether the window is currently open.
        /// </summary>
        public bool IsOpen => m_RenderWindow.IsOpen;
        //public static SFG.RenderWindow ActiveWindow => m_RenderWindow;
        //public static float GetTick => m_Tick;
        /// <summary>
        /// Gets the underlying SFML RenderWindow instance.
        /// </summary>
        public HE_RenderWindow RenderWindow => m_RenderWindow;

        private readonly InputController m_InputCtrl;
        private WindowSpecification m_WinSpec;
        private SFG.RenderWindow m_RenderWindow = null!;
        private SFG.View m_View;
        //private HE_Clock m_Clock;
        //private static float m_Tick = 0f;
        //private readonly InputController m_InputCtrl;
    }
}
