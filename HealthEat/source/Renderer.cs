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
            foreach (IRenderable obj in m_RenderObjects)
            {
                Window.ActiveWindow.Draw(obj.DrawingContext);
                Console.WriteLine("Name: " + obj.Name + ", Id: " + obj.ID);
            }
        }

        public bool AddRenderObject(IRenderable renderObject)
        {
            if (!m_RenderObjects.Add(renderObject))
            {

        #if DEBUG
                Console.WriteLine($"Failed adding RenderObject {renderObject.Name} to renderer. Make sure to not duplicate object name");
        #endif
                return false;
            }

        #if DEBUG
            Console.WriteLine("Added test object: { " + renderObject.Name + ", " + renderObject.ID + " }");
        #endif
            return true;
        }

        public static Renderer Get => s_Instance.Value;
        private HashSet<IRenderable> m_RenderObjects = new HashSet<IRenderable>(10);
    }
}
