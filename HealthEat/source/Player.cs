using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HealthEat.Exceptions;

namespace HealthEat
{
    internal class Player : KinematicEntity
    {

        public Player() : base(new HE_Vec2(50f, 50f), "Player", "star.png", 1)
        {
            //m_Velocity = 180f;
            //Acceleration = 200f;
            m_MoveSpeed = 160f;

            #if HE_DEBUG
            Console.WriteLine($"[Player]: Created player");
            #endif
        }

        public override void Update()
        {
            Velocity = new HE_Vec2(0f, 0f);

            if (InputController.Get.IsActionHeld(HE_Action.MoveUp))
                m_Velocity.Y -= m_MoveSpeed;

            if (InputController.Get.IsActionHeld(HE_Action.MoveDown))
                m_Velocity.Y += m_MoveSpeed;

            if (InputController.Get.IsActionHeld(HE_Action.MoveLeft))
                m_Velocity.X -= m_MoveSpeed;

            if (InputController.Get.IsActionHeld(HE_Action.MoveRight))
                m_Velocity.X += m_MoveSpeed;

        }


        public void SetHealth(int hp)
        {
            if (hp > 100u)
                throw new HE_InvalidArgumentValueException($"[Player]: Maximum health overflow");

            m_PlayerStats.Health = hp;
        }


        public void UpdateHealth(int val)
        {
            int hp = m_PlayerStats.Health + val;

            if (hp > 100 || hp < 0)
                hp = 100;

            SetHealth(hp);
        }


        public void SetScore(int score)
        {
            m_PlayerStats.Score = score;
        }


        public void UpdateScore(int val)
        {
            SetScore(m_PlayerStats.Score + val);
        }


        public void UpdateEnergy(int val)
        {
            int eng = m_PlayerStats.Energy + val;

            if (eng > 100 || eng < 0)
                eng = 100;

            SetEnergy(eng);
        }


        public void SetEnergy(int energy)
        {
            if (energy > 100 || energy < 0)
                throw new HE_InvalidArgumentValueException($"[Player]: Maximum energy overflow");

            m_PlayerStats.Energy = energy;
        }

        public PlayerData Data => m_PlayerStats;
        public override HE_EntityType EntityType => HE_EntityType.Player;
        //public HE_FloatRect Body => Body;

        private PlayerData m_PlayerStats;
    }


    internal struct PlayerData
    {
        public int Health;
        public int Score;
        public int Energy;
    }
}
