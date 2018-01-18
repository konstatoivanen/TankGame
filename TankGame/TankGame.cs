using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;

namespace TankGame
{
    class TankGame
    {
        private static int startTime = 0;
        private static GameWindow game = new GameWindow();

        //Get time since start
        private static float time
        {
            get { return Environment.TickCount / 1000.0f; }
        }

        private static float aspectRatio
        {
            get { return (float)game.Width / game.Height; }
        }
      

        [STAThread]
        public static void Main()
        {
            float lastTime = time;

            using (game)
            {
                game.Load += (sender, e) =>
                {
                    game.VSync = VSyncMode.On;
                    startTime = Environment.TickCount;
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {

                    float curTime = time;
                    float delta = curTime - lastTime;
                    delta = Math.Max(0.01f, delta); //Min Frame deltatime
                    lastTime = curTime;

                    //Dont simulate whe not focused
                    if (game.Focused)
                    {
                        //Display fps in the title
                        game.Title = "Tank Game";

                        // demo controls
                        if (game.Keyboard[Key.Escape])
                        {
                            game.Exit();
                        }

                    }
                };

                // Create test tank
                TestTank tank = new TestTank();
                List<Mesh> tankMeshes = tank.CreateMeshes();

                game.RenderFrame += (sender, e) =>
                {
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    #region PROJECTION MATRIX
                    var projMat = Matrix4.CreateOrthographic(25 * aspectRatio, 25, 0.1f, 500);
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadMatrix(ref projMat);
                    #endregion

                    #region CAMERA MATRIX
                    GL.MatrixMode(MatrixMode.Modelview);
                    var lookMat = Matrix4.LookAt(new Vector3(0, 0, -10), new Vector3(0, 0, 1), Vector3.UnitY);
                    var modelMat = Matrix4.CreateRotationY(0);
                    lookMat = modelMat * lookMat;
                    GL.LoadMatrix(ref lookMat);
                    #endregion

                    GL.Begin(PrimitiveType.Lines);

                    //DrawTank();
                    
                    // Move and draw test tank
                    foreach (Mesh mesh in tankMeshes)
                    {
                        mesh.Position = new Vector2(mesh.Position[0] + 0.01f, 0);
                    }
                    DrawMeshes(tankMeshes, Color.Red);

                    GL.End();

                    game.SwapBuffers();
                };

                game.Run(60.0);
            }
        }

        public static void DrawTank()
        {
            GL.Color4(Color.Cyan);

            //Hull
            GL.Vertex3(new Vector3(-2, 1, 0));
            GL.Vertex3(new Vector3(2,  1, 0));

            GL.Vertex3(new Vector3(2,  1, 0));
            GL.Vertex3(new Vector3(2, -1, 0));

            GL.Vertex3(new Vector3(2,  -1, 0));
            GL.Vertex3(new Vector3(-2, -1, 0));

            GL.Vertex3(new Vector3(-2,  1, 0));
            GL.Vertex3(new Vector3(-2, -1, 0));

            //Tower
            GL.Vertex3(new Vector3(-.75f, .75f, 0));
            GL.Vertex3(new Vector3(.75f, .75f, 0));

            GL.Vertex3(new Vector3(.75f, .75f, 0));
            GL.Vertex3(new Vector3(.75f, -.75f, 0));

            GL.Vertex3(new Vector3(.75f, -.75f, 0));
            GL.Vertex3(new Vector3(-.75f, -.75f, 0));

            GL.Vertex3(new Vector3(-.75f, .75f, 0));
            GL.Vertex3(new Vector3(-.75f, -.75f, 0));

            //Cannon
            GL.Vertex3(new Vector3(.75f, .15f, 0));
            GL.Vertex3(new Vector3(3, .15f, 0));

            GL.Vertex3(new Vector3(3, .15f, 0));
            GL.Vertex3(new Vector3(3, -.15f, 0));

            GL.Vertex3(new Vector3(3, -.15f, 0));
            GL.Vertex3(new Vector3(.75f, -.15f, 0));

        }

        public static void DrawMeshes(List<Mesh> meshes, Color color)
        {
            GL.Color4(color);
            foreach (Mesh mesh in meshes)
            {
                for (int i = 0; i < mesh.Vertices.Length; i++)
                {
                    // Vertex positions are offsets from mesh.Position
                    // Draw lines between 2 vertices
                    GL.Vertex2(mesh.Position + mesh.Vertices[i]);
                    if(i + 1 < mesh.Vertices.Length)
                        GL.Vertex2(mesh.Position + mesh.Vertices[i + 1]);
                    else
                        GL.Vertex2(mesh.Position + mesh.Vertices[0]);
                }
            }            
        }
    }
}
