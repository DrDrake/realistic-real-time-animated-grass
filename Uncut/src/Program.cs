using System;

using SlimDX.Direct3D10;

namespace Uncut
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (SlimScene scene = new GrassScene())
            {
                try
                {
                    scene.Run();
                }
                catch (Direct3D10Exception e)
                {
                    Console.WriteLine("Catched Exception in main: " + e.Message);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Catched Exception in main: " + e.Message);
                }
            }
        }
    }
}