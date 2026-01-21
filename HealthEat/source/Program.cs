
#define HE_DEBUG

using System;
using System.Threading;
using SFG = SFML.Graphics;
using SFW = SFML.Window;
using SFML.Graphics;
using HealthEat;

namespace HealthEat
{
    internal class Program
    {

        public Program()
        {

        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Program prog = new Program();
            prog.Run();
            
        }

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
