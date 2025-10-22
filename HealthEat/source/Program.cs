
using System;
using System.Threading;
using SFG = SFML.Graphics;
using SFW = SFML.Window;
using SFML.Graphics;
using HealthEat.Rendering;

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
                width = 800u,
                height = 600u,
                resizable = true,
                VSync = false
            };

            Window window = new Window(winSpec);

            while (window.IsOpen)
            {
                window.HandleEvents();

                //window.Update();
                window.Test();
                Thread.Sleep(10);
            }
        }
    }
}
