using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HealthEat.Exceptions;

namespace HealthEat
{
    /// <summary>
    /// Manages entity interactions and clickable objects within a scene.
    /// Handles interaction hints, click detection, and interaction range checking.
    /// </summary>
    internal class InteractionSupervisor
    {
        /// <summary>
        /// Initializes a new InteractionSupervisor for the specified scene.
        /// </summary>
        /// <param name="scene">The scene to manage interactions for.</param>
        /// <param name="player">Optional player reference for interaction range checking.</param>
        public InteractionSupervisor(Scene scene, Player? player = null)
        {

            m_InteractScene = scene;
            m_Player = player;
            m_InteractHint = new SpriteObject(new HE_Vec2(0f, 0f), "interact_hint.png", "Hint_Icon", 0.06f);
            m_InteractHint.Visible = false;
            m_HintLayer = new SceneLayer("Hint_Layer");
            m_HintLayer.AddToLayer(m_InteractHint);

            HE_EventBus.Get.SubscribeToEvent<HE_MouseClickedEvent>(OnClickEvent);

        }


        /// <summary>
        /// Adds an interactable entity to the interaction supervisor.
        /// </summary>
        /// <param name="interactable">The interactable entity to add.</param>
        public void AddInteractable(IInteractable interactable)
        {
            m_Interacatables.Add(interactable);
        }

        /// <summary>
        /// Removes an interactable entity from the interaction supervisor.
        /// </summary>
        /// <param name="interactable">The interactable entity to remove.</param>
        public void RemoveInteractable(IInteractable interactable)
        {
            m_Interacatables.Remove(interactable);
        }

        /// <summary>
        /// Adds a clickable object to the interaction supervisor.
        /// </summary>
        /// <param name="clickable">The clickable object to add.</param>
        public void AddClickable(IClickable clickable)
        {
            m_Clickables.Add(clickable);
        }

        /// <summary>
        /// Removes a clickable object from the interaction supervisor.
        /// </summary>
        /// <param name="clickable">The clickable object to remove.</param>
        public void RemoveClickable(IClickable clickable)
        {
            m_Clickables.Remove(clickable);
        }

        /// <summary>
        /// Updates interaction detection, shows interaction hints, and handles interaction input.
        /// </summary>
        public void Update()
        {
            if (m_Player == null)
                return;

            ClearInteractHint();
            float closest = 2000.0f;
            IInteractable? candidate = null;

            
            foreach (IInteractable i in m_Interacatables)
            {
                if (i == m_Player)
                    continue;

                if (!i.CanInteract)
                    continue;

                float distance = (i.Position - m_Player.Position).GetLength() - m_Player.Body.Width;

                if (distance <= i.InteractRange && distance < closest)
                {
                    closest = distance;
                    m_CanInteract = true;
                    candidate = i;
                }
            }

            m_EntityToInteract = candidate;
            m_CanInteract = (m_EntityToInteract != null);

            if (m_CanInteract)
            {
                ShowInteractHint();

                if (InputController.Get.IsActionTriggered(HE_Action.Interact))
                {
                    m_EntityToInteract?.OnInteraction(m_Player);
                }
            }
        }

        /// <summary>
        /// Handles mouse click events and triggers clickable object callbacks.
        /// </summary>
        /// <param name="e">The mouse clicked event.</param>
        public void OnClickEvent(HE_MouseClickedEvent e)
        {
            for (int i = m_Clickables.Count - 1; i >= 0; i--)
            {
                if (m_Clickables[i].ClickBounds.Contains(e.ClickPos))
                {
                    m_Clickables[i].OnClick(m_Player);
                    return;
                }
            }
        }

        /// <summary>
        /// Handles interaction events from entities.
        /// </summary>
        /// <param name="interactor">The entity attempting to interact.</param>
        public void OnInteractEvent(Entity interactor)
        {
            if (interactor.EntityType != HE_EntityType.Player)
                return;

            if (m_EntityToInteract != null && m_CanInteract)
            {
                m_EntityToInteract.OnInteraction(interactor);
            }
        }

        /// <summary>
        /// Sets the player reference for interaction range checking.
        /// </summary>
        /// <param name="p">The player entity, or null to clear the reference.</param>
        public void SetPlayerRef(Player? p)
        {
            m_Player = p;
        }

        //public void OnInteractPressed(HE_InteractInfo? info = null)
        //{
        //    if (!m_CanInteract)
        //        return;

        //    if (info?.Caller != Player.Get)
        //        throw new HE_InvalidArgumentValueException($"[InteractSv]: {nameof(info)}: Interaction caller is not a player");

        //    if (m_EntityToInteract == null)
        //        throw new HE_LogicException($"[InteractSv]: Interaction entity is null", HE_ExceptionType.Critical);

        //    m_EntityToInteract.OnClick();
        //}


        /// <summary>
        /// Cleans up the interaction supervisor and unsubscribes from events.
        /// </summary>
        public void Exit()
        {
            ClearInteractHint();
            m_Player = null;
            m_Clickables.Clear();
            m_Interacatables.Clear();

            HE_EventBus.Get.UnsubscribeFromEvent<HE_MouseClickedEvent>(OnClickEvent);
        }


        private void ClearInteractHint()
        {
            if (!m_InteractHint.Visible) return;

            m_InteractHint.Visible = false;
            m_InteractScene.PopLayer();
            //layer.RemoveFromLayer(m_InteractHint);
        }


        private void ShowInteractHint()
        {
            //if (m_Added) return;

            m_InteractHint.SetPosition(m_EntityToInteract.Position.X, m_EntityToInteract.Position.Y - 40f);
            m_InteractHint.Visible = true;
            m_InteractScene.PushLayer(m_HintLayer);

        }


        ~InteractionSupervisor()
        {

        }


        private HashSet<IInteractable> m_Interacatables = new HashSet<IInteractable>(10);
        private List<IClickable> m_Clickables = new List<IClickable>(10);
        private IInteractable? m_EntityToInteract = null;
        private SpriteObject m_InteractHint;
        private SceneLayer m_HintLayer;
        private Scene m_InteractScene;
        private Player? m_Player = null;
        private bool m_CanInteract = false;
        private bool m_Added = false;
        private float m_CurrentInteractDistance = 2000.0f;
        private const float m_MaxInteractDistance = 30.0f;
    }
}
