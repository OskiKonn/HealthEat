using HealthEat.Exceptions;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

using SFG = SFML.Graphics;
using SFS = SFML.System;
using SFW = SFML.Window;

namespace HealthEat
{

    internal enum HE_RenderObjectType : byte
    {
        Text,
        Sprite,
        RectangleShape,
        Entity
    }


    internal interface IRenderable
    {
        HE_RenderObjectType ObjectType { get; }
        SFG.Drawable DrawingContext { get; }
        string Name { get; }
        int ID { get; }
        bool Visible { get; set; }
    }


    internal interface IReadOnlySprite
    {
        HE_FloatRect Body { get; }
        RectangleShapeObject Border { get; }
        HE_FloatRect Bounds { get; }
        HE_Vec2 Position { get; }
    }


    /// <summary>
    /// Base abstract generic class for in-game rendering mechanism. Serves as a base for other classes used in rendering such as TextObject.
    /// Creates DrawingContext of type T that is later used for actions on SFML types during rendering
    /// </summary>
    /// <typeparam name="T">Type of SFML framework type to be used as rendedring context</typeparam>
    internal abstract class RenderObject<T> : IRenderable where T : SFG.Drawable
    {
        public RenderObject(T contextType, string? name = null)
        {
            m_Name = name ?? "Object_" + GetNextIDStr();
            m_ID = m_Name.GetHashCode();
            m_ContextType = typeof(T).Name;
            m_Context = contextType;
            Visible = true;
        }
        
        public abstract HE_RenderObjectType ObjectType { get; }

        public override bool Equals(Object? obj)
        {
            return obj is IRenderable other && m_ID == other.ID;
        }

        public override int GetHashCode()
        {
            return m_ID;
        }

        public void Destroy()
        {
            m_Expired = true;
        }

        /// <summary>
        /// Method used for obtaining id to be assigned to the object in form of string type
        /// </summary>
        /// <returns>New id of the object as string</returns>
        private static string GetNextIDStr()
        {
            string id = sm_NextID.ToString();
            sm_NextID++;
            return id;
        }

        /// <summary>
        /// Method used for obtaining id to be assigned to the object
        /// </summary>
        /// <returns>New if of the object</returns>
        private static int GetNextID()
        {
            return sm_NextID++;
        }

        /// <summary>
        /// Name of the render object
        /// </summary>
        public string Name => m_Name;

        /// <summary>
        /// ID of the rendering object
        /// </summary>
        public int ID => m_ID;

        /// <summary>
        /// Drawing context as T type
        /// </summary>
        public T Context => m_Context;

        /// <summary>
        /// Drawing context as SFML Drawable class object
        /// </summary>
        public SFG.Drawable DrawingContext => m_Context;

        public bool Visible { get; set; } = true;
        public bool IsExpired => m_Expired;

        protected T m_Context;
        protected bool m_Expired = false;
        protected string m_Name;

        private static int sm_NextID = 1;
        private readonly string m_ContextType;
        private readonly int m_ID;
    }

    /// <summary>
    /// Wrapper for SFML Text class used in rendering process
    /// </summary>
    internal class TextObject : RenderObject<HE_Text>
    {

        public TextObject(string text, uint charSize = 24, string? objectName = null)
            : this(new HE_Text(text, m_Font, charSize), objectName) { }

        public TextObject(HE_Text text, string? objectName = null) : base(text, objectName)
        {
            m_Context.FillColor = SFG.Color.Red;
            m_Context.Font = m_Font;
            m_Context.CharacterSize = 24;

            HE_FloatRect bounds = m_Context.GetGlobalBounds();
            m_Context.Origin = new HE_Vec2(bounds.Width / 2, bounds.Height / 2);
        }

        public override HE_RenderObjectType ObjectType => HE_RenderObjectType.Text;

        private static readonly SFG.Font m_Font = new SFG.Font("C:/Windows/Fonts/Arial.ttf");

    }


    /// <summary>
    /// Wrapper for SFML Sprite class used in rendering process
    /// </summary>
    internal class SpriteObject : RenderObject<HE_Sprite>, IReadOnlySprite
    {

        public SpriteObject() : this(new HE_Vec2(0f, 0f), "Unnamed") { }
        public SpriteObject(HE_Vec2 position, string name, float scale = 1.0f) : base(new HE_Sprite(), name)
        {
            //if (!ValidateArguments(position, scale))
            //    throw new HE_InvalidArgumentValueException("[HE_Exception]: Invalid constructor argument");

            m_Context.Scale = new HE_Vec2(scale, scale);

            HE_FloatRect localBnds = m_Context.GetLocalBounds();
            m_Context.Origin = new HE_Vec2(localBnds.Width / 2.0f, localBnds.Height / 2.0f);

            m_Context.Position = position;
            m_Name = "Sprite_" + name;

            m_Bounds = m_Context.GetGlobalBounds();
            m_Border = new RectangleShapeObject(new HE_Vec2(m_Bounds.Width, m_Bounds.Height), $"{m_Name}_border");
            m_Border.Context.Origin = new HE_Vec2(m_Bounds.Width / 2.0f, m_Bounds.Height / 2.0f);
            m_Border.Context.FillColor = HE_Color.Transparent;
            m_Border.Context.OutlineColor = HE_Color.Transparent;
            m_Border.Context.OutlineThickness = 2.0f;
            m_Border.Context.Position = m_Context.Position;


#if HE_DEBUG
            Console.WriteLine("Created sprite object without texture");
            #endif
        }

        public SpriteObject(string texture, string name) : this(new HE_Vec2(0f, 0f), texture, name) { }


        public SpriteObject(HE_Vec2 position, string texture, string name, float scale = 1.0f) : base(new HE_Sprite(ResourceManager.Get.GetTexture(texture)), name)
        {

            //if (!ValidateArguments(position, scale))
            //    throw new HE_InvalidArgumentValueException("[HE_Exception]: Invalid constructor argument");

            m_Context.Scale = new HE_Vec2(scale, scale);
            HE_FloatRect localBnds = m_Context.GetLocalBounds();
            m_Context.Origin = new HE_Vec2(localBnds.Width / 2.0f, localBnds.Height / 2.0f);

            m_Context.Position = position;
            m_Name = "Sprite_" + name;

            m_Bounds = m_Context.GetGlobalBounds();
            m_Border = new RectangleShapeObject(new HE_Vec2(m_Bounds.Width, m_Bounds.Height), $"{m_Name}_border");
            m_Border.Context.Origin = new HE_Vec2(m_Bounds.Width / 2.0f, m_Bounds.Height / 2.0f);
            m_Border.Context.FillColor = HE_Color.Transparent;
            m_Border.Context.OutlineColor = HE_Color.Transparent;
            m_Border.Context.OutlineThickness = 2.0f;
            m_Border.Context.Position = m_Context.Position;


#if HE_DEBUG
            Console.WriteLine("Created sprite object");
            #endif
        }

        public override HE_RenderObjectType ObjectType => HE_RenderObjectType.Sprite;

        public void SetTexture(HE_Texture txt)
        {
            m_Context.Texture = txt;
        }

        public void SetTexture(string name)
        {
            try
            {
                m_Context.Texture = ResourceManager.Get.GetTexture(name);
            }
            catch (HE_MissingAssetException e)
            {
                #if HE_DEBUG
                Console.WriteLine(e.Message);
                #endif
                return;
            }
        }

        public void SetPosition(float x, float y)
        {
            SetPosition(new HE_Vec2(x, y));
        }

        public void SetPosition(HE_Vec2 position)
        {
            m_Context.Position = position;
            m_Border.Context.Position = m_Context.Position;
            m_Bounds = m_Context.GetGlobalBounds();
        }

        public void SetDebugMode(bool val)
        {

            if (val && m_DebugMode == false)
            {
                m_Border.Context.OutlineColor = HE_Color.Green;
                m_DebugMode = val;
            }
            else if (!val && m_DebugMode == true)
            {
                m_Border.Context.OutlineColor = HE_Color.Transparent;
                m_DebugMode = val;
            }

        }

        protected static bool ValidateArguments(HE_Vec2 pos, float scale)
        {
            if (pos.X < 0.0f || pos.Y < 0.0f)
                return false;

            if (scale <= 0.0f)
                return false;

            return true;
        }

        public HE_Sprite Sprite => m_Context;
        public RectangleShapeObject Border => m_Border;
        public HE_FloatRect Body => m_Bounds;
        public HE_FloatRect Bounds => Body;
        public HE_Vec2 Position => m_Context.Position;

        protected HE_Vec2 m_Position;
        protected HE_FloatRect m_Bounds;
        protected RectangleShapeObject m_Border;

        private bool m_DebugMode = false;

    }


    internal class RectangleShapeObject : RenderObject<HE_RectShape>
    {
        public RectangleShapeObject() : this("Unnamed_FloatRect") { }
        public RectangleShapeObject(string name) : base(new HE_RectShape(), name)
        {
            #if HE_DEBUG
            Console.WriteLine($"Created RectangleShape {name}");
            #endif

        }

        public RectangleShapeObject(HE_Vec2 size, string name) : base(new HE_RectShape(size), name)
        {
            #if HE_DEBUG
            Console.WriteLine($"Created RectangleShape {name}");
            #endif
        }

        public override HE_RenderObjectType ObjectType => HE_RenderObjectType.RectangleShape;

    }

}
