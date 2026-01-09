using HealthEat.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFG = SFML.Graphics;
using SFS = SFML.System;

namespace HealthEat
{
    internal class Entity : SpriteObject
    {
        
        public Entity(string name) : this(new HE_Vec2(0.0f, 0.0f), name)
        {
            Console.WriteLine("[Entity]: Created entity");
        }


        public Entity(HE_Vec2 position, string name, float scale = 1.0f, bool interactable = false) : this(position, name, new HE_Vec2(scale, scale), interactable) { }
        public Entity(HE_Vec2 position, string name, string texture, float scale = 1.0f, bool interactable = false) :
            this(position, name, texture, new HE_Vec2(scale, scale), interactable) { }


        public Entity(HE_Vec2 position, string name, HE_Vec2 scale, bool interactable = false) : base(name)
        {
            if (!ValidateArguments(position, scale))
                throw new HE_InvalidArgumentValueException("[HE_Exception]: Invalid constructor argument");

            m_Context.Position = position;
            m_Name = "Entity_" + name;
            m_Context.Scale = scale;
            m_Interactable = interactable;
            m_Index = AssignEntityIndex();

            m_Bounds = m_Context.GetGlobalBounds();
            m_Border = new RectangleShapeObject(new HE_Vec2(m_Bounds.Width, m_Bounds.Height), $"{m_Name}_border");
            m_Border.Context.FillColor = HE_Color.Transparent;
            m_Border.Context.OutlineColor = HE_Color.Transparent;
            m_Border.Context.OutlineThickness = 2.0f;
            m_Border.Context.Position = new HE_Vec2(m_Bounds.Left, m_Bounds.Top);
        }


        public Entity(HE_Vec2 position, string name, string texture, HE_Vec2 scale, bool interactable = false) : base(texture, name)
        {
            if (!ValidateArguments(position, scale))
                throw new HE_InvalidArgumentValueException("[HE_Exception]: Invalid constructor argument");

            m_Context.Position = position;
            m_Name = "Entity_" + name;
            m_Context.Scale = scale;
            m_Interactable = interactable;
            m_Index = AssignEntityIndex();

            m_Bounds = m_Context.GetGlobalBounds();
            m_Border = new RectangleShapeObject(new HE_Vec2(m_Bounds.Width, m_Bounds.Height), $"{m_Name}_border");
            m_Border.Context.FillColor = HE_Color.Transparent;
            m_Border.Context.OutlineColor = HE_Color.Transparent;
            m_Border.Context.OutlineThickness = 2.0f;
            m_Border.Context.Position = new HE_Vec2(m_Bounds.Left, m_Bounds.Top);
        }


        public virtual void Move(HE_Vec2 offset)
        {
            m_Context.Position += offset;
            m_Border.Context.Position = m_Context.Position;
            m_Bounds = m_Context.GetGlobalBounds();
        }


        public virtual void Move(float offsetX, float offsetY)
        {
            m_Context.Position += new HE_Vec2(offsetX, offsetY);
            m_Border.Context.Position = m_Context.Position;
            m_Bounds = m_Context.GetGlobalBounds();
        }


        public virtual void Move(HE_Vec2 offset, float dt)
        {
            m_Context.Position += offset * m_Velocity * dt;
            m_Border.Context.Position = m_Context.Position;
            m_Bounds = m_Context.GetGlobalBounds();
        }


        public virtual void Move(float offsetX, float offsetY, float dt)
        {
            m_Context.Position += new HE_Vec2(offsetX, offsetY) * m_Velocity * dt;
            m_Border.Context.Position = m_Context.Position;
            m_Bounds = m_Context.GetGlobalBounds();
        }


        public void SetDebugMode(bool val, Scene scene)
        {

            if (val && m_DebugMode == false)
            {
                m_Border.Context.OutlineColor = HE_Color.Green;
                scene.AddToScene(m_Border);
                m_DebugMode = val;
            }
            else if (!val && m_DebugMode == true)
            {
                m_Border.Context.OutlineColor = HE_Color.Transparent;
                scene.DeleteFromScene(m_Border);
                m_DebugMode = val;
            }

            //m_Border.Context.OutlineColor = val ? HE_Color.Green : HE_Color.Transparent;
            //m_DebugMode = val;
            //Console.WriteLine($"SAASD - {m_Border.Context.Position.X} | {m_Border.Context.Position.Y}");
        }


        public bool Intersects(Entity ent)
        {
            return m_Bounds.Intersects(ent.Bounds);
        }


        public bool Intersects(HE_FloatRect rect)
        {
            return m_Bounds.Intersects(rect);
        }


        private uint AssignEntityIndex()
        {
            return sm_NextIndex++;
        }


        private static bool ValidateArguments(HE_Vec2 pos, HE_Vec2? scale)
        {
            if (pos.X < 0.0f || pos.Y < 0.0f)
                return false;

            if (scale != null && scale.Value.X <= 0.0f)
                return false;

            return true;
        }



        public float Velocity { get => m_Velocity; set { if (value > 0.0f) m_Velocity = value; } }
        public HE_Sprite Body => m_Context;
        public HE_FloatRect Bounds => m_Bounds;
        public HE_Vec2 Position => m_Context.Position;

        private HE_Vec2 m_Position;
        private HE_FloatRect m_Bounds;
        private RectangleShapeObject m_Border;
        private readonly uint m_Index;
        private readonly string m_Name = "Entity_-1";
        private bool m_Interactable = false;
        private bool m_DebugMode = false;
        private float m_Velocity = 1.0f;

        private static uint sm_NextIndex = 0;
    }
}
