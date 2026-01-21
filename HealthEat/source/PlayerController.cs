using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthEat
{
    internal class PlayerController
    {

        public PlayerController()
        {
            m_Player = new Player();

            #if HE_DEBUG
            Console.WriteLine($"[PlayerController]: PlayerController created");
            #endif
        }


        public void Update(float dt)
        {
            m_Player.MovePlayer(ref m_MovementFlags, dt);
        }


        public void UpdateFlags(HE_MovementEvent e)
        {
            if (e.KeyAction == HE_KeyAction.Pressed)
                m_MovementFlags |= e.Action;

            else if (e.KeyAction == HE_KeyAction.Released && e.Action != HE_MovementFlags.Jump)
                m_MovementFlags &= ~e.Action;
        }


        public static uint GetPlayerIndex => m_Player.Index;
        public static IReadOnlyEntity GetPlayer => (IReadOnlyEntity)m_Player;

        private static Player m_Player = null!;
        private HE_MovementFlags m_MovementFlags = HE_MovementFlags.NoMove;

    }


    [Flags]
    internal enum HE_MovementFlags : int
    {
        NoMove = 0x00,
        MoveLeft = 0x01,
        MoveRight = 0x02,
        MoveUp = 0x04,
        MoveDown = 0x08,
        Jump = 0x10
    }
}
