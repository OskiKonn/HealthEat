using HealthEat.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

using SFG = SFML.Graphics;
using SFS = SFML.System;

namespace HealthEat
{
    internal interface IReadOnlyEntity
    {
        HE_RenderObjectType GetObjectType { get; }
        HE_Vec2 GetVelocity { get; }
        uint GetIndex { get; }
        HE_EntityType GetEntityType { get; }
        bool IsTouchable { get; }
        bool IsClickable { get; }
        bool IsInteractable { get; }
        string Name { get; }
        HE_Vec2 Position { get; }
        void OnInteraction() { }
        void OnClick() { }

        void SetDebugMode(bool value) { }
    }


    internal interface ICollider
    {
        HE_FloatRect Body { get; }
        bool IsKinematic { get; }
        bool IsTrigger { get; }
        bool CanTouch { get; }

        void OnCollision(ICollider other);
    }


    internal interface IKinematic : ICollider
    {
        HE_Vec2 Velocity { get; set; }
        float Acceleration { get; set; }
        bool Freezed { get; set; }

        void Move(HE_Vec2 offset);
        void Push(HE_Vec2 offset);
    }


    internal interface IInteractable
    {
        HE_Vec2 Position { get; }
        float InteractRange { get; }
        bool CanInteract { get; }
        string InteractPrompt { get; }

        void OnInteraction(Entity interactor);
    }


    internal interface IClickable
    {
        HE_FloatRect ClickBounds { get; }

        void OnClick(Entity? target);
        void OnMouseEnter();
        void OnMouseLeave();
    }


    internal abstract class Entity : SpriteObject, IReadOnlyEntity
    {
        
        public Entity(string name, string texture) : this(new HE_Vec2(0.0f, 0.0f), name, texture)
        {
        }


        public Entity(HE_Vec2 position, string name, string texture, float scale = 1.0f, bool touchable = false, bool interactable = false, bool clickable = false)
            : base(position, texture, name, scale)
        {

            m_Name = "Entity_" + name;
            CanTouch = touchable;
            Interactable = interactable;
            m_CanInteract = interactable;
            Clickable = clickable;
            m_Index = AssignEntityIndex();


        }


        private uint AssignEntityIndex()
        {
            return sm_NextIndex++;
        }

        private void FirePropertyChangedEvent(HE_EntityProperty p, bool v)
        {
            HE_EventBus.Get.BroadcastEvent(new HE_PropertyChangedEvent(this, p, v));
        }


        public bool CanTouch
        {
            get => m_CanTouch;
            set { m_CanTouch = value; FirePropertyChangedEvent(HE_EntityProperty.Touchable, value); }
        }

        public bool Interactable
        {
            get => m_Interactable;
            set { m_Interactable = value; FirePropertyChangedEvent(HE_EntityProperty.Interactable, value); }
        }


        public override HE_RenderObjectType ObjectType => HE_RenderObjectType.Entity;
        public HE_Vec2 Velocity { get => m_Velocity; set { if (value.X >= 0.0f && value.Y >= 0.0f) m_Velocity = value; } }
        public uint Index => m_Index;
        public virtual HE_EntityType EntityType => HE_EntityType.Entity;

        // IReadOnlyEntity
        public HE_RenderObjectType GetObjectType => ObjectType;
        public HE_Vec2 GetVelocity => m_Velocity;
        public uint GetIndex => m_Index;
        public HE_EntityType GetEntityType => EntityType;
        public bool IsTouchable => m_CanTouch;
        public bool IsInteractable => m_Interactable;

        protected HE_Vec2 m_Velocity = new HE_Vec2(0f, 0f);

        protected bool m_CanTouch = false;
        protected bool m_Interactable = false;
        protected bool m_Clickable = false;
        protected bool m_CanInteract = false;
        protected float m_InteractRange = 1f;
        protected string m_InteractPrompt = "";

        private readonly uint m_Index;
        private static uint sm_NextIndex = 0;

    }


    internal class StaticEntity : Entity, ICollider, IInteractable, IClickable
    {
        public StaticEntity(HE_Vec2 position, string name, string texture, float scale = 1, bool interactable = false, bool clickable = false)
            : base(position, name, texture, scale, true, interactable, clickable)
        {
            m_InteractRange = Math.Max(m_Bounds.Height, m_Bounds.Width) / 2.0f + 5.0f;

        }

        public void SetOnCollisionAction(Action<ICollider> action)
        {
            m_OnCollisionAction += action;
        }

        public void SetOnInteractionAction(Action<Entity> action)
        {
            m_OnInteractionAction += action;
        }

        public virtual void OnInteraction(Entity interactor)
        {
            m_OnInteractionAction?.Invoke(interactor);
        }

        public virtual void OnCollision(ICollider other)
        {
            m_OnCollisionAction?.Invoke(other);
        }


        public float InteractRange => m_InteractRange;
        public bool CanInteract { get => m_CanInteract; set => m_CanInteract = value; }
        public string InteractPrompt => m_InteractPrompt;

        public bool IsKinematic => false;
        public bool IsTrigger { get; private set; }

        private event Action<ICollider>? m_OnCollisionAction;
        private event Action<Entity>? m_OnInteractionAction;

    }


    internal class KinematicEntity : Entity, IKinematic
    {
        public KinematicEntity(HE_Vec2 position, string name, string texture, float scale = 1)
            : base(position, name, texture, scale, true, false, false)
        {
            m_CanTouch = true;
            Acceleration = 1f;
        }

        public void SetOnCollisionAction(Action<ICollider> action)
        {
            m_OnCollisionAction += action;
        }

        public virtual void OnCollision(ICollider other)
        {
            m_OnCollisionAction?.Invoke(other);
        }

        public virtual void Push(HE_Vec2 offset)
        {
            m_Context.Position += offset;
            m_Border.Position = m_Context.Position;
            m_Bounds = m_Context.GetGlobalBounds();
        }

        public virtual void Move(HE_Vec2 offset)
        {
            Push(offset * Acceleration);
        }


        public virtual void Move(HE_Vec2 offset, float dt)
        {
            Move(offset * dt);
        }


        public virtual void Move(float offsetX, float offsetY)
        {
            Move(new HE_Vec2(offsetX, offsetY));
        }


        public virtual void Move(float offsetX, float offsetY, float dt)
        {
            Move(new HE_Vec2(offsetX, offsetY) * dt);
        }

        public float Acceleration { get; set; }
        public bool IsKinematic => true;
        public bool IsTrigger => false;
        public bool Freezed { get => m_Freezed; set => m_Freezed = value; }

        protected float m_MoveSpeed = 150f;
        protected bool m_Freezed = false;

        private event Action<ICollider>? m_OnCollisionAction;

    }


    internal class InvisibleCollider : RectangleShapeObject, ICollider
    {
        public InvisibleCollider(HE_Vec2 position, HE_Vec2 size) : base(position, size, "Invisible_Collider" + sm_Counter++)
        {
            m_Context.FillColor = HE_Color.Transparent;
            m_Context.OutlineColor = HE_Color.Transparent;
            m_Context.OutlineThickness = 0.0f;
        }

        public void OnCollision(ICollider other)
        {

        }

        public new void SetPosition(HE_Vec2 position)
        {
            m_Context.Position = position;
            m_Border.Position = m_Context.Position;
            m_Bounds = GetGlobalBounds();
        }

        protected override void OnMouseScrollEvent(HE_MouseScrollEvent args)
        {
            float delta = m_Context.Scale.X + (args.Delta / 100);

            if (m_IsDragged)
            {

                if (InputController.Get.IsActionHeld(HE_Action.MoveLeft))
                    m_Context.Size = new HE_Vec2(m_Context.Size.X + 10f, m_Context.Size.Y);

                if (InputController.Get.IsActionHeld(HE_Action.MoveRight))
                    m_Context.Size = new HE_Vec2(m_Context.Size.X - 10f, m_Context.Size.Y);

                if (InputController.Get.IsActionHeld(HE_Action.MoveUp))
                    m_Context.Size = new HE_Vec2(m_Context.Size.X, m_Context.Size.Y + 10f);

                if (InputController.Get.IsActionHeld(HE_Action.MoveDown))
                    m_Context.Size = new HE_Vec2(m_Context.Size.X, m_Context.Size.Y - 10f);

                SetScale(delta);
            }
        }

        public bool CanTouch { get; set; } = true;
        public bool IsKinematic => false;
        public bool IsTrigger { get; private set; }
        public override bool Collider => true;

        private static int sm_Counter = 0;
    }

    //internal class HE_EntityPropertyChangedEventArgs : EventArgs
    //{

    //    public HE_EntityPropertyChangedEventArgs(Entity caller, HE_EntityProperty property, bool value)
    //    {
    //        Caller = caller;
    //        Property = property;
    //        Value = value;
    //    }

    //    public Entity Caller { get; }
    //    public HE_EntityProperty Property { get; }
    //    public bool Value { get; }

    //}


    internal readonly struct HE_PropertyChangedEvent(Entity ent, HE_EntityProperty property, bool value) : IHE_EventType
    {
        public Entity Entity { get; } = ent;
        public HE_EntityProperty Property { get; } = property;
        public bool Value { get; } = value;
    }


    internal enum HE_EntityProperty
    {
        Touchable = 1,
        Interactable = 2,
        Clickable = 3,
    }


    internal enum HE_EntityType : byte
    {
        Player = 0,
        Entity,
    }
}
