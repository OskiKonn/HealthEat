using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HealthEat.Exceptions;

namespace HealthEat
{
    /// <summary>
    /// Handles physics simulation including collision detection and resolution.
    /// Manages kinematic and static colliders.
    /// </summary>
    internal class PhysicsEngine
    {
        /// <summary>
        /// Initializes a new PhysicsEngine instance.
        /// </summary>
        public PhysicsEngine()
        {
            #if HE_DEBUG
            Console.WriteLine($"[PhysicsEngine]: Physcis Engine created");
            #endif
        }

        /// <summary>
        /// Updates physics simulation by moving kinematic bodies and resolving collisions.
        /// </summary>
        /// <param name="dt">Delta time in seconds since last frame.</param>
        public void Update(float dt)
        {
            foreach (IKinematic body in m_KinematicEnts)
            {
                if (body.Freezed)
                    continue;

                body.Move(body.Velocity * dt);
            }

            ResolveCollisions();
        }

        /// <summary>
        /// Clears all registered colliders from the physics engine.
        /// </summary>
        public void Clear()
        {
            m_StaticEnts.Clear();
            m_KinematicEnts.Clear();
            m_RegisteredColliders.Clear();
        }

        /// <summary>
        /// Resolves collisions between all registered colliders.
        /// Handles both kinematic-static and kinematic-kinematic collisions.
        /// </summary>
        public void ResolveCollisions()
        {
            int count = m_KinematicEnts.Count;

            for (int i = 0; i < count; i++)
            {
                IKinematic ki = m_KinematicEnts[i];

                if (!ki.CanTouch)
                    continue;

                foreach (ICollider st in m_StaticEnts)
                {
                    if (!st.CanTouch)
                        continue;

                    if (CheckForCollision(ki, st, out HE_Vec2 pv))
                    {
                        ki.Push(pv);
                        ki.OnCollision(st);
                        st.OnCollision(ki);
                    }
                }

                for (int j = i + 1; j < count; j++)
                {
                    //IKinematic A = m_KinematicEnts[i];
                    IKinematic B = m_KinematicEnts[j];

                    if (!B.CanTouch)
                        continue;

                    if (CheckForCollision(ki, B, out HE_Vec2 pv))
                    {
                        ki.Push(pv * 0.5f);
                        ki.Push(-pv * 0.5f);

                        ki.OnCollision(B);
                        B.OnCollision(ki);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a collider to the physics engine for collision detection.
        /// </summary>
        /// <param name="item">The collider to add.</param>
        public void AddCollider(ICollider item)
        {
            if (m_RegisteredColliders.Contains(item))
                return;

            if (item.IsKinematic)
                m_KinematicEnts.Add((IKinematic)item);
            else
                m_StaticEnts.Add(item);

            m_RegisteredColliders.Add(item);

            #if HE_DEBUG
            Console.WriteLine($"[PhysicsEngine]: Added collider {((IRenderable)item).Name}");
            #endif
        }

        /// <summary>
        /// Removes a collider from the physics engine.
        /// </summary>
        /// <param name="item">The collider to remove.</param>
        public void RemoveCollider(ICollider item)
        {
            if (!m_RegisteredColliders.Contains(item))
                return;

            if (item.IsKinematic)
                m_KinematicEnts.Remove((IKinematic)item);
            else
                m_StaticEnts.Remove(item);

            m_RegisteredColliders.Remove(item);
        }

        private bool CheckForCollision(ICollider a, ICollider b, out HE_Vec2 pushVector)
        {
            pushVector = new HE_Vec2(0f, 0f);

            if (!(a.Body.Intersects(b.Body, out HE_FloatRect overlap)))
                return false;

            pushVector = CalcPushVector(a, b, overlap);
            return true;

        }

        private HE_Vec2 CalcPushVector(ICollider A, ICollider B, HE_FloatRect overlap)
        {
            HE_FloatRect bA = A.Body;
            HE_FloatRect bB = B.Body;

            float dx = bA.Position.X - bB.Position.X;
            float dy = bA.Position.Y - bB.Position.Y;

            if (overlap.Width < overlap.Height)
            {
                int sign = Math.Sign(dx);
                return new HE_Vec2(sign * overlap.Width, 0f);
            }
            else
            {
                int sign = Math.Sign(dy);
                return new HE_Vec2(0f, sign * overlap.Height);
            }
        }

        private List<ICollider> m_StaticEnts = new List<ICollider>(10);
        private List<IKinematic> m_KinematicEnts = new List<IKinematic>(10);
        private HashSet<ICollider> m_RegisteredColliders = new HashSet<ICollider>(15);
    }
}
