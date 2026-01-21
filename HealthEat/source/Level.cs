using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HealthEat
{
    internal class Level
    {

        public Level(string name, LevelSupervisor supervisor, bool hasPlayer = false)
        {
            //m_Supervisor = supervisor;
            m_PlayerPresent = hasPlayer;
            m_Name = name;
            m_SceneManager = new SceneManager();

            #if HE_DEBUG
            Console.WriteLine($"[Level]: Created level {name}");
            #endif
        }


        public void Update(float dt)
        {
            //m_InteractionSupervisor.Update();
            //m_Player?.Update();
            m_SceneManager?.Update(dt);
        }

        public void Draw(Window wnd)
        {
            m_SceneManager?.Draw(wnd);
        }

        public virtual void Load()
        {

            m_SceneManager = new SceneManager();
            m_Player = new Player();

            TextObject textObj = new TextObject("Kocham Pati <3", 48, "test1");
            textObj.Context.Position = new HE_Vec2(100f, 100f);

            SpriteObject sprObj = new SpriteObject("menubtn.png", "testSprite");
            sprObj.Context.Position = new HE_Vec2(120f, 120f);
            sprObj.Context.Scale = new HE_Vec2(0.5f, 0.5f);

            StaticEntity ent = new StaticEntity(new HE_Vec2(280.0f, 280.0f), "Enty", "stats-transp.png", 0.7f, true);
            ent.CanInteract = true;

            SceneLayer layer = new SceneLayer("Test layer");
            Scene newScene = new Scene(m_SceneManager, "Test scene");
            newScene.PushLayer(layer);
            ent.SetDebugMode(true);
            ent.Clickable = true;
            ent.SetOnClickAction(() =>
            {
                Console.WriteLine("Kocham Pati");
            });
            ent.SetOnInteractionAction((interactor) =>
            {
                if (interactor.EntityType == HE_EntityType.Player)
                {
                    Console.WriteLine("Player interacted");
                }
            });
            //layer.AddToLayer(textObj);
            //layer.AddToLayer(sprObj);
            //layer.AddEntityToLayer(ent);
            //layer.AddEntityToLayer(m_Player);
            newScene.AddToLayer(textObj, "Test layer");
            newScene.AddToLayer(sprObj, "Test layer");
            newScene.AddToLayer(ent, "Test layer");
            newScene.AddToLayer(m_Player, "Test layer");
            m_Player.SetDebugMode(true);

            m_PlayerLevelInfo.Scene = newScene;
            m_PlayerLevelInfo.Layer = layer;

            m_SceneManager.RegisterScene(newScene);
            //Renderer.SceneToRender = newScene;
            //ent.Move(new HE_Vec2(300f, 20f));

        }


        //public Scene? GetScene(string name)
        //{
        //    return m_SceneManager.GetSceneByName(name);
        //}


        //public Scene? GetActiveScene()
        //{
        //    return m_SceneManager.AcitveScene;
        //}


        //public virtual void InjectPlayer(Player player)
        //{
        //    SceneLayer? l = m_SceneManager.GetSceneByName("Test scene")?.GetLayerByName("Test layer");

        //    if (l == null) return;

        //    l.AddEntityToLayer(player);
        //    player.SetDebugMode(true);

        //}


        public virtual void Close()
        {
            m_PlayerLevelInfo.Scene = null;
            m_PlayerLevelInfo.Layer = null;
        }


        public override bool Equals(object? obj)
        {
            return obj is Level l && l.Name == m_Name;
        }


        public override int GetHashCode()
        {
            return ("Level_" + m_Name).GetHashCode();
        }


        ~Level()
        {
            m_SceneManager?.DestroyScenes();
            m_ShowPopup = false;
        }


        public string Name => m_Name;
        public HE_PlayerInLevelInfo GetPlayerLevelInfo => m_PlayerLevelInfo;

        //private LevelSupervisor m_Supervisor;
        private SceneManager? m_SceneManager = null;
        private Player? m_Player;
        //private SceneLayer? m_PlayerLayer = null;
        //private Scene? m_PlayerScene = null;
        private HE_PlayerInLevelInfo m_PlayerLevelInfo;
        private bool m_ShowPopup = false;
        private bool m_PlayerPresent = false;
        private readonly string m_Name = "";

    }


    internal struct HE_PlayerInLevelInfo
    {
        public Scene? Scene;
        public SceneLayer? Layer;
    }

}
