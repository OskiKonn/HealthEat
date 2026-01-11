using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HealthEat.Exceptions;

namespace HealthEat
{
    internal class Player : Entity
    {

        public Player() : base(new HE_Vec2(50f, 50f), "Player", "star.png", 1, true, true)
        {
            m_Velocity = 100f;

            #if DEBUG
            Console.WriteLine($"[Player]: Created player");
            #endif
        }


        public void MovePlayer(HE_MovementEventArgs ma)
        {
            float tick = ma.Tick;

            switch (ma.Action)
            {
                case HE_MovementFlags.Jump:
                    m_MakeJump = m_MakeJump ? m_MakeJump : true;
                    if (m_MakeJump) Jump(tick);
                    break;

                case HE_MovementFlags.MoveUp:
                    MoveUp(tick);
                    break;

                case HE_MovementFlags.MoveDown:
                    MoveDown(tick);
                    break;

                case HE_MovementFlags.MoveLeft:
                    MoveLeft(tick);
                    break;

                case HE_MovementFlags.MoveRight:
                    MoveRight(tick);
                    break;
            }
        }


        public void Jump(float dt)
        {

            if (m_JumpedDistance < m_JumpHeight && !m_JumpPeak)
            {
                m_IsGrounded = false;
                Move(0, -1, dt);
                m_JumpedDistance++;
                return;
            }
            else if (m_JumpedDistance == m_JumpHeight)
            {
                m_JumpPeak = true;
            }
            else if (m_JumpPeak && m_JumpedDistance > 0)
            {
                Move(0, 1, dt);
                m_JumpedDistance--;
            }
            else if (m_JumpPeak && m_JumpedDistance == 0)
            {
                m_JumpPeak = false;
                m_MakeJump = false;
                m_IsGrounded = true;
            }

        }


        public void MoveLeft(float dt)
        {
            Move(-1, 0, dt);
        }


        public void MoveRight(float dt)
        {
            Move(1, 0, dt);
        }


        public void MoveUp(float dt)
        {
            if (!m_IsGrounded) return;
            Move(0, -1, dt);
        }


        public void MoveDown(float dt)
        {
            if (!m_IsGrounded) return;
            Move(0, 1, dt);
        }


        public void SetHealth(uint hp)
        {
            if (hp > 100u)
                throw new HE_InvalidArgumentValueException($"[Player]: Maximum health overflow");

            m_PlayerStats.Health = hp;
        }


        public void AddHealth(uint val)
        {
            uint hp = m_PlayerStats.Health + val;

            if (hp > 100u)
                hp = 100u;

            SetHealth(hp);
        }


        public void SetScore(uint score)
        {
            m_PlayerStats.Score = score;
        }


        public void AddScore(uint val)
        {
            SetScore(m_PlayerStats.Score + val);
        }


        public void AddEnergy(uint val)
        {
            uint eng = m_PlayerStats.Energy + val;

            if (eng > 100u)
                eng = 100u;

            SetEnergy(eng);
        }


        public void SetEnergy(uint energy)
        {
            if (energy > 100u)
                throw new HE_InvalidArgumentValueException($"[Player]: Maximum energy overflow");

            m_PlayerStats.Energy = energy;
        }

        public PlayerData Data => m_PlayerStats;

        private PlayerData m_PlayerStats;
        private bool m_IsGrounded = true;
        private bool m_MakeJump = false;
        private bool m_JumpPeak = false;
        private int m_JumpedDistance = 0;
        private int m_JumpHeight = 10;
    }


    internal struct PlayerData
    {
        public uint Health;
        public uint Score;
        public uint Energy;
    }
}
