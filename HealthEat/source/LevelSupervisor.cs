using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HealthEat.Exceptions;

namespace HealthEat
{
    internal class LevelSupervisor
    {
        public LevelSupervisor(GameManager manager)
        {
            m_GameManager = manager;

            Level l = new Level("Test level", this, true);
            m_Levels.Add(l);
            //m_ActiveLevel = l;
        }


        public void Update(float dt)
        {
            m_ActiveLevel?.Update(dt);
        }

        public void Draw(Window wnd)
        {
            m_ActiveLevel?.Draw(wnd);
        }

        public void LoadLevel(string levelName)
        {
            Level? level = GetLevel(levelName);

            if (level == null)
                throw new HE_InvalidArgumentValueException($"[LevelSupervisor]: Level {levelName} doesn't exist");

            if (m_ActiveLevel != null)
                CloseLevel();

            level.Load();
            m_ActiveLevel = level;
        }

        
        private Level? GetLevel(string name)
        {
            foreach (Level level in m_Levels)
            {
                if (level.Name == name) return level;
            }

            return null;
        }


        public void CloseLevel()
        {
            if (m_ActiveLevel == null)
                throw new HE_LogicException($"[LevelSupervisor]: Tried closing level when there is no loaded one", HE_ExceptionType.Critical);
        
            m_ActiveLevel.Close();
            m_ActiveLevel = null;
        }

        public GameManager GetSupervisorManager => m_GameManager;

        private HashSet<Level> m_Levels = new HashSet<Level>(1);
        private Level? m_ActiveLevel = null;
        private GameManager m_GameManager;

    }
}
