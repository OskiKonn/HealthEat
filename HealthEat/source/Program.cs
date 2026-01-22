
#define HE_DEBUG

using System;
using System.Threading;
using SFG = SFML.Graphics;
using SFW = SFML.Window;
using SFML.Graphics;
using HealthEat;

namespace HealthEat
{
    /// <summary>
    /// Main entry point for the HealthEat application.
    /// Initializes the game window, resource manager, and game manager.
    /// </summary>
    internal class Program
    {

        /// <summary>
        /// Default constructor for Program class.
        /// </summary>
        public Program()
        {

        }

        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        /// <param name="args">Command line arguments passed to the program.</param>
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Program prog = new Program();
            prog.Run();
            
        }

        /// <summary>
        /// Runs the main game loop. Initializes window, resource manager, and game manager.
        /// </summary>
        public void Run()
        {
            WindowSpecification winSpec = new WindowSpecification()
            {
                title = "HealthEat",
                width = 1024u,
                height = 720u,
                resizable = false,
                VSync = false
            };

            ResourceManager resManager = new();
            resManager.LoadResources();

            //InputController ic = new();
            //ic.SetDefaultKeybinds();
            GameManager manager = new GameManager();
            manager.Run();
            //Window window = new Window(winSpec);

            //window.Test();

            //while (window.IsOpen)
            //{
            //    window.HandleEvents();
            //    manager.Update();
            //    window.Update();
                    
                
            //    Thread.Sleep(10);
            //}
        }
    }
}
