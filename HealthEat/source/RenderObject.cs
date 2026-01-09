using HealthEat.Exceptions;
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

    internal interface IRenderable
    {
        SFG.Drawable DrawingContext { get; }
        string Name { get; }
        int ID { get; }
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
        }
        
        public override bool Equals(Object? obj)
        {
            return obj is IRenderable other && m_ID == other.ID;
        }

        public override int GetHashCode()
        {
            return m_ID;
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

        protected T m_Context;

        private static int sm_NextID = 1;
        private readonly string m_ContextType;
        private readonly string m_Name;
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
        }

        private static readonly SFG.Font m_Font = new SFG.Font("C:/Windows/Fonts/Arial.ttf");

    }


    /// <summary>
    /// Wrapper for SFML Sprite class used in rendering process
    /// </summary>
    internal class SpriteObject : RenderObject<HE_Sprite>
    {

        public SpriteObject() : this("Unnamed") { }
        public SpriteObject(string name) : base(new HE_Sprite(), name)
        {
        #if DEBUG
            Console.WriteLine("Created sprite object without texture");
            #endif
        }

        public SpriteObject(string texture, string name) : base
            (new HE_Sprite(ResourceManager.Get.GetTexture(texture)), name)
        { }

        public SpriteObject(HE_Texture txt, string name) : base(new HE_Sprite(txt), name)
        {
            #if DEBUG
            Console.WriteLine("Created sprite object");
            #endif
        }

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
                #if DEBUG
                Console.WriteLine(e.Message);
                #endif
                return;
            }
        }

    }


    internal class RectangleShapeObject : RenderObject<HE_RectShape>
    {
        public RectangleShapeObject() : this("Unnamed_FloatRect") { }
        public RectangleShapeObject(string name) : base(new HE_RectShape(), name)
        {
            #if DEBUG
            Console.WriteLine($"Created RectangleShape {name}");
            #endif
        }
        public RectangleShapeObject(HE_Vec2 size, string name) : base(new HE_RectShape(size), name)
        {
            #if DEBUG
            Console.WriteLine($"Created RectangleShape {name}");
            #endif
        }

    }

}
