using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFG = SFML.Graphics;
using SFS = SFML.System;
using SFW = SFML.Window;


namespace HealthEat.Rendering
{
    internal class Renderer
    {

        private static readonly Lazy<Renderer> s_Instance = new Lazy<Renderer>(() => new Renderer(), false);

        private Renderer() { }

        public void Render()
        {

            if (SceneManager.Get.AcitveScene == null)
            {
                Console.WriteLine("Null active scene!!!");
                return;
            }

            foreach (IRenderable obj in SceneManager.Get.AcitveScene.SceneObjects)
            {
                Window.ActiveWindow.Draw(obj.DrawingContext);
                //Console.WriteLine("Name: " + obj.Name + ", Id: " + obj.ID);
            }
        }

        

        public static Renderer Get => s_Instance.Value;
        //private HashSet<Scene> m_RenderObjects = new HashSet<IRenderable>(10);
    }
}
