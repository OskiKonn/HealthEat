using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFG = SFML.Graphics;
using SFW = SFML.Window;
using SFS = SFML.System;

namespace HealthEat.Rendering
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

        public Window() : this(new WindowSpecification()) { }

        public Window(WindowSpecification winSpec)
        {
            m_WinSpec = winSpec;

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
            sm_RenderWindow.DispatchEvents();
        }

        public void Update()
        {
            sm_RenderWindow.Clear(SFG.Color.Blue);
            sm_RenderWindow.Display();
        }

        public void Test()
        {
            sm_RenderWindow.Clear(SFG.Color.White);
            TextObject textObj = new TextObject("Test text", 48, "test1");
            textObj.Context.Position = new SFS.Vector2f(100f, 100f);
            Renderer.Get.AddRenderObject(textObj);
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
            sm_RenderWindow.KeyPressed += (object? s, SFW.KeyEventArgs ke) => { Console.WriteLine(ke.Scancode); };
            sm_RenderWindow.Resized += OnResize;
        }

        public bool IsOpen => sm_RenderWindow.IsOpen;
        public static SFG.RenderWindow ActiveWindow => sm_RenderWindow;

        private WindowSpecification m_WinSpec;
        private static SFG.RenderWindow sm_RenderWindow = null!;
        private SFG.View m_View;
    }
}
