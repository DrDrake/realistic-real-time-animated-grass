using System;

namespace Uncut
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (SlimSceneController sceneController = new TestViewController()) //SampleTriangleController, SampleJupiterController
            {
                sceneController.Run();
            }
        }
    }
}