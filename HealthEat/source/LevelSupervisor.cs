using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HealthEat.Exceptions;

namespace HealthEat
{
    /// <summary>
    /// Manages level loading, switching, and updates.
    /// Handles the active level lifecycle and player reference.
    /// </summary>
    internal class LevelSupervisor
    {
        /// <summary>
        /// Initializes the LevelSupervisor with a reference to the game manager.
        /// </summary>
        /// <param name="manager">The game manager instance.</param>
        public LevelSupervisor(GameManager manager)
        {
            m_GameManager = manager;
            //Level l = new Level("Test level", this, true);
            //MenuLevel ml = new MenuLevel(this);
            //HouseLevel hl = new HouseLevel(this);
            //KitchenLevel kl = new KitchenLevel(this);
            //m_Levels.Add(l);
            //m_Levels.Add(ml);
            //m_Levels.Add(hl);
            //m_Levels.Add(kl);
            //m_ActiveLevel = l;
        }


        /// <summary>
        /// Updates the active level and handles level switching.
        /// </summary>
        /// <param name="dt">Delta time in seconds since last frame.</param>
        public void Update(float dt)
        {
            m_ActiveLevel?.Update(dt);

            if (m_PendingLevel != null)
            {
                SwitchLevel();
            }
        }

        /// <summary>
        /// Draws the active level to the window.
        /// </summary>
        /// <param name="wnd">The window to draw to.</param>
        public void Draw(Window wnd)
        {
            m_ActiveLevel?.Draw(wnd);
        }

        /// <summary>
        /// Queues a level to be loaded. The level will be loaded on the next update cycle.
        /// </summary>
        /// <param name="level">The level instance to load.</param>
        public void LoadLevel(Level level)
        {
            m_PendingLevel = level;
        }
        
        /// <summary>
        /// Gets a level by its name from the registered levels.
        /// </summary>
        /// <param name="name">The name of the level to find.</param>
        /// <returns>The level with the specified name, or null if not found.</returns>
        private Level? GetLevel(string name)
        {
            foreach (Level level in m_Levels)
            {
                if (level.Name == name) return level;
            }

            return null;
        }


        /// <summary>
        /// Closes the currently active level and cleans up its resources.
        /// </summary>
        /// <exception cref="HE_LogicException">Thrown when attempting to close a level when none is loaded.</exception>
        public void CloseLevel()
        {
            if (m_ActiveLevel == null)
                throw new HE_LogicException($"[LevelSupervisor]: Tried closing level when there is no loaded one", HE_ExceptionType.Critical);
        
            m_ActiveLevel.Close();
            m_ActiveLevel = null;
        }

        private void SwitchLevel()
        {
            if (m_ActiveLevel != null)
            {
                m_ActiveLevel.Close();
            }

            m_ActiveLevel = m_PendingLevel;
            m_ActiveLevel.Load();
            HE_EventBus.Get.BroadcastEvent(new HE_PlayerDataChangedEvent(m_GlobalPlayer.Data));
            m_PendingLevel = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Gets the global player instance.
        /// </summary>
        /// <returns>The player instance.</returns>
        public Player GetPlayer() => m_GlobalPlayer;
        /// <summary>
        /// Gets the game manager instance.
        /// </summary>
        public GameManager GetSupervisorManager => m_GameManager;

        private HashSet<Level> m_Levels = new HashSet<Level>(1);
        private Level? m_ActiveLevel = null;
        private Level? m_PendingLevel = null;
        private GameManager m_GameManager;
        private Player m_GlobalPlayer = new Player();

    }
}
