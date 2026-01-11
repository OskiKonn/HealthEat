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
    internal class Scene
    {
        public Scene(string name = "Unnamed")
        {
            m_Name = name;
            m_id = SceneManager.Get.ObtainID();
        }


        public void PushLayer(SceneLayer layer)
        {
            m_LayerStack.Add(layer);
            layer.SetMasterScene(this);
            m_LayerCount++;
        }


        public void PopLayer()
        {
            SceneLayer sceneToRemove = m_LayerStack[^1];
            sceneToRemove.SetMasterScene(null);

            m_LayerStack.Remove(sceneToRemove);
            m_LayerCount--;
        }


        public bool AddToScene(IRenderable obj, int nLayer = 0)
        {
            if (m_LayerCount == 0)
            {
                #if DEBUG
                Console.WriteLine($"[Scene_{m_Name}]: Failed adding object to layer. LayerCount is zero");
                #endif

                return false;
            }

            if (nLayer > m_LayerCount || nLayer < 0)
            {
                #if DEBUG
                Console.WriteLine($"[Scene_{m_Name}]: Failed adding object to layer {nLayer}. Layer doesn't exist");
                #endif

                return false;
            }
            else if (nLayer == 0)
            {
                SceneLayer topLayer = m_LayerStack[^1];
                return topLayer.AddToLayer(obj);
            }

            SceneLayer layer = m_LayerStack[nLayer - 1];
            return layer.AddToLayer(obj);
        }


        public bool RemoveFromScene(IRenderable obj, int nLayer = 0)
        {
            if (m_LayerCount == 0)
            {
                #if DEBUG
                Console.WriteLine($"[Scene_{m_Name}]: Failed removing object from layer. LayerCount is zero");
                #endif

                return false;
            }

            if (nLayer > m_LayerCount || nLayer < 0)
            {
                #if DEBUG
                Console.WriteLine($"[Scene_{m_Name}]: Failed removing object from layer {nLayer}. Layer doesn't exist");
                #endif

                return false;
            }
            else if (nLayer == 0)
            {
                SceneLayer topLayer = m_LayerStack[^1];
                return topLayer.RemoveFromLayer(obj);
            }

            SceneLayer layer = m_LayerStack[nLayer - 1];
            return layer.RemoveFromLayer(obj);
        }


        ~Scene()
        {
            LayerStack.Clear();
        }

        public string Name => m_Name;
        public int ID => m_id;
        public List<SceneLayer> LayerStack => m_LayerStack;
        public int LayerCount => LayerCount;

        private readonly string m_Name;
        private int m_id;
        private int m_LayerCount = 0;
        private List<SceneLayer> m_LayerStack = new List<SceneLayer>(1);
    }

    internal class SceneLayer
    {

        public SceneLayer()
        {
            #if DEBUG
            Console.WriteLine("[Layer]: New SceneLayer created");
            #endif
        }


        public bool AddToLayer(IRenderable renderObject)
        {
            if (!m_LayerObjects.Add(renderObject))
            {

                #if DEBUG
                Console.WriteLine($"[Layer_{m_MasterName}]: Failed adding RenderObject {renderObject.Name} to renderer. Make sure to not duplicate object name");
                #endif
                return false;
            }

            #if DEBUG
            Console.WriteLine($"[Layer_{m_MasterName}]: Added test object:  {{ {renderObject.Name}, {renderObject.ID} }}");
            #endif

            return true;
        }

        public bool RemoveFromLayer(IRenderable layerObject)
        {
            return m_LayerObjects.Remove(layerObject);
        }

        internal void SetMasterScene(Scene? sc, uint layerDepth = 0u)
        {
            m_MasterScene = sc;
            m_MasterName = (sc != null) ? sc.Name : "";
        }

        ~SceneLayer()
        {
            m_LayerObjects.Clear();
            #if DEBUG
            Console.WriteLine($"[Layer_{m_MasterName}]: Layer Destroyed");
            #endif
        }

        public HashSet<IRenderable> LayerObjects => m_LayerObjects;
        public Scene? MasterScene => m_MasterScene;
        public string MasterName => m_MasterName;

        private HashSet<IRenderable> m_LayerObjects = new HashSet<IRenderable>(10);
        private Scene? m_MasterScene = null;
        private string m_MasterName = "";
    }

}
