using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFG = SFML.Graphics;
using SFS = SFML.System;
using SFW = SFML.Window;


namespace HealthEat
{
    internal class Renderer
    {

        private static readonly Lazy<Renderer> s_Instance = new Lazy<Renderer>(() => new Renderer(), false);
        private Renderer() { }


        public void Render()
        {

            Scene? activeScene = SceneManager.Get.AcitveScene;

            if (activeScene == null)
            {
                Console.WriteLine("Null active scene!!!");
                return;
            }

            foreach (SceneLayer layer in activeScene.LayerStack)
            {
                foreach(IRenderable obj in layer.LayerObjects)
                    Window.ActiveWindow.Draw(obj.DrawingContext);
            }

        }

        

        public static Renderer Get => s_Instance.Value;
        //private HashSet<Scene> m_RenderObjects = new HashSet<IRenderable>(10);
    }
}
