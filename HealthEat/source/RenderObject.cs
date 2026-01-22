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
        bool IsClickable { get; }

        void Update();
        void Draw(Window wnd);
    }


    internal interface IReadOnlySprite
    {
        HE_FloatRect Body { get; }
        SFG.RectangleShape Border { get; }
        HE_FloatRect Bounds { get; }
        HE_Vec2 Position { get; }
    }


    /// <summary>
    /// Base abstract generic class for in-game rendering mechanism. Serves as a base for other classes used in rendering such as TextObject.
    /// Creates DrawingContext of type T that is later used for actions on SFML types during rendering
    /// </summary>
    /// <typeparam name="T">Type of SFML framework type to be used as rendedring context</typeparam>
    internal abstract class RenderObject<T> : IRenderable, IClickable where T : SFG.Transformable, SFG.Drawable
    {
        public RenderObject(T contextType, string? name = null, HE_Vec2? position = null, float scale = 1f)
        {
            m_Name = name ?? "Object_" + GetNextIDStr();
            m_ID = m_Name.GetHashCode();
            m_ContextType = typeof(T).Name;
            m_Context = contextType;
            m_Border = new SFG.RectangleShape();
            m_Border.FillColor = HE_Color.Transparent;
            m_Border.OutlineColor = HE_Color.Transparent;
            m_Border.OutlineThickness = 2.0f;
            Visible = true;

            m_OnUpdateAction += OnMouseDrag;
            HE_EventBus.Get.SubscribeToEvent<HE_MouseScrollEvent>(OnMouseScrollEvent);

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

        public virtual void Update()
        {
            m_OnUpdateAction?.Invoke();
        }

        public void SetPosition(float x, float y)
        {
            SetPosition(new HE_Vec2(x, y));
        }

        public void SetPosition(HE_Vec2 position)
        {
            m_Context.Position = position;
            m_Border.Position = m_Context.Position;
            m_Bounds = GetGlobalBounds();
        }

        public void SetScale(float scale)
        {
            CenterOrigin(m_Context.Position, new HE_Vec2(scale, scale));
        }

        public void SetDebugMode(bool val)
        {

            if (val && m_DebugMode == false)
            {
                m_Border.OutlineColor = HE_Color.Green;
                m_DebugMode = val;
            }
            else if (!val && m_DebugMode == true)
            {
                m_Border.OutlineColor = HE_Color.Transparent;
                m_DebugMode = val;
            }

        }

        public void SetSize(HE_Vec2 size)
        {
            HE_FloatRect localBounds = GetLocalBounds();

            if (localBounds.Width == 0 || localBounds.Height == 0)
                return;

            float newSX = size.X / localBounds.Width;
            float newSY = size.Y / localBounds.Height;

            CenterOrigin(m_Context.Position, new HE_Vec2(newSX, newSY));
        }

        public void SwitchMouseDragging()
        {
            m_IsDragged = !m_IsDragged;

            #if HE_DEBUG
            if (m_IsDragged)
                Console.WriteLine($"Object position on begin: ({m_Context.Position.X}, {m_Context.Position.Y}) | scale: {m_Context.Scale.X}, w: {m_Bounds.Width}, h: {m_Bounds.Height}");

            else
                Console.WriteLine($"Object position on end: ({m_Context.Position.X}, {m_Context.Position.Y}) | scale: {m_Context.Scale.X}, w: {m_Bounds.Width}, h: {m_Bounds.Height}");
            #endif
        }

        public virtual void Draw(Window wnd)
        {
            wnd.RenderWindow.Draw(m_Context);

            if (m_DebugMode)
                wnd.RenderWindow.Draw(m_Border);
        }

        public void Destroy()
        {
            m_Expired = true;
        }

        public void SetOnUpdateAction(Action action)
        {
            m_OnUpdateAction += action;
        }

        public void SetOnClickAction(Action<Entity?> action)
        {
            m_OnClickAction += action;
        }

        public void SetDefaultClickAction()
        {
            //Clickable = true;

            SetOnClickAction((e) =>
            {
                SwitchMouseDragging();
            });
        }

        public virtual void OnClick(Entity? target)
        {

            m_OnClickAction?.Invoke(target);
            #if HE_DEBUG
            Console.WriteLine($"[Entity_{m_Name}]: Entity clicked");
            #endif
        }

        public virtual void OnMouseEnter()
        {

        }

        public virtual void OnMouseLeave()
        {

        }

        protected void CenterOrigin(HE_Vec2 position, HE_Vec2 scale)
        {
            m_Context.Scale = scale;
            HE_FloatRect localBnds = GetLocalBounds();
            m_Context.Origin = new HE_Vec2(
                localBnds.Left + localBnds.Width / 2.0f,
                localBnds.Top + localBnds.Height / 2.0f
            );

            m_Context.Position = position;

            m_Bounds = GetGlobalBounds();
            m_Border.Size = new HE_Vec2(m_Bounds.Width, m_Bounds.Height);
            m_Border.Origin = new HE_Vec2(m_Bounds.Width / 2.0f, m_Bounds.Height / 2.0f);
            m_Border.Position = m_Context.Position;
        }

        protected void CenterOrigin(HE_Vec2 position, float scale = 1f)
        {
            m_Context.Scale = new HE_Vec2(scale, scale);
            HE_FloatRect localBnds = GetLocalBounds();
            m_Context.Origin = new HE_Vec2(
                localBnds.Left + localBnds.Width / 2.0f,
                localBnds.Top + localBnds.Height / 2.0f
            );

            m_Context.Position = position;

            m_Bounds = GetGlobalBounds();
            m_Border.Size = new HE_Vec2(m_Bounds.Width, m_Bounds.Height);
            m_Border.Origin = new HE_Vec2(m_Bounds.Width / 2.0f, m_Bounds.Height / 2.0f);
            m_Border.Position = m_Context.Position;
        }

        protected void OnMouseDrag()
        {
            if (m_IsDragged)
            {
                HE_Vec2i mPos = InputController.Get.GetMousePos();
                SetPosition((HE_Vec2)mPos);
            }
        }

        protected virtual void OnMouseScrollEvent(HE_MouseScrollEvent args)
        {
            float delta = m_Context.Scale.X + (args.Delta / 100);

            if (m_IsDragged)
            {
                SetScale(delta);
            }
        }

        protected abstract HE_FloatRect GetGlobalBounds();
        protected abstract HE_FloatRect GetLocalBounds();

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
        public SFG.RectangleShape Border => m_Border;
        public HE_FloatRect Body => m_Bounds;
        public HE_FloatRect Bounds => Body;
        public HE_FloatRect ClickBounds => Body;
        public HE_Vec2 Position => m_Context.Position;
        public bool Clickable { get; set; }
        public bool IsClickable => Clickable;


        public bool Visible { get; set; } = true;
        public bool IsExpired => m_Expired;

        protected T m_Context;
        protected bool m_Expired = false;
        protected string m_Name;
        protected HE_Vec2 m_Position;
        protected HE_FloatRect m_Bounds;
        protected SFG.RectangleShape m_Border;
        protected Action? m_OnUpdateAction;
        protected Action<Entity?>? m_OnClickAction;
        protected bool m_IsDragged = false;

        private static int sm_NextID = 1;
        private bool m_DebugMode = false;
        private readonly string m_ContextType;
        private readonly int m_ID;



    }

    /// <summary>
    /// Wrapper for SFML Text class used in rendering process
    /// </summary>
    internal class TextObject : RenderObject<HE_Text>
    {

        public TextObject(string text, uint charSize = 24, string? objectName = null)
            : this(new HE_Vec2(0,0), new HE_Text(text, m_Font, charSize), objectName) { }

        public TextObject(HE_Vec2 position, HE_Text text, string? objectName = null) : base(text, objectName)
        {
            m_Context.FillColor = SFG.Color.Red;
            m_Context.Font = m_Font;
            m_Context.CharacterSize = 24;

            CenterOrigin(position);

            //HE_FloatRect bounds = m_Context.GetGlobalBounds();
            //m_Context.Origin = new HE_Vec2(bounds.Width / 2, bounds.Height / 2);
        }

        public void SetText(string txt)
        {
            m_Context.DisplayedString = txt;
        }

        protected override HE_FloatRect GetGlobalBounds()
        {
            return m_Context.GetGlobalBounds();
        }

        protected override HE_FloatRect GetLocalBounds()
        {
            return m_Context.GetLocalBounds();
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
        public SpriteObject(HE_Vec2 position, string name, float scale = 1.0f) : base(new HE_Sprite(), name, position)
        {
            //if (!ValidateArguments(position, scale))
            //    throw new HE_InvalidArgumentValueException("[HE_Exception]: Invalid constructor argument");

            //m_Context.Scale = new HE_Vec2(scale, scale);

            //HE_FloatRect localBnds = m_Context.GetLocalBounds();
            //m_Context.Origin = new HE_Vec2(localBnds.Width / 2.0f, localBnds.Height / 2.0f);

            //m_Context.Position = position;
            m_Name = "Sprite_" + name;
            CenterOrigin(position, scale);

            //m_Bounds = m_Context.GetGlobalBounds();
            //m_Border = new RectangleShapeObject(new HE_Vec2(m_Bounds.Width, m_Bounds.Height), $"{m_Name}_border");
            //m_Border.Context.Origin = new HE_Vec2(m_Bounds.Width / 2.0f, m_Bounds.Height / 2.0f);
            //m_Border.Context.FillColor = HE_Color.Transparent;
            //m_Border.Context.OutlineColor = HE_Color.Transparent;
            //m_Border.Context.OutlineThickness = 2.0f;
            //m_Border.Context.Position = m_Context.Position;


#if HE_DEBUG
            Console.WriteLine("Created sprite object without texture");
            #endif
        }

        public SpriteObject(string texture, string name) : this(new HE_Vec2(0f, 0f), texture, name) { }


        public SpriteObject(HE_Vec2 position, string texture, string name, float scale = 1.0f) : base(new HE_Sprite(ResourceManager.Get.GetTexture(texture)), name, position)
        {

            //if (!ValidateArguments(position, scale))
            //    throw new HE_InvalidArgumentValueException("[HE_Exception]: Invalid constructor argument");

            //m_Context.Scale = new HE_Vec2(scale, scale);
            //HE_FloatRect localBnds = m_Context.GetLocalBounds();
            //m_Context.Origin = new HE_Vec2(localBnds.Width / 2.0f, localBnds.Height / 2.0f);

            //m_Context.Position = position;
            m_Name = "Sprite_" + name;
            CenterOrigin(position, scale);

            //m_Bounds = m_Context.GetGlobalBounds();
            //m_Border = new RectangleShapeObject(new HE_Vec2(m_Bounds.Width, m_Bounds.Height), $"{m_Name}_border");
            //m_Border.Context.Origin = new HE_Vec2(m_Bounds.Width / 2.0f, m_Bounds.Height / 2.0f);
            //m_Border.Context.FillColor = HE_Color.Transparent;
            //m_Border.Context.OutlineColor = HE_Color.Transparent;
            //m_Border.Context.OutlineThickness = 2.0f;
            //m_Border.Context.Position = m_Context.Position;


#if HE_DEBUG
            Console.WriteLine("Created sprite object");
            #endif
        }

        public override HE_RenderObjectType ObjectType => HE_RenderObjectType.Sprite;

        public void SetTexture(HE_Texture txt)
        {
            m_Context.Texture = txt;
            CenterOrigin(m_Context.Position, m_Context.Scale.X);
        }

        public void SetTexture(string name)
        {
            try
            {
                m_Context.Texture = ResourceManager.Get.GetTexture(name);
                CenterOrigin(m_Context.Position, m_Context.Scale.X);
            }
            catch (HE_MissingAssetException e)
            {
                #if HE_DEBUG
                Console.WriteLine(e.Message);
                #endif
                return;
            }
        }

        protected override HE_FloatRect GetGlobalBounds()
        {
            return m_Context.GetGlobalBounds();
        }

        protected override HE_FloatRect GetLocalBounds()
        {
            return m_Context.GetLocalBounds();
        }

        protected static bool ValidateArguments(HE_Vec2 pos, float scale)
        {
            if (pos.X < 0.0f || pos.Y < 0.0f)
                return false;

            if (scale <= 0.0f)
                return false;

            return true;
        }


    }


    internal class RectangleShapeObject : RenderObject<HE_RectShape>
    {
        public RectangleShapeObject() : this("Unnamed_FloatRect") { }
        public RectangleShapeObject(string name) : this(new HE_Vec2(0,0), new HE_Vec2(0,0), name)
        {
            #if HE_DEBUG
            Console.WriteLine($"Created RectangleShape {name}");
            #endif

        }

        public RectangleShapeObject(HE_Vec2 size, string name) : this(new HE_Vec2(0,0), size, name)
        {
            #if HE_DEBUG
            Console.WriteLine($"Created RectangleShape {name}");
            #endif
        }

        public RectangleShapeObject(HE_Vec2 position, HE_Vec2 size, string name) : base(new HE_RectShape(size), name, position)
        {
            CenterOrigin(position);

            #if HE_DEBUG
            Console.WriteLine($"Created RectangleShape {name}");
            #endif
        }

        public void SetTexture(HE_Texture txt)
        {
            m_Context.Texture = txt;
            CenterOrigin(m_Context.Position, m_Context.Scale.X);
        }


        public void SetTexture(string name)
        {
            try
            {
                m_Context.Texture = ResourceManager.Get.GetTexture(name);
                CenterOrigin(m_Context.Position, m_Context.Scale.X);
            }
            catch (HE_MissingAssetException e)
            {
            #if HE_DEBUG
                Console.WriteLine(e.Message);
            #endif
                return;
            }
        }

        protected override HE_FloatRect GetGlobalBounds()
        {
            return m_Context.GetGlobalBounds();
        }

        protected override HE_FloatRect GetLocalBounds()
        {
            return m_Context.GetLocalBounds();
        }

        public virtual bool Collider => false;
        public override HE_RenderObjectType ObjectType => HE_RenderObjectType.RectangleShape;

    }

}
