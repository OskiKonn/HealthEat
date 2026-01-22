using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HealthEat
{
    /// <summary>
    /// Base class for all game levels. Manages scene creation and lifecycle.
    /// </summary>
    internal class Level
    {

        /// <summary>
        /// Initializes a new level with the specified name and supervisor.
        /// </summary>
        /// <param name="name">The name identifier for this level.</param>
        /// <param name="supervisor">The level supervisor managing this level.</param>
        /// <param name="hasPlayer">Whether this level contains a player entity.</param>
        public Level(string name, LevelSupervisor supervisor, bool hasPlayer = false)
        {
            //m_Supervisor = supervisor;
            m_PlayerPresent = hasPlayer;
            m_Name = name;
            m_SceneManager = new SceneManager();
            m_Supervisor = supervisor;

            #if HE_DEBUG
            Console.WriteLine($"[Level]: Created level {name}");
            #endif
        }


        /// <summary>
        /// Updates the level's scene manager and handles cleanup if the level is closing.
        /// </summary>
        /// <param name="dt">Delta time in seconds since last frame.</param>
        public virtual void Update(float dt)
        {
            //m_InteractionSupervisor.Update();
            //m_Player?.Update();
            m_SceneManager?.Update(dt);

            if (m_ToClose)
                m_SceneManager?.DestroyScenes();
        }

        /// <summary>
        /// Draws all scenes in the level to the window.
        /// </summary>
        /// <param name="wnd">The window to draw to.</param>
        public void Draw(Window wnd)
        {
            m_SceneManager?.Draw(wnd);
        }

        /// <summary>
        /// Virtual method to be overridden by derived classes to load level-specific content.
        /// </summary>
        public virtual void Load()
        {


        }


        //public Scene? GetScene(string name)
        //{
        //    return m_SceneManager.GetSceneByName(name);
        //}


        //public Scene? GetActiveScene()
        //{
        //    return m_SceneManager.AcitveScene;
        //}


        //public virtual void InjectPlayer(Player player)
        //{
        //    SceneLayer? l = m_SceneManager.GetSceneByName("Test scene")?.GetLayerByName("Test layer");

        //    if (l == null) return;

        //    l.AddEntityToLayer(player);
        //    player.SetDebugMode(true);

        //}


        /// <summary>
        /// Closes the level and cleans up all resources. Destroys all scenes.
        /// </summary>
        public virtual void Close()
        {
            m_PlayerLevelInfo.Scene = null;
            m_PlayerLevelInfo.Layer = null;
            m_SceneManager?.DestroyScenes();
        }

        /// <summary>
        /// Determines whether the specified object is equal to this level.
        /// </summary>
        /// <param name="obj">The object to compare with this level.</param>
        /// <returns>True if the object is a level with the same name, otherwise false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is Level l && l.Name == m_Name;
        }

        /// <summary>
        /// Returns a hash code for this level based on its name.
        /// </summary>
        /// <returns>A hash code for the current level.</returns>
        public override int GetHashCode()
        {
            return ("Level_" + m_Name).GetHashCode();
        }


        ~Level()
        {
            //m_SceneManager?.DestroyScenes();
            m_ShowPopup = false;
        }


        /// <summary>
        /// Gets the name of this level.
        /// </summary>
        public string Name => m_Name;
        /// <summary>
        /// Gets information about the player's position in this level.
        /// </summary>
        public HE_PlayerInLevelInfo GetPlayerLevelInfo => m_PlayerLevelInfo;

        protected LevelSupervisor m_Supervisor;
        protected SceneManager m_SceneManager;
        protected Player? m_Player;
        //private SceneLayer? m_PlayerLayer = null;
        //private Scene? m_PlayerScene = null;
        protected HE_PlayerInLevelInfo m_PlayerLevelInfo;
        protected bool m_ShowPopup = false;
        protected bool m_PlayerPresent = false;
        protected readonly string m_Name = "";

        private bool m_ToClose = false;

    }


    /// <summary>
    /// Structure containing information about the player's scene and layer in a level.
    /// </summary>
    internal struct HE_PlayerInLevelInfo
    {
        /// <summary>
        /// The scene containing the player.
        /// </summary>
        public Scene? Scene;
        /// <summary>
        /// The layer containing the player.
        /// </summary>
        public SceneLayer? Layer;
    }

}
