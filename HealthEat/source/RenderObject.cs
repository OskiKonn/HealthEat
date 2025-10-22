using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using SFG = SFML.Graphics;
using SFS = SFML.System;
using SFW = SFML.Window;

namespace HealthEat.Rendering
{

    internal interface IRenderable
    {
        SFG.Drawable DrawingContext { get; }
        string Name { get; }
        int ID { get; }
    }

    internal abstract class RenderObject<T> : IRenderable where T : SFG.Drawable
    {
        public RenderObject(T contextType, string? name = null)
        {
            m_Name = name ?? "Object_" + m_ID;
            m_ID = m_Name.GetHashCode();
            m_ContextType = typeof(T).Name;
            m_Context = contextType;
        }
        public T Context => m_Context;
        public SFG.Drawable DrawingContext => m_Context;
        
        public override bool Equals(Object? obj)
        {
            return obj is IRenderable other && m_ID == other.ID;
        }

        public override int GetHashCode()
        {
            return m_ID;
        }

        public string Name => m_Name;
        public int ID => m_ID;

        protected T m_Context;
        private readonly string m_ContextType;
        private readonly string m_Name;
        private readonly int m_ID;
    }

    internal class TextObject : RenderObject<SFG.Text>
    {

        public TextObject(string text, uint charSize = 24, string? objectName = null)
            : this(new SFG.Text(text, new SFG.Font("C:\\WINDOWS\\FONTS\\ARIAL.TTF"), charSize), objectName) { }

        public TextObject(SFG.Text text, string? objectName = null) : base(text, objectName)
        {
            m_Context.FillColor = SFG.Color.Red;
            m_Context.Font = new SFG.Font("C:/WINDOWS/FONTS/ARIAL.TTF");
            m_Context.CharacterSize = 24;
        }

    }

}
