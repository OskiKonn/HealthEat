
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
    internal struct WindowSpecification
    {
        public uint width = 800;
        public uint height = 600;
        public string? title;
        public bool VSync = false;
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

    internal class Window
    {

        public Window(GameManager manager) : this(new WindowSpecification(), manager) { }

        public Window(WindowSpecification winSpec, GameManager manager)
        {
            m_WinSpec = winSpec;
            m_Clock = new HE_Clock();
            m_GameManager = manager;
            //m_InputCtrl = ic;

            SFW.VideoMode videoMode = new SFW.VideoMode(m_WinSpec.width, m_WinSpec.height);
            SFW.Styles winStyles = SFW.Styles.Titlebar | SFW.Styles.Close;

            if (m_WinSpec.resizable)
                winStyles |= SFW.Styles.Resize;

            sm_RenderWindow = new SFG.RenderWindow(videoMode, m_WinSpec.title, winStyles);
            sm_RenderWindow.SetVerticalSyncEnabled(m_WinSpec.VSync);
            m_View = sm_RenderWindow.GetView();
            SubscribeEventHandlers();

        }

        ~Window()
        {
            sm_RenderWindow.Dispose();
        }

        public void Close()
        {
            sm_RenderWindow.Close();
        }

        public void HandleEvents()
        {
            m_Tick = m_Clock.Restart().AsSeconds();
            sm_RenderWindow.DispatchEvents();
        }

        public void Update()
        {
            sm_RenderWindow.Clear(HE_Color.Blue);


            sm_RenderWindow.Display();
        }

        public void Test()
        {
            sm_RenderWindow.Clear(HE_Color.White);
            TextObject textObj = new TextObject("Kocham Pati <3", 48, "test1");
            textObj.Context.Position = new SFS.Vector2f(100f, 100f);

            SpriteObject sprObj = new SpriteObject("menubtn.png", "testSprite");
            sprObj.Context.Position = new HE_Vec2(120f, 120f);
            sprObj.Context.Scale = new HE_Vec2(0.5f, 0.5f);

            HE_FloatRect bounds = sprObj.Context.GetGlobalBounds();
            RectangleShapeObject border = new RectangleShapeObject(new HE_Vec2(bounds.Width * 2f, bounds.Height * 2f), "blabla");
            border.Context.FillColor = HE_Color.Transparent;
            border.Context.OutlineColor = HE_Color.Green;
            border.Context.OutlineThickness = 2.0f;
            border.Context.Position = new HE_Vec2(bounds.Left - bounds.Width / 2, bounds.Top - bounds.Height / 2);

            Entity ent = new Entity(new HE_Vec2(280.0f, 280.0f), "Enty", "stats-transp.png", 0.7f);

            SceneLayer layer = new();
            Scene newScene = new Scene("Test scene");
            newScene.PushLayer(layer);
            ent.SetDebugMode(true, newScene);
            layer.AddToLayer(textObj);
            layer.AddToLayer(sprObj);
            layer.AddToLayer(border);
            layer.AddToLayer(ent);
            SceneManager.Get.RegisterScene(newScene);
            ent.Move(new HE_Vec2(300f, 20f));

            Renderer.Get.Render();
            sm_RenderWindow.Display();
        }

        private void OnResize(object? sender, SFW.SizeEventArgs se)
        {
            m_View = new SFG.View(new SFG.FloatRect(0, 0, se.Width, se.Height));
            sm_RenderWindow.SetView(m_View);
        }

        private void SubscribeEventHandlers()
        {
            sm_RenderWindow.Closed += (object? s, EventArgs e) => Close();
            sm_RenderWindow.Resized += OnResize;
            sm_RenderWindow.KeyPressed += m_GameManager.InputController.HandleKbInput;
            sm_RenderWindow.MouseButtonPressed += m_GameManager.InputController.HandleMousePressedEvent;
            sm_RenderWindow.MouseButtonReleased += m_GameManager.InputController.HandleMouseReleasedEvent;
            sm_RenderWindow.MouseMoved += m_GameManager.InputController.HandleMouseMovedEvent;
            // m_InputCtrl.MovementEvent += (object? s, HE_MovementEventArgs e) => { Console.WriteLine($"Movement: {e.Action}"); };
        }


        public bool IsOpen => sm_RenderWindow.IsOpen;
        public static SFG.RenderWindow ActiveWindow => sm_RenderWindow;
        public static float GetTick => m_Tick;

        private readonly GameManager m_GameManager;
        private WindowSpecification m_WinSpec;
        private static SFG.RenderWindow sm_RenderWindow = null!;
        private SFG.View m_View;
        private HE_Clock m_Clock;
        private static float m_Tick = 0f;
        //private readonly InputController m_InputCtrl;
    }
}
