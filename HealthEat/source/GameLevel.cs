using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthEat
{
    /// <summary>
    /// Base class for game levels that use scene management.
    /// </summary>
    internal class GameLevel : Level
    {
        /// <summary>
        /// Initializes a new GameLevel with the specified name and supervisor.
        /// </summary>
        /// <param name="name">The name identifier for this level.</param>
        /// <param name="supervisor">The level supervisor managing this level.</param>
        public GameLevel(string name, LevelSupervisor supervisor) : base(name, supervisor)
        {
            m_SceneManager = new SceneManager();
        }
    }

    /// <summary>
    /// Main menu level displaying start and exit buttons.
    /// </summary>
    internal class MenuLevel : GameLevel
    {
        /// <summary>
        /// Initializes a new MenuLevel.
        /// </summary>
        /// <param name="supervisor">The level supervisor managing this level.</param>
        public MenuLevel(LevelSupervisor supervisor) : base("Menu_Level", supervisor)
        {

        }

        public override void Load()
        {
            float midX = 1024 / 2;
            float posY = 720 / 2 - 80;

            Scene mainScene = new Scene(m_SceneManager, "Main");
            SceneLayer mainLayer = new SceneLayer("Main_layer");
            mainScene.PushLayer(mainLayer);

            StaticEntity btnStart = new StaticEntity(new HE_Vec2(midX, posY), "btn_start", "btn-start.png", 1);
            btnStart.Clickable = true;
            btnStart.SetOnClickAction((e) =>
            {
                m_Supervisor.LoadLevel(new HouseLevel(m_Supervisor));
            });
            mainScene.AddToLayer(btnStart, mainLayer.Name);

            //StaticEntity btnMenu = new StaticEntity(new HE_Vec2(midX, posY + 50f), "btn_menu", "btn-menu.png", 1);
            //btnMenu.Clickable = true;
            //btnMenu.SetDefaultClickAction();
            //mainScene.AddToLayer(btnMenu, mainLayer.Name);

            //StaticEntity btnSettings = new StaticEntity(new HE_Vec2(midX, posY + 100f), "btn_settings", "btn-settings.png", 1);
            //btnSettings.SetTexture("btn-settings.png");
            //btnSettings.Clickable = true;
            //btnSettings.SetDefaultClickAction();
            //mainScene.AddToLayer(btnSettings, mainLayer.Name);

            StaticEntity btnExit = new StaticEntity(new HE_Vec2(midX, posY + 150f), "btn_exit", "btn-exit.png", 1);
            btnExit.Clickable = true;
            btnExit.SetOnClickAction((e) =>
            {
                HE_EventBus.Get.BroadcastEvent(new HE_ExitEvent());
            });
            mainScene.AddToLayer(btnExit, mainLayer.Name);

            m_SceneManager.RegisterScene(mainScene);

        }
    }


    /// <summary>
    /// Main house level where the player can interact with various objects.
    /// Contains kitchen table, TV, and doors for accessing other levels.
    /// </summary>
    internal class HouseLevel : GameLevel
    {
        private PopupSystem? m_PopupSystem = null;
        private float m_PopupTimer = 0f;
        private bool m_ShowEnergyWarning = false;

        /// <summary>
        /// Initializes a new HouseLevel.
        /// </summary>
        /// <param name="sv">The level supervisor managing this level.</param>
        public HouseLevel(LevelSupervisor sv) : base("House_Level", sv)
        {

        }

        public override void Load()
        {
            Scene mainScene = new Scene(m_SceneManager, "Main");
            SceneLayer mainLayer = new SceneLayer("Main_layer");
            SceneLayer popupLayer = new SceneLayer("Popup_Layer");
            HUDLayer hudLayer = new HUDLayer(m_SceneManager);
            hudLayer.LoadHUD(mainScene);
            mainScene.PushLayer(mainLayer);
            mainScene.PushLayer(popupLayer);
            mainScene.PushLayer(hudLayer);

            m_PopupSystem = new PopupSystem(mainScene, popupLayer);
            m_PopupTimer = 0f;
            m_ShowEnergyWarning = false;

            m_Supervisor.GetPlayer().SetPosition(1024 / 2, 720 / 2);
            m_Supervisor.GetPlayer().SetScale(0.6f);
            //m_Supervisor.GetPlayer().SetDebugMode(true);

            RectangleShapeObject map = new RectangleShapeObject(new HE_Vec2(1024 / 2, 720 / 2), new HE_Vec2(1024, 720), "map");
            map.SetTexture("house.png");
            //map.SetDebugMode(true);
            mainScene.AddToLayer(map, mainLayer.Name);

            InvisibleCollider leftWall = new InvisibleCollider(new HE_Vec2(53, 359), new HE_Vec2(54, 712));
            //leftWall.SetDebugMode(true);
            leftWall.SetDefaultClickAction();
            mainScene.AddToLayer(leftWall, mainLayer.Name);

            InvisibleCollider rightWall = new InvisibleCollider(new HE_Vec2(974, 355), new HE_Vec2(54, 712));
            //rightWall.SetDebugMode(true);
            mainScene.AddToLayer(rightWall, mainLayer.Name);

            InvisibleCollider bottomWall = new InvisibleCollider(new HE_Vec2(519, 633), new HE_Vec2(995.27f, 53.36f));
            //bottomWall.SetDebugMode(true);
            bottomWall.SetDefaultClickAction();
            mainScene.AddToLayer(bottomWall, mainLayer.Name);

            InvisibleCollider topWall = new InvisibleCollider(new HE_Vec2(519, 14), new HE_Vec2(995.27f, 53.36f));
            //topWall.SetDebugMode(true);
            mainScene.AddToLayer(topWall, mainLayer.Name);

            InvisibleCollider kitchenBlocker = new InvisibleCollider(new HE_Vec2(299, 182), new HE_Vec2(384.87f, 33.012f));
            //kitchenBlocker.SetDebugMode(true);
            mainScene.AddToLayer(kitchenBlocker, mainLayer.Name);

            InvisibleCollider doorBlocker = new InvisibleCollider(new HE_Vec2(729, 149), new HE_Vec2(384.87f, 33.012f));
            //doorBlocker.SetDebugMode(true);
            mainScene.AddToLayer(doorBlocker, mainLayer.Name);

            InvisibleCollider blokerBottom = new InvisibleCollider(new HE_Vec2(511f, 515f), new HE_Vec2(49.56f, 195.88f));
            //blokerBottom.SetDebugMode(true);
            mainScene.AddToLayer(blokerBottom, mainLayer.Name);

            InvisibleCollider blokerTop = new InvisibleCollider(new HE_Vec2(508f, 163f), new HE_Vec2(49.56f, 195.88f));
            //blokerTop.SetDebugMode(true);
            mainScene.AddToLayer(blokerTop, mainLayer.Name);

            InvisibleCollider blokerCouchFront = new InvisibleCollider(new HE_Vec2(737, 479), new HE_Vec2(181.44f, 18.72f));
            //blokerCouchFront.SetDebugMode(true);
            mainScene.AddToLayer(blokerCouchFront, mainLayer.Name);

            InvisibleCollider blokerCouchLeft = new InvisibleCollider(new HE_Vec2(646, 494), new HE_Vec2(21.98f, 51.24f));
            //blokerCouchLeft.SetDebugMode(true);
            mainScene.AddToLayer(blokerCouchLeft, mainLayer.Name);

            InvisibleCollider blokerCouchRight = new InvisibleCollider(new HE_Vec2(822, 494), new HE_Vec2(21.98f, 51.24f));
            //blokerCouchRight.SetDebugMode(true);
            mainScene.AddToLayer(blokerCouchRight, mainLayer.Name);

            InvisibleCollider blokerComode = new InvisibleCollider(new HE_Vec2(891, 522), new HE_Vec2(64.38f, 52.14f));
            //blokerComode.SetDebugMode(true);
            mainScene.AddToLayer(blokerComode, mainLayer.Name);

            InvisibleCollider blokerTable = new InvisibleCollider(new HE_Vec2(736, 394), new HE_Vec2(116.18f, 74.74f));
            //blokerTable.SetDebugMode(true);
            mainScene.AddToLayer(blokerTable, mainLayer.Name);

            InvisibleCollider blokerKitchenTable = new InvisibleCollider(new HE_Vec2(318, 370), new HE_Vec2(102.90f, 109.2f));
            //blokerKitchenTable.SetDebugMode(true);
            mainScene.AddToLayer(blokerKitchenTable, mainLayer.Name);

            InvisibleCollider blokerChair = new InvisibleCollider(new HE_Vec2(236, 352), new HE_Vec2(48.72f, 67.76f));
            //blokerChair.SetDebugMode(true);
            mainScene.AddToLayer(blokerChair, mainLayer.Name);

            InvisibleCollider blokerFlowerLeft = new InvisibleCollider(new HE_Vec2(575, 221), new HE_Vec2(48.72f, 67.76f));
            //blokerFlowerLeft.SetDebugMode(true);
            mainScene.AddToLayer(blokerFlowerLeft, mainLayer.Name);

            InvisibleCollider blokerFlowerRight = new InvisibleCollider(new HE_Vec2(897, 208), new HE_Vec2(48.72f, 67.76f));
            //blokerFlowerRight.SetDebugMode(true);
            mainScene.AddToLayer(blokerFlowerRight, mainLayer.Name);

            StaticEntity kitchenTable = new StaticEntity(new HE_Vec2(294, 200), "Kitchen_table", "blank.png", 1f, true, false);
            //kitchenTable.SetDebugMode(true);
            kitchenTable.SetSize(new HE_Vec2(98.1f, 79.8f));
            kitchenTable.CanTouch = false;
            kitchenTable.SetOnInteractionAction((e) =>
            {
                m_Supervisor.LoadLevel(new KitchenLevel(m_Supervisor));
            });
            mainScene.AddToLayer(kitchenTable, mainLayer.Name);

            StaticEntity doors = new StaticEntity(new HE_Vec2(758, 148), "door", "blank.png", 1f, true, true);
            //doors.SetDebugMode(true);
            doors.SetSize(new HE_Vec2(113.9f, 120.7f));
            doors.CanTouch = false;
            doors.SetOnInteractionAction((e) =>
            {
                Player player = m_Supervisor.GetPlayer();
                if (player.Data.Energy < 20)
                {
                    m_PopupSystem?.ShowPopup("Jesteś zbyt zmęczony! Potrzebujesz przynajmniej 20 energii, aby iść biegać.", 2.5f);
                    m_ShowEnergyWarning = true;
                    m_PopupTimer = 0f;
                }
                else
                {
                    m_Supervisor.LoadLevel(new RunningLevel(m_Supervisor));
                }
            });
            mainScene.AddToLayer(doors, mainLayer.Name);

            StaticEntity tv = new StaticEntity(new HE_Vec2(739, 554), "tv", "blank.png", 1f, true, true);
            //tv.SetDebugMode(true);
            tv.SetSize(new HE_Vec2(118.08f, 75.44f));
            tv.SetOnInteractionAction((e) =>
            {
                m_Supervisor.LoadLevel(new TVLevel(m_Supervisor));
            });
            mainScene.AddToLayer(tv, mainLayer.Name);

            mainScene.AddToLayer(m_Supervisor.GetPlayer(), mainLayer.Name);
            m_SceneManager.RegisterScene(mainScene);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            m_PopupSystem?.Update(dt);

            if (m_ShowEnergyWarning)
            {
                m_PopupTimer += dt;
                if (m_PopupTimer >= 2.5f)
                {
                    m_PopupSystem?.HidePopup();
                    m_ShowEnergyWarning = false;
                    m_PopupTimer = 0f;
                }
            }
        }
    }


    /// <summary>
    /// Kitchen level where players can click on food items.
    /// Healthy foods increase health and energy, unhealthy foods decrease them.
    /// </summary>
    internal class KitchenLevel(LevelSupervisor sv) : GameLevel("Kitchen_Level", sv)
    {
        private PopupSystem? m_PopupSystem = null;
        private float m_ReturnTimer = 0f;
        private bool m_ShouldReturn = false;

        /// <summary>
        /// Loads the kitchen level with food items and popup system.
        /// </summary>
        public override void Load()
        {
            Scene mainScene = new Scene(m_SceneManager, "Main_Scene");
            SceneLayer mainLayer = new SceneLayer("Main_Layer");
            SceneLayer popupLayer = new SceneLayer("Popup_Layer");
            mainScene.PushLayer(mainLayer);
            mainScene.PushLayer(popupLayer);

            m_PopupSystem = new PopupSystem(mainScene, popupLayer);
            m_ReturnTimer = 0f;
            m_ShouldReturn = false;

            RectangleShapeObject table = new RectangleShapeObject(new HE_Vec2(1024 / 2, 720 / 2), new HE_Vec2(1024, 720), "food-table");
            table.SetTexture("food-table.png");
            mainScene.AddToLayer(table, mainLayer.Name);

            // Healthy foods (apple, etc.)
            StaticEntity apple = new StaticEntity(new HE_Vec2(300, 300), "Apple", "apple.png", 0.5f, false, true);
            apple.SetSize(new HE_Vec2(80, 80));
            apple.SetOnClickAction((e) =>
            {
                OnFoodClicked(true, "Apple", "Zdrowy wybór!");
            });
            mainScene.AddToLayer(apple, mainLayer.Name);

            // Unhealthy foods (ice cream, etc.)
            StaticEntity icecream = new StaticEntity(new HE_Vec2(500, 300), "IceCream", "icecream.png", 0.5f, false, true);
            icecream.SetSize(new HE_Vec2(80, 80));
            icecream.SetOnClickAction((e) =>
            {
                OnFoodClicked(false, "Ice Cream", "Niezdrowe... Spróbuj czegoś innego!");
            });
            mainScene.AddToLayer(icecream, mainLayer.Name);

            // More food items
            StaticEntity apple2 = new StaticEntity(new HE_Vec2(700, 300), "Apple2", "apple.png", 0.5f, false, true);
            apple2.SetSize(new HE_Vec2(80, 80));
            apple2.SetOnClickAction((e) =>
            {
                OnFoodClicked(true, "Apple", "Zdrowy wybór!");
            });
            mainScene.AddToLayer(apple2, mainLayer.Name);

            StaticEntity icecream2 = new StaticEntity(new HE_Vec2(400, 400), "IceCream2", "icecream.png", 0.5f, false, true);
            icecream2.SetSize(new HE_Vec2(80, 80));
            icecream2.SetOnClickAction((e) =>
            {
                OnFoodClicked(false, "Ice Cream", "Niezdrowe... Spróbuj czegoś innego!");
            });
            mainScene.AddToLayer(icecream2, mainLayer.Name);

            StaticEntity apple3 = new StaticEntity(new HE_Vec2(600, 400), "Apple3", "apple.png", 0.5f, false, true);
            apple3.SetSize(new HE_Vec2(80, 80));
            apple3.SetOnClickAction((e) =>
            {
                OnFoodClicked(true, "Apple", "Zdrowy wybór!");
            });
            mainScene.AddToLayer(apple3, mainLayer.Name);

            m_SceneManager.RegisterScene(mainScene);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            m_PopupSystem?.Update(dt);

            if (m_ShouldReturn)
            {
                m_ReturnTimer += dt;
                if (m_ReturnTimer >= 2.5f)
                {
                    LoadBack();
                }
            }
        }

        private void OnFoodClicked(bool isHealthy, string foodName, string message)
        {
            Player player = m_Supervisor.GetPlayer();
            
            if (isHealthy)
            {
                player.UpdateHealth(10);
                player.UpdateEnergy(5);
                player.UpdateScore(20);
                m_PopupSystem?.ShowPopup(message, 2.0f);
            }
            else
            {
                player.UpdateHealth(-10);
                player.UpdateEnergy(-5);
                player.UpdateScore(5);
                m_PopupSystem?.ShowPopup(message, 2.0f);
            }

            m_ShouldReturn = true;
            m_ReturnTimer = 0f;
        }

        /// <summary>
        /// Returns to the house level after completing the kitchen interaction.
        /// </summary>
        public void LoadBack()
        {
            m_PopupSystem?.HidePopup();
            m_ShouldReturn = false;
            m_ReturnTimer = 0f;
            m_Supervisor.LoadLevel(new HouseLevel(m_Supervisor));
        }
    }

    /// <summary>
    /// TV level where watching TV decreases health but adds a bit of energy.
    /// </summary>
    internal class TVLevel : GameLevel
    {
        private PopupSystem? m_PopupSystem = null;
        private float m_ReturnTimer = 0f;
        private bool m_ShouldReturn = false;

        /// <summary>
        /// Initializes a new TVLevel.
        /// </summary>
        /// <param name="sv">The level supervisor managing this level.</param>
        public TVLevel(LevelSupervisor sv) : base("TV_Level", sv)
        {
        }

        /// <summary>
        /// Loads the TV level and applies health penalty and energy bonus.
        /// </summary>
        public override void Load()
        {
            Scene mainScene = new Scene(m_SceneManager, "Main_Scene");
            SceneLayer mainLayer = new SceneLayer("Main_Layer");
            SceneLayer popupLayer = new SceneLayer("Popup_Layer");
            mainScene.PushLayer(mainLayer);
            mainScene.PushLayer(popupLayer);

            m_PopupSystem = new PopupSystem(mainScene, popupLayer);
            m_ReturnTimer = 0f;
            m_ShouldReturn = false;

            // Background
            RectangleShapeObject bg = new RectangleShapeObject(new HE_Vec2(1024 / 2, 720 / 2), new HE_Vec2(1024, 720), "tv_bg");
            bg.Context.FillColor = new HE_Color(20, 20, 40);
            mainScene.AddToLayer(bg, mainLayer.Name);

            // TV screen representation
            RectangleShapeObject tvScreen = new RectangleShapeObject(new HE_Vec2(1024 / 2, 720 / 2), new HE_Vec2(800, 500), "tv_screen");
            tvScreen.Context.FillColor = new HE_Color(10, 10, 30);
            tvScreen.Context.OutlineColor = HE_Color.White;
            tvScreen.Context.OutlineThickness = 5.0f;
            mainScene.AddToLayer(tvScreen, mainLayer.Name);

            // Show popup and apply effects
            Player player = m_Supervisor.GetPlayer();
            player.UpdateEnergy(5); // Add a bit of energy
            player.UpdateHealth(-5); // Subtract health
            player.UpdateScore(10);
            m_PopupSystem.ShowPopup("Oglądałeś telewizję przez chwilę... Straciłeś trochę zdrowia.", 2.0f);

            m_ShouldReturn = true;
            m_ReturnTimer = 0f;

            m_SceneManager.RegisterScene(mainScene);
        }

        /// <summary>
        /// Updates the TV level, popup system, and handles automatic return to house.
        /// </summary>
        /// <param name="dt">Delta time in seconds since last frame.</param>
        public override void Update(float dt)
        {
            base.Update(dt);
            m_PopupSystem?.Update(dt);

            if (m_ShouldReturn)
            {
                m_ReturnTimer += dt;
                if (m_ReturnTimer >= 2.5f)
                {
                    LoadBack();
                }
            }
        }

        /// <summary>
        /// Returns to the house level after completing the TV interaction.
        /// </summary>
        private void LoadBack()
        {
            m_PopupSystem?.HidePopup();
            m_ShouldReturn = false;
            m_ReturnTimer = 0f;
            m_Supervisor.LoadLevel(new HouseLevel(m_Supervisor));
        }
    }

    /// <summary>
    /// Running level where going for a run decreases energy but significantly increases health.
    /// </summary>
    internal class RunningLevel : GameLevel
    {
        private PopupSystem? m_PopupSystem = null;
        private float m_ReturnTimer = 0f;
        private bool m_ShouldReturn = false;

        /// <summary>
        /// Initializes a new RunningLevel.
        /// </summary>
        /// <param name="sv">The level supervisor managing this level.</param>
        public RunningLevel(LevelSupervisor sv) : base("Running_Level", sv)
        {
        }

        /// <summary>
        /// Loads the running level and applies energy/health changes.
        /// </summary>
        public override void Load()
        {
            Scene mainScene = new Scene(m_SceneManager, "Main_Scene");
            SceneLayer mainLayer = new SceneLayer("Main_Layer");
            SceneLayer popupLayer = new SceneLayer("Popup_Layer");
            mainScene.PushLayer(mainLayer);
            mainScene.PushLayer(popupLayer);

            m_PopupSystem = new PopupSystem(mainScene, popupLayer);
            m_ReturnTimer = 0f;
            m_ShouldReturn = false;

            // Background
            RectangleShapeObject bg = new RectangleShapeObject(new HE_Vec2(1024 / 2, 720 / 2), new HE_Vec2(1024, 720), "running_bg");
            bg.Context.FillColor = new HE_Color(100, 150, 100);
            mainScene.AddToLayer(bg, mainLayer.Name);

            // Show popup and apply effects
            Player player = m_Supervisor.GetPlayer();
            player.UpdateEnergy(-20);
            player.UpdateHealth(25); // Significant health boost
            player.UpdateScore(30);
            m_PopupSystem.ShowPopup("Postanowiłeś iść pobiegać. Zdrowie wzrosło!", 2.5f);

            m_ShouldReturn = true;
            m_ReturnTimer = 0f;

            m_SceneManager.RegisterScene(mainScene);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            m_PopupSystem?.Update(dt);

            if (m_ShouldReturn)
            {
                m_ReturnTimer += dt;
                if (m_ReturnTimer >= 3.0f)
                {
                    LoadBack();
                }
            }
        }

        private void LoadBack()
        {
            m_PopupSystem?.HidePopup();
            m_ShouldReturn = false;
            m_ReturnTimer = 0f;
            m_Supervisor.LoadLevel(new HouseLevel(m_Supervisor));
        }
    }


    /// <summary>
    /// Specialized scene layer for displaying the HUD (Heads-Up Display).
    /// Shows player health, energy, and score.
    /// </summary>
    internal class HUDLayer : SceneLayer
    {
        /// <summary>
        /// Initializes a new HUDLayer and subscribes to player data change events.
        /// </summary>
        /// <param name="manager">The scene manager (unused, kept for compatibility).</param>
        public HUDLayer(SceneManager manager) : base("HUD_Layer")
        {
            HE_EventBus.Get.SubscribeToEventForce<HE_PlayerDataChangedEvent>(OnPlayerDataChanged);
        }

        /// <summary>
        /// Loads HUD elements (health, energy, score displays) into the scene.
        /// </summary>
        /// <param name="sc">The scene to add HUD elements to.</param>
        public void LoadHUD(Scene sc)
        {
            sc.PushLayer(this);

            StaticEntity health = new StaticEntity(new HE_Vec2(40, 76), "Health", "heart.png", 0.39f, false, true);
            //health.SetTexture("energy.png");
            health.SetDefaultClickAction();
            sc.AddToLayer(health, this.Name);

            m_HP.SetPosition(40, 130);
            m_HP.SetDefaultClickAction();
            m_HP.Clickable = true;
            m_HP.Context.FillColor = HE_Color.White;
            sc.AddToLayer(m_HP, this.Name);

            StaticEntity energy = new StaticEntity(new HE_Vec2(38, 210), "Energy", "energy.png", 0.43f, false, true);
            //energy.SetTexture("energy.png");
            energy.SetDefaultClickAction();
            sc.AddToLayer(energy, this.Name);

            m_EnergyScore.SetPosition(40, 272);
            m_EnergyScore.Context.FillColor = HE_Color.White;
            sc.AddToLayer(m_EnergyScore, this.Name);

            StaticEntity points = new StaticEntity(new HE_Vec2(38, 351), "Points", "star-icon.png", 0.39f, false, true);
            //health.SetTexture("energy.png");
            points.SetDefaultClickAction();
            sc.AddToLayer(points, this.Name);

            m_PointsScore.SetPosition(40, 410);
            m_PointsScore.Context.FillColor = HE_Color.White;
            sc.AddToLayer(m_PointsScore, this.Name);
        }


        /// <summary>
        /// Handles player data change events and updates HUD displays.
        /// </summary>
        /// <param name="e">The player data changed event containing updated stats.</param>
        public void OnPlayerDataChanged(HE_PlayerDataChangedEvent e)
        {
            m_HP.SetText(e.Data.Health.ToString());
            m_EnergyScore.SetText(e.Data.Energy.ToString());
            m_PointsScore.SetText(e.Data.Score.ToString());
        }

        private TextObject m_HP = new TextObject("100", 24, "hp");
        private TextObject m_EnergyScore = new TextObject("100", 24, "energy-score");
        private TextObject m_PointsScore = new TextObject("100", 24, "points-score");
    }

}
