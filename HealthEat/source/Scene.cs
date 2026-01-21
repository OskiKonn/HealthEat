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
    internal class Scene
    {
        public Scene(SceneManager sceneManager, string name = "Unnamed")
        {
            m_Name = name;
            //m_SceneManager = sceneManager;
            m_id = sceneManager.ObtainID();
            m_InteractSpv = new InteractionSupervisor(this);
        }

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

                l.Update(dt);
            }

            m_PhyEngine.Update(dt);
            m_InteractSpv.Update();

        }

        public void Draw(Window wnd)
        {
            foreach (SceneLayer l in m_LayerStack)
            {
                l.Draw(wnd);
            }
        }

        public void PushLayer(SceneLayer layer)
        {
            m_LayerStack.Add(layer);
            m_LayerCount++;
        }


        public void PopLayer()
        {
            SceneLayer sceneToRemove = m_LayerStack[^1];
            m_LayerStack.Remove(sceneToRemove);
            m_LayerCount--;
        }

        public bool AddToLayer(IRenderable obj, string layerName)
        {
            SceneLayer? l = GetLayerByName(layerName);
            if (l == null)
                return false;

            if (!l.AddToLayer(obj))
                return false;

            if (obj.ObjectType == HE_RenderObjectType.Entity)
            {
                Entity ent = (Entity)obj;

                if (ent.IsTouchable)
                    m_PhyEngine.AddCollider((ICollider)obj);

                if (ent.IsInteractable)
                    m_InteractSpv.AddInteractable((IInteractable)obj);

                if (ent.EntityType == HE_EntityType.Player)
                    m_InteractSpv.SetPlayerRef((Player)ent);

                if (ent.IsClickable)
                    m_InteractSpv.AddClickable((IClickable)obj);
            }

            return true;
        }

        
        public void RemoveFromLayer(IRenderable obj, string layerName)
        {
            SceneLayer? l = GetLayerByName(layerName);
            if (l == null)
                return;

            l.RemoveFromLayer(obj);

            if (obj.ObjectType == HE_RenderObjectType.Entity)
            {
                Entity ent = (Entity)obj;

                if (ent.IsTouchable)
                    m_PhyEngine.RemoveCollider((ICollider)obj);

                if (ent.IsInteractable)
                    m_InteractSpv.RemoveInteractable((IInteractable)obj);

                if (ent.EntityType == HE_EntityType.Player)
                    m_InteractSpv.SetPlayerRef(null);

                if (ent.IsClickable)
                    m_InteractSpv.AddClickable((IClickable)obj);
            }
        }


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

        public string Name => m_Name;
        public int ID => m_id;
        public IReadOnlyList<SceneLayer> LayerStack => m_LayerStack;
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

    internal class SceneLayer
    {

        public SceneLayer(string name)
        {
            m_Name = name;

            #if HE_DEBUG
            Console.WriteLine($"[Layer]: New SceneLayer ({name}) created");
            #endif
        }

        public void Update(float dt)
        {
            foreach (Entity e in m_LayerEntities)
            {
                e.Update();
            }
        }

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

        public void Draw(Window wnd)
        {
            foreach (IRenderable obj in m_LayerObjects)
            {
                if (obj.Visible)
                    wnd.RenderWindow.Draw(obj.DrawingContext);
            }
        }

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

        public void RemoveFromLayer(IRenderable layerObject)
        {
            if (layerObject.ObjectType == HE_RenderObjectType.Entity)
            {
                RemoveEntityFromLayer((Entity)layerObject);
                return;
            }

            m_LayerObjects.Remove(layerObject);

        }

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

            failed = AddToLayer(entity.Border) ? failed : true;

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

            RemoveFromLayer(entity.Border);

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
