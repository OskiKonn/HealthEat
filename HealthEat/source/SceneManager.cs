using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFG = SFML.Graphics;
using SFS = SFML.System;
using SFW = SFML.Window;

namespace HealthEat
{
    /// <summary>
    /// Manages multiple scenes, their lifecycle, and active scene selection.
    /// </summary>
    internal class SceneManager
    {
        //private static readonly Lazy<SceneManager> s_Instance = new Lazy<SceneManager>(() => new SceneManager(), false);

        /// <summary>
        /// Initializes a new SceneManager instance.
        /// </summary>
        public SceneManager()
        {
        }

        ~SceneManager()
        {
            m_Scenes.Clear();
            m_SceneCount = 0;
            m_ActiveScene = null;
        }

        /// <summary>
        /// Updates all registered scenes.
        /// </summary>
        /// <param name="dt">Delta time in seconds since last frame.</param>
        public void Update(float dt)
        {
            foreach (Scene s in m_Scenes)
            {
                s.Update(dt);
            }

        }

        /// <summary>
        /// Draws all registered scenes to the window.
        /// </summary>
        /// <param name="wnd">The window to draw to.</param>
        public void Draw(Window wnd)
        {
            foreach(Scene s in m_Scenes)
            {
                s.Draw(wnd);
            }
        }

        /// <summary>
        /// Destroys all registered scenes and clears the scene list.
        /// </summary>
        public void DestroyScenes()
        {
            m_ActiveScene = null;

            foreach (Scene sc in m_Scenes)
            {
                sc.Clear();
            }

            m_Scenes.Clear();

            m_SceneCount = 0;
        }

        /// <summary>
        /// Unregisters a scene from the scene manager.
        /// </summary>
        /// <param name="sc">The scene to unregister.</param>
        public void UnregisterScene(Scene sc)
        {
            if (m_ActiveScene == sc)
                m_ActiveScene = null;

            if (m_Scenes.Remove(sc))
            {
                m_SceneCount--;
            }
        }

        /// <summary>
        /// Registers a new scene with the scene manager.
        /// </summary>
        /// <param name="sc">The scene to register.</param>
        /// <returns>True if the scene was successfully registered, false otherwise.</returns>
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

        /// <summary>
        /// Obtains a unique ID for a new scene.
        /// </summary>
        /// <returns>The next available scene ID.</returns>
        public int ObtainID()
        {
            return m_SceneCount;
        }

        /// <summary>
        /// Sets the active scene by scene instance.
        /// </summary>
        /// <param name="s">The scene to set as active.</param>
        public void SetActiveScene(Scene s)
        {
            if (s.ID < 0 || s.ID > m_SceneCount + 1)
                return;

            m_ActiveScene = m_Scenes[s.ID];
        }

        /// <summary>
        /// Sets the active scene by scene name.
        /// </summary>
        /// <param name="sname">The name of the scene to set as active.</param>
        public void SetActiveScene(string sname)
        {
            foreach (Scene s in m_Scenes)
            {
                if (s.Name == sname)
                    m_ActiveScene = s;
            }
        }


        public Scene? GetSceneByName(string name)
        {
            foreach (Scene s in m_Scenes)
            {
                if (s.Name == name)
                    return s;
            }

            return null;
        }


        //public SceneLayer? GetPlayerLayer()
        //{
        //    if (m_ActiveScene != null)
        //    {
        //        return m_ActiveScene.GetPlayerLayer;
        //    }

        //    return null;
        //}


        //public static SceneManager Get => s_Instance.Value;
        public Scene? AcitveScene => m_ActiveScene;
        public List<Scene> Scenes => m_Scenes;
        public int SceneCount => m_SceneCount;

        private List<Scene> m_Scenes = new List<Scene>(1);
        private int m_SceneCount = 0;
        private Scene? m_ActiveScene;
    }
}
