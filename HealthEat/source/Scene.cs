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
    internal class Scene
    {
        public Scene(string name = "Unnamed")
        {
            m_Name = name;
            m_id = SceneManager.Get.ObtainID();
        }

        public bool AddToScene(IRenderable renderObject)
        {
            if (!m_SceneObjects.Add(renderObject))
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

        public bool DeleteFromScene(IRenderable sceneObject)
        {
            return m_SceneObjects.Remove(sceneObject);
        }

        public string Name => m_Name;
        public int ID => m_id;
        public HashSet<IRenderable> SceneObjects => m_SceneObjects;

        private readonly string m_Name;
        private int m_id;
        private HashSet<IRenderable> m_SceneObjects = new HashSet<IRenderable>(10);
    }
}
