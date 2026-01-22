using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HealthEat.Exceptions;

using SFG = SFML.Graphics;
using SFS = SFML.System;
using SFW = SFML.Window;

namespace HealthEat
{
    /// <summary>
    /// Represents a scene containing multiple layers of renderable objects.
    /// Manages physics, interactions, and rendering of scene content.
    /// </summary>
    internal class Scene
    {
        /// <summary>
        /// Initializes a new scene with the specified manager and name.
        /// </summary>
        /// <param name="sceneManager">The scene manager managing this scene.</param>
        /// <param name="name">The name identifier for this scene. Default is "Unnamed".</param>
        public Scene(SceneManager sceneManager, string name = "Unnamed")
        {
            m_Name = name;
            //m_SceneManager = sceneManager;
            m_id = sceneManager.ObtainID();
            m_InteractSpv = new InteractionSupervisor(this);
        }

        /// <summary>
        /// Updates all layers in the scene, physics engine, and interaction supervisor.
        /// </summary>
        /// <param name="dt">Delta time in seconds since last frame.</param>
        public void Update(float dt)
        {
            foreach (SceneLayer l in m_LayerStack)
            {
                l.CleanupEntities((entity) =>
                {
                    if (entity.IsTouchable)
                    {
                        m_PhyEngine.RemoveCollider((ICollider)entity);
                    }
                });

                l.Update();
            }

            m_PhyEngine.Update(dt);
            m_InteractSpv.Update();

        }

        /// <summary>
        /// Clears all layers, physics engine, and interaction supervisor from the scene.
        /// </summary>
        public void Clear()
        {
            m_PhyEngine.Clear();
            m_InteractSpv.Exit();

            foreach (SceneLayer l in m_LayerStack)
            {
                l.Clear();
            }

            m_LayerStack.Clear();
        }

        /// <summary>
        /// Draws all layers in the scene to the window.
        /// </summary>
        /// <param name="wnd">The window to draw to.</param>
        public void Draw(Window wnd)
        {
            foreach (SceneLayer l in m_LayerStack)
            {
                l.Draw(wnd);
            }
        }

        /// <summary>
        /// Adds a new layer to the top of the layer stack.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        public void PushLayer(SceneLayer layer)
        {
            m_LayerStack.Add(layer);
            m_LayerCount++;
        }

        /// <summary>
        /// Removes the topmost layer from the layer stack.
        /// </summary>
        public void PopLayer()
        {
            SceneLayer sceneToRemove = m_LayerStack[^1];
            m_LayerStack.Remove(sceneToRemove);
            m_LayerCount--;
        }

        /// <summary>
        /// Adds a renderable object to a specific layer by name.
        /// </summary>
        /// <param name="obj">The renderable object to add.</param>
        /// <param name="layerName">The name of the layer to add the object to.</param>
        /// <returns>True if the object was successfully added, false otherwise.</returns>
        public bool AddToLayer(IRenderable obj, string layerName)
        {
            SceneLayer? l = GetLayerByName(layerName);
            if (l == null)
                return false;

            if (!l.AddToLayer(obj))
                return false;

            if (obj.IsClickable)
                m_InteractSpv.AddClickable((IClickable)obj);

            if (obj.ObjectType == HE_RenderObjectType.RectangleShape && ((RectangleShapeObject)obj).Collider)
                m_PhyEngine.AddCollider((ICollider)obj);

            if (obj.ObjectType == HE_RenderObjectType.Entity)
            {
                Entity ent = (Entity)obj;

                if (ent.IsTouchable)
                    m_PhyEngine.AddCollider((ICollider)obj);

                if (ent.IsInteractable)
                    m_InteractSpv.AddInteractable((IInteractable)obj);

                if (ent.EntityType == HE_EntityType.Player)
                    m_InteractSpv.SetPlayerRef((Player)ent);

            }

            return true;
        }

        
        /// <summary>
        /// Removes a renderable object from a specific layer by name.
        /// </summary>
        /// <param name="obj">The renderable object to remove.</param>
        /// <param name="layerName">The name of the layer to remove the object from.</param>
        public void RemoveFromLayer(IRenderable obj, string layerName)
        {
            SceneLayer? l = GetLayerByName(layerName);
            if (l == null)
                return;

            l.RemoveFromLayer(obj);

            if (obj.IsClickable)
                m_InteractSpv.AddClickable((IClickable)obj);

            if (obj.ObjectType == HE_RenderObjectType.Entity)
            {
                Entity ent = (Entity)obj;

                if (ent.IsTouchable)
                    m_PhyEngine.RemoveCollider((ICollider)obj);

                if (ent.IsInteractable)
                    m_InteractSpv.RemoveInteractable((IInteractable)obj);

                if (ent.EntityType == HE_EntityType.Player)
                    m_InteractSpv.SetPlayerRef(null);

            }
        }

        /// <summary>
        /// Adds a renderable object to a layer by index.
        /// </summary>
        /// <param name="obj">The renderable object to add.</param>
        /// <param name="nLayer">The index of the layer (0 = topmost layer).</param>
        /// <returns>True if the object was successfully added, false otherwise.</returns>
        /// <exception cref="HE_LogicException">Thrown when there are no layers in the scene.</exception>
        /// <exception cref="HE_InvalidArgumentValueException">Thrown when the layer index is invalid.</exception>
        public bool AddToScene(IRenderable obj, int nLayer = 0)
        {
            if (m_LayerCount == 0)
            {
                throw new HE_LogicException($"[Scene_{m_Name}]: Failed adding object to layer. LayerCount is zero", HE_ExceptionType.Soft);
            }

            if (nLayer > m_LayerCount || nLayer < 0)
                throw new HE_InvalidArgumentValueException($"[Scene_{m_Name}]: Failed adding object to layer {nLayer}. Layer doesn't exist");

            else if (nLayer == 0)
            {
                SceneLayer topLayer = m_LayerStack[^1];
                return topLayer.AddToLayer(obj);
            }

            SceneLayer layer = m_LayerStack[nLayer - 1];
            return layer.AddToLayer(obj);
        }


        /// <summary>
        /// Removes a renderable object from a layer by index.
        /// </summary>
        /// <param name="obj">The renderable object to remove.</param>
        /// <param name="nLayer">The index of the layer (0 = topmost layer).</param>
        /// <exception cref="HE_LogicException">Thrown when there are no layers in the scene.</exception>
        /// <exception cref="HE_InvalidArgumentValueException">Thrown when the layer index is invalid.</exception>
        public void RemoveFromScene(IRenderable obj, int nLayer = 0)
        {
            if (m_LayerCount == 0)
            {
                throw new HE_LogicException($"[Scene_{m_Name}]: Failed removing object from layer. LayerCount is zero", HE_ExceptionType.Soft);
            }

            if (nLayer > m_LayerCount || nLayer < 0)
            {
                throw new HE_InvalidArgumentValueException($"[Scene_{m_Name}]: Failed removing object from layer {nLayer}. Layer doesn't exist");
            }
            else if (nLayer == 0)
            {
                SceneLayer topLayer = m_LayerStack[^1];
                topLayer.RemoveFromLayer(obj);
                return;
            }

            SceneLayer layer = m_LayerStack[nLayer - 1];
            layer.RemoveFromLayer(obj);
            return;
        }

        /// <summary>
        /// Gets a layer by its name.
        /// </summary>
        /// <param name="name">The name of the layer to find.</param>
        /// <returns>The layer with the specified name, or null if not found.</returns>
        public SceneLayer? GetLayerByName(string name)
        {
            foreach (SceneLayer layer in m_LayerStack)
            {
                if (layer.Name == name)
                    return layer;

            }

            return null;
        }


        ~Scene()
        {
            m_LayerStack.Clear();
            m_LayerCount = 0;
        }

        /// <summary>
        /// Gets the name of this scene.
        /// </summary>
        public string Name => m_Name;
        /// <summary>
        /// Gets the unique identifier for this scene.
        /// </summary>
        public int ID => m_id;
        /// <summary>
        /// Gets a read-only list of all layers in this scene.
        /// </summary>
        public IReadOnlyList<SceneLayer> LayerStack => m_LayerStack;
        /// <summary>
        /// Gets the number of layers in this scene.
        /// </summary>
        public int LayerCount => LayerCount;
        //public SceneLayer? GetPlayerLayer => m_PlayerLayer;
        //public bool IsPlayerPresent { get; set; }

        //private readonly SceneManager m_SceneManager;
        private readonly string m_Name;
        private int m_id;
        private int m_LayerCount = 0;
        private List<SceneLayer> m_LayerStack = new List<SceneLayer>(1);
        private PhysicsEngine m_PhyEngine = new PhysicsEngine();
        private InteractionSupervisor m_InteractSpv;
    }

    /// <summary>
    /// Represents a layer within a scene that contains renderable objects.
    /// Manages entities, rendering order, and object lifecycle.
    /// </summary>
    internal class SceneLayer
    {

        /// <summary>
        /// Initializes a new scene layer with the specified name.
        /// </summary>
        /// <param name="name">The name identifier for this layer.</param>
        public SceneLayer(string name)
        {
            m_Name = name;

            #if HE_DEBUG
            Console.WriteLine($"[Layer]: New SceneLayer ({name}) created");
            #endif
        }

        /// <summary>
        /// Updates all renderable objects in this layer.
        /// </summary>
        public virtual void Update()
        {
            foreach (IRenderable r in m_LayerObjects)
            {
                r.Update();
            }
        }

        /// <summary>
        /// Clears all objects and entities from this layer.
        /// </summary>
        public void Clear()
        {
            m_LayerObjects.Clear();
            m_LayerEntities.Clear();
            m_TouchableEntities.Clear();
            m_InteractableEntities.Clear();
            m_ClickableEntities.Clear();
        }

        /// <summary>
        /// Removes expired entities from the layer and invokes cleanup action.
        /// </summary>
        /// <param name="onEntityCleanup">Action to perform when cleaning up an entity.</param>
        public void CleanupEntities(Action<Entity> onEntityCleanup)
        {
            foreach (Entity e in m_LayerEntities)
            {
                if (e.IsExpired)
                {
                    onEntityCleanup.Invoke(e);
                    RemoveEntityFromLayer(e);
                }    
            }

        }

        /// <summary>
        /// Draws all visible objects in this layer to the window.
        /// </summary>
        /// <param name="wnd">The window to draw to.</param>
        public void Draw(Window wnd)
        {
            foreach (IRenderable obj in m_LayerObjects)
            {
                if (obj.Visible)
                    obj.Draw(wnd);
            }
        }

        /// <summary>
        /// Adds a renderable object to this layer.
        /// </summary>
        /// <param name="renderObject">The renderable object to add.</param>
        /// <returns>True if the object was successfully added, false otherwise.</returns>
        public bool AddToLayer(IRenderable renderObject)
        {

            if (renderObject.ObjectType == HE_RenderObjectType.Entity)
                return AddEntityToLayer((Entity)renderObject);

            if (!m_LayerObjects.Add(renderObject))
            {

                #if HE_DEBUG
                Console.WriteLine($"[Layer_{m_Name}]: Failed adding RenderObject {renderObject.Name} to renderer. Make sure to not duplicate object name");
                #endif
                return false;
            }

            #if HE_DEBUG
            Console.WriteLine($"[Layer_{m_Name}]: Added layer object:  {{ {renderObject.Name}, {renderObject.ID} }}");
            #endif

            return true;

        }

        /// <summary>
        /// Removes a renderable object from this layer.
        /// </summary>
        /// <param name="layerObject">The renderable object to remove.</param>
        public void RemoveFromLayer(IRenderable layerObject)
        {
            if (layerObject.ObjectType == HE_RenderObjectType.Entity)
            {
                RemoveEntityFromLayer((Entity)layerObject);
                return;
            }

            m_LayerObjects.Remove(layerObject);

        }

        /// <summary>
        /// Adds an entity to this layer and registers it with appropriate systems.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>True if the entity was successfully added, false otherwise.</returns>
        public bool AddEntityToLayer(Entity entity)
        {
            bool failed = false;

            failed = m_LayerObjects.Add(entity) ? failed : true;
            failed = m_LayerEntities.Add(entity) ? failed : true;


            if (entity.IsTouchable)
                failed = AddEntityToPropertySet(entity, HE_EntityProperty.Touchable) ? failed : true;

            if (entity.IsInteractable)
                failed = AddEntityToPropertySet(entity, HE_EntityProperty.Interactable) ? failed : true;

            if (entity.IsClickable)
                failed = AddEntityToPropertySet(entity, HE_EntityProperty.Clickable) ? failed : true;

            if (failed)
            {
                #if HE_DEBUG
                Console.WriteLine($"[Layer_{m_Name}]: Failed adding entity {entity.Name} to layer");
#               endif

                RemoveEntityFromLayer(entity);
                return false;
            }

            //entity.PropertyChangedEvent += OnEntityPropertyChange;

            #if HE_DEBUG
            Console.WriteLine($"[Layer_{m_Name}]: Added entity {entity.Name} to layer");
            #endif

            return true;
        }

        /// <summary>
        /// Removes an entity from this layer and unregisters it from appropriate systems.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        public void RemoveEntityFromLayer(Entity entity)
        {
            bool removed = false;

            removed = m_LayerObjects.Remove(entity);
            m_LayerEntities.Remove(entity);

            if (entity.IsTouchable)
                RemoveEntityFromPropertySet(entity, HE_EntityProperty.Touchable);

            if (entity.IsInteractable)
                RemoveEntityFromPropertySet(entity, HE_EntityProperty.Interactable);

            if (entity.IsClickable)
                RemoveEntityFromPropertySet(entity, HE_EntityProperty.Clickable);

            //if (removed)
            //    entity.PropertyChangedEvent -= OnEntityPropertyChange;

            //#if HE_DEBUG
            //Console.WriteLine($"[Layer_{m_Name}]: Removed entity {entity.Name} from layer");
            //#endif

        }

        //public void SetMasterScene(Scene? sc, uint layerDepth = 0u)
        //{
        //    m_MasterScene = sc;
        //    m_Name = (sc != null) ? sc.Name : "";
        //}

        /// <summary>
        /// Gets an entity by its name from this layer.
        /// </summary>
        /// <param name="name">The name of the entity to find.</param>
        /// <returns>The entity with the specified name, or null if not found.</returns>
        public Entity? GetEntity(string name)
        {
            foreach (Entity ent in m_LayerEntities)
            {
                if (ent.Name == name)
                    return ent;
            }

            return null;
        }

        private bool AddEntityToSet(Entity entity, HashSet<Entity> set)
        {
            if (!set.Add(entity))
            {
                #if HE_DEBUG
                Console.WriteLine($"[Layer_{m_Name}]: Failed adding entity to set");
                #endif

                return false;
            }

            return true;
        }

        private bool RemoveEntityFromSet(Entity entity, HashSet<Entity> set)
        {
            if (!set.Remove(entity))
            {
                #if HE_DEBUG
                Console.WriteLine($"[Layer_{m_Name}]: Failed removing entity from layer set");
                #endif

                return false;
            }

            return true;
        }

        private bool RemoveEntityFromPropertySet(Entity entity, HE_EntityProperty property)
        {
            switch (property)
            {
                case HE_EntityProperty.Touchable:
                    return RemoveEntityFromSet(entity, m_TouchableEntities);

                case HE_EntityProperty.Interactable:
                    return RemoveEntityFromSet(entity, m_InteractableEntities);

                case HE_EntityProperty.Clickable:
                    return RemoveEntityFromSet(entity, m_ClickableEntities);

                default:
                    return false;
            }
        }

        private bool AddEntityToPropertySet(Entity entity, HE_EntityProperty property)
        {
            switch (property)
            {
                case HE_EntityProperty.Touchable:
                    return AddEntityToSet(entity, m_TouchableEntities);

                case HE_EntityProperty.Interactable:
                    return AddEntityToSet(entity, m_InteractableEntities);

                case HE_EntityProperty.Clickable:
                    return AddEntityToSet(entity, m_ClickableEntities);

                default:
                    return false;
            }
        }

        private void OnEntityPropertyChange(object? s, HE_PropertyChangedEvent args)
        {
            if (args.Value == false)
                RemoveEntityFromPropertySet(args.Entity, args.Property);

            else if (args.Value == true)
                RemoveEntityFromPropertySet(args.Entity, args.Property);
        }

        ~SceneLayer()
        {
            m_LayerObjects.Clear();
            m_LayerEntities.Clear();
            m_InteractableEntities.Clear();
            m_ClickableEntities.Clear();
            m_TouchableEntities.Clear();

            //m_MasterScene = null;
            m_Name = "";

            #if HE_DEBUG
            Console.WriteLine($"[Layer_{m_Name}]: Layer Destroyed");
            #endif
        }

        //public IReadOnlyCollection<IRenderable> LayerObjects => m_LayerObjects;
        //public IReadOnlyCollection<Entity> GetEntities => m_LayerEntities;
        //public IReadOnlyCollection<Entity> GetTouchableEntities => m_TouchableEntities;
        //public IReadOnlyCollection<Entity> GetInteractableEntities => m_InteractableEntities;
        //public IReadOnlyCollection<Entity> GetClickableEntities => m_ClickableEntities;
        //public Scene? MasterScene => m_MasterScene;
        //public string MasterName => m_Name;
        public string Name => m_Name;

        private HashSet<IRenderable> m_LayerObjects = new HashSet<IRenderable>(10);
        private HashSet<Entity> m_LayerEntities = new HashSet<Entity>(10);
        private HashSet<Entity> m_TouchableEntities = new HashSet<Entity>(5);
        private HashSet<Entity> m_InteractableEntities = new HashSet<Entity>(5);
        private HashSet<Entity> m_ClickableEntities = new HashSet<Entity>(5);

        //private Scene? m_MasterScene = null;
        private string m_Name = "";
    }

}
