using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFG = SFML.Graphics;
using SFS = SFML.System;
using SFW = SFML.Window;

using HealthEat.Exceptions;

namespace HealthEat
{
    internal class Renderer
    {

        private static readonly Lazy<Renderer> s_Instance = new Lazy<Renderer>(() => new Renderer(), false);
        private Renderer() { }


        public void Render(List<Scene> sceneList)
        {


            //if (SceneToRender == null)
            //{
            //    throw new HE_LogicException("[Renderer]: There is no active scene", HE_ExceptionType.Critical);
            //}

            //foreach (SceneLayer layer in SceneToRender.LayerStack)
            //{
            //    foreach(IRenderable obj in layer.LayerObjects)
            //    {
            //        if (obj.Visible)
            //            Window.ActiveWindow.Draw(obj.DrawingContext);
            //    }
            //}


        }

        

        public static Renderer Get => s_Instance.Value;
        public static Scene? SceneToRender { get; set; }
        //private HashSet<Scene> m_RenderObjects = new HashSet<IRenderable>(10);
    }
}
