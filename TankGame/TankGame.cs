using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;
using Utils;

//Konsta
namespace TankGame
{
    //Simple Static class for counting time
    public static class Time
    {
        public  static float startTime;
        public  static float timeSinceStartUp
        {
            get { return Environment.TickCount / 1000.0f; }
        }
        private static float lastTime;
        public  static float deltatime;

        public static void Init()
        {
            lastTime = timeSinceStartUp;
        }
        public static void UpdateTime()
        {
            deltatime = timeSinceStartUp - lastTime;
            deltatime = Math.Max(0.01f, deltatime); //Min Frame deltatime
            lastTime  = timeSinceStartUp;
        }
    }

    public class TankGame
    {
        private static int startTime = 0;

        public static GameWindow game = new GameWindow();

        private static float aspectRatio
        {
            get { return (float)game.Width / game.Height; }
        }

        public delegate void UpdateEvent();
        public static UpdateEvent OnUpdate; 


        private static List<Mesh> m_meshList = new List<Mesh>();      
        public static void AddMeshToRenderStack(Mesh m)
        {
            if (m_meshList.Contains(m))
                return;

            m_meshList.Add(m);
        }
        public static void AddMeshesToRenderStack(List<Mesh> m)
        {
            for (int i = 0; i < m.Count; ++i)
                if (m_meshList.Contains(m[i]))
                    return;


            m_meshList.AddRange(m);
        }     
        public static void RemoveMeshFromRenderStack(Mesh m)
        {
            if (!m_meshList.Contains(m))
                return;

            m_meshList.Remove(m);
        }


        [STAThread]
        public static void Main()
        {
            Time.Init();

            Tank tank       = new Tank(1, 1, 1);

            using (game)
            {       
                game.Load += (sender, e) =>
                {
                    game.VSync = VSyncMode.On;
                    startTime = Environment.TickCount;

                    // Create test tank
                    
                    AddMeshesToRenderStack(tank.mesh);
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {
                    Time.UpdateTime();

                    //Dont simulate whe not focused
                    if (game.Focused)
                    {
                        //Display fps in the title
                        game.Title = "Tank Game";

                        if(OnUpdate != null) OnUpdate();

                        // demo controls
                        if (game.Keyboard[Key.Escape])
                        {
                            game.Exit();
                        }

                    }
                };

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

                    for (int i = 0; i < m_meshList.Count; ++i) m_meshList[i].Draw();

                    GL.End();

                    game.SwapBuffers();
                };

                game.Run(60.0);
            }
        }

    }
}
