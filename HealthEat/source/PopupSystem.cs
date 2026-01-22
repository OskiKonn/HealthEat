using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthEat
{
    /// <summary>
    /// System for displaying popup messages to the user.
    /// Manages popup visibility, timing, and rendering.
    /// </summary>
    internal class PopupSystem
    {
        /// <summary>
        /// Initializes a new popup system with the specified scene and layer.
        /// </summary>
        /// <param name="scene">The scene to display popups in.</param>
        /// <param name="layer">The layer to render popups on.</param>
        public PopupSystem(Scene scene, SceneLayer layer)
        {
            m_Scene = scene;
            m_Layer = layer;
            m_PopupVisible = false;
        }

        /// <summary>
        /// Shows a popup message for the specified duration.
        /// </summary>
        /// <param name="message">The message text to display.</param>
        /// <param name="duration">How long the popup should be visible in seconds. Default is 2.0 seconds.</param>
        public void ShowPopup(string message, float duration = 2.0f)
        {
            if (m_PopupVisible)
                HidePopup();

            // Create background rectangle
            m_Background = new RectangleShapeObject(
                new HE_Vec2(1024 / 2, 720 / 2),
                new HE_Vec2(400, 150),
                "popup_bg"
            );
            m_Background.Context.FillColor = new HE_Color(50, 50, 50, 230);
            m_Background.Context.OutlineColor = HE_Color.White;
            m_Background.Context.OutlineThickness = 3.0f;
            m_Layer.AddToLayer(m_Background);

            // Create text
            m_Text = new TextObject(message, 28, "popup_text");
            m_Text.SetPosition(1024 / 2, 720 / 2);
            m_Text.Context.FillColor = HE_Color.White;
            m_Layer.AddToLayer(m_Text);

            m_PopupVisible = true;
            m_PopupTimer = duration;
        }

        /// <summary>
        /// Updates the popup system. Handles automatic hiding after duration expires.
        /// </summary>
        /// <param name="dt">Delta time in seconds since last frame.</param>
        public void Update(float dt)
        {
            if (m_PopupVisible && m_PopupTimer > 0)
            {
                m_PopupTimer -= dt;
                if (m_PopupTimer <= 0)
                {
                    HidePopup();
                }
            }
        }

        /// <summary>
        /// Hides the currently visible popup and cleans up resources.
        /// </summary>
        public void HidePopup()
        {
            if (!m_PopupVisible)
                return;

            if (m_Background != null)
            {
                m_Layer.RemoveFromLayer(m_Background);
                m_Background.Destroy();
                m_Background = null;
            }

            if (m_Text != null)
            {
                m_Layer.RemoveFromLayer(m_Text);
                m_Text.Destroy();
                m_Text = null;
            }

            m_PopupVisible = false;
            m_PopupTimer = 0;
        }

        /// <summary>
        /// Gets whether a popup is currently visible.
        /// </summary>
        public bool IsVisible => m_PopupVisible;

        private Scene m_Scene;
        private SceneLayer m_Layer;
        private RectangleShapeObject? m_Background = null;
        private TextObject? m_Text = null;
        private bool m_PopupVisible = false;
        private float m_PopupTimer = 0f;
    }
}
