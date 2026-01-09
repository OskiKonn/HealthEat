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
    internal class SceneManager
    {
        private static readonly Lazy<SceneManager> s_Instance = new Lazy<SceneManager>(() => new SceneManager(), false);

        private SceneManager()
        {
            Console.WriteLine("SceneManager created!");
        }

        ~SceneManager()
        {
            m_Scenes.Clear();
            m_SceneCount = 0;
            m_ActiveScene = null;
        }

        public bool RegisterScene(Scene sc)
        {
            m_Scenes.Add(sc);
            
            if (m_Scenes.Count <= m_SceneCount)
            {
                return false;
            }


            if (m_SceneCount == 0)
                SetActiveScene(sc);

            m_SceneCount++;
            return true;
        }

        public int ObtainID()
        {
            return m_SceneCount;
        }

        public void SetActiveScene(Scene s)
        {
            if (s.ID < 0 || s.ID > m_SceneCount + 1)
                return;

            m_ActiveScene = m_Scenes[s.ID];
        }

        public void SetActiveScene(string sname)
        {
            foreach (Scene s in m_Scenes)
            {
                if (s.Name == sname)
                    m_ActiveScene = s;
            }
        }

        public Scene? AcitveScene => m_ActiveScene;

        public static SceneManager Get => s_Instance.Value;
        public List<Scene> Scenes => m_Scenes;
        public int SceneCount => m_SceneCount;

        private List<Scene> m_Scenes = new List<Scene>(1);
        private int m_SceneCount = 0;
        private Scene? m_ActiveScene;
    }
}
