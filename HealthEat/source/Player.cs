using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HealthEat.Exceptions;

namespace HealthEat
{
    /// <summary>
    /// Represents the player entity in the game.
    /// Handles player movement, health, energy, and score management.
    /// </summary>
    internal class Player : KinematicEntity
    {

        /// <summary>
        /// Initializes a new player instance with default position and stats.
        /// </summary>
        public Player() : base(new HE_Vec2(50f, 50f), "Player", "player.png", 1)
        {
            //m_Velocity = 180f;
            //Acceleration = 200f;
            m_MoveSpeed = 160f;
            m_PlayerStats.Health = 100;
            m_PlayerStats.Energy = 100;
            m_PlayerStats.Score = 0;

            HE_EventBus.Get.RegisterEvent<HE_PlayerDataChangedEvent>();
            HE_EventBus.Get.BroadcastEvent<HE_PlayerDataChangedEvent>(new HE_PlayerDataChangedEvent(m_PlayerStats));

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


        /// <summary>
        /// Sets the player's health to a specific value.
        /// </summary>
        /// <param name="hp">The health value to set (0-100).</param>
        /// <exception cref="HE_InvalidArgumentValueException">Thrown when health exceeds maximum value of 100.</exception>
        public void SetHealth(int hp)
        {
            if (hp > 100u)
                throw new HE_InvalidArgumentValueException($"[Player]: Maximum health overflow");

            m_PlayerStats.Health = hp;
            HE_EventBus.Get.BroadcastEvent(new HE_PlayerDataChangedEvent(m_PlayerStats));
        }

        /// <summary>
        /// Updates the player's health by adding or subtracting a value.
        /// Health is clamped between 0 and 100.
        /// </summary>
        /// <param name="val">The amount to change health by (positive to increase, negative to decrease).</param>
        public void UpdateHealth(int val)
        {
            int hp = m_PlayerStats.Health + val;

            if (hp > 100)
                hp = 100;

            else if (hp < 0)
                hp = 0;

                SetHealth(hp);
        }

        /// <summary>
        /// Sets the player's score to a specific value.
        /// </summary>
        /// <param name="score">The score value to set.</param>
        public void SetScore(int score)
        {
            m_PlayerStats.Score = score;
            HE_EventBus.Get.BroadcastEvent(new HE_PlayerDataChangedEvent(m_PlayerStats));
        }

        /// <summary>
        /// Updates the player's score by adding a value.
        /// Score cannot go below 0.
        /// </summary>
        /// <param name="val">The amount to change score by.</param>
        public void UpdateScore(int val)
        {
            int score = m_PlayerStats.Score + val;

            if (score < 0)
                score = 0;

            SetScore(m_PlayerStats.Score + val);
        }

        /// <summary>
        /// Updates the player's energy by adding or subtracting a value.
        /// Energy is clamped between 0 and 100.
        /// </summary>
        /// <param name="val">The amount to change energy by (positive to increase, negative to decrease).</param>
        public void UpdateEnergy(int val)
        {
            int eng = m_PlayerStats.Energy + val;

            if (eng > 100)
                eng = 100;

            else if (eng < 0)
                eng = 0;

                SetEnergy(eng);
        }

        /// <summary>
        /// Sets the player's energy to a specific value.
        /// </summary>
        /// <param name="energy">The energy value to set (0-100).</param>
        /// <exception cref="HE_InvalidArgumentValueException">Thrown when energy is outside the valid range of 0-100.</exception>
        public void SetEnergy(int energy)
        {
            if (energy > 100 || energy < 0)
                throw new HE_InvalidArgumentValueException($"[Player]: Maximum energy overflow");

            m_PlayerStats.Energy = energy;
            HE_EventBus.Get.BroadcastEvent(new HE_PlayerDataChangedEvent(m_PlayerStats));
        }

        /// <summary>
        /// Gets the current player data (health, energy, score).
        /// </summary>
        public PlayerData Data => m_PlayerStats;
        /// <summary>
        /// Gets the entity type, which is always Player for this class.
        /// </summary>
        public override HE_EntityType EntityType => HE_EntityType.Player;
        //public HE_FloatRect Body => Body;

        private PlayerData m_PlayerStats;
    }


    /// <summary>
    /// Structure containing the player's current statistics.
    /// </summary>
    internal struct PlayerData
    {
        /// <summary>
        /// The player's current health (0-100).
        /// </summary>
        public int Health;
        /// <summary>
        /// The player's current score.
        /// </summary>
        public int Score;
        /// <summary>
        /// The player's current energy (0-100).
        /// </summary>
        public int Energy;
    }

    /// <summary>
    /// Event that is broadcast when player data (health, energy, score) changes.
    /// </summary>
    internal readonly struct HE_PlayerDataChangedEvent(PlayerData data) : IHE_EventType
    {
        /// <summary>
        /// Gets the updated player data.
        /// </summary>
        public PlayerData Data { get; } = data;
    }
}
