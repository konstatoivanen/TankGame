using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Utils;
using Utils.Physics;
using System.Linq;

namespace TankGame
{
    //Simple Static class for counting time
    public static class Time
    {
        public  static float startTime;
        public  static float time
        {
            get { return Environment.TickCount / 1000.0f; }
        }
        private static float lastTime;
        public  static float deltatime;

        public static void Init()
        {
            startTime = Environment.TickCount;
            lastTime  = time;
        }
        public static void UpdateTime()
        {
            deltatime = time - lastTime;
            deltatime = Math.Max(0.01f, deltatime); //Min Frame deltatime
            lastTime  = time;
        }
    }

    public class TankGame
    {
        public static GameWindow game = new GameWindow();

        public static Random random = new Random();

        public  static float    aspectRatio
        {
            get { return (float)game.Width / game.Height; }
        }
        public  static Vector2  battlefieldSize
        {
            get { return new Vector2(25 * aspectRatio, 25); }
        }

        public delegate void UpdateEvent();
        public static UpdateEvent OnUpdate;

        public delegate void RestartEvent();
        public static RestartEvent OnRestart;

        public delegate void ResizeEvent();
        public static ResizeEvent OnResize;

        public static Tank player1;
        public static Tank player2;

        #region renderStack Variables
        private static List<Mesh> m_meshList = new List<Mesh>();      
        public  static void AddMeshToRenderStack(Mesh m)
        {
            if (m_meshList.Contains(m))
                return;

            m_meshList.Add(m);
        }
        public  static void AddMeshesToRenderStack(List<Mesh> m)
        {
            for (int i = 0; i < m.Count; ++i)
                if (m_meshList.Contains(m[i]))
                    return;


            m_meshList.AddRange(m);
        }     
        public  static void RemoveMeshFromRenderStack(Mesh m)
        {
            if (!m_meshList.Contains(m))
                return;

            m_meshList.Remove(m);
        }
        public  static void RemoveMeshesFromRenderStack(List<Mesh> m)
        {
            for (int i = 0; i < m.Count; ++i)
            {
                if (m_meshList.Contains(m[i]))
                    m_meshList.Remove(m[i]);
            }             
        }

        private static List<DebugMesh> m_debugMeshList = new List<DebugMesh>();
        public  static void DrawDebugMesh(DebugMesh m)
        {
            if (m_debugMeshList.Contains(m))
                return;

            m_debugMeshList.Add(m);
        }
        #endregion

        [STAThread]
        public static void Main()
        {
            using (game)
            {       
                game.Load        += (sender, e) => Start();
                game.Resize      += (sender, e) => Resize();
                game.UpdateFrame += (sender, e) => Update();
                game.RenderFrame += (sender, e) => RenderFrame();
                game.Run(60.0);
            }
        }

        #region UpdateMethods
        private static void Restart()
        {
            if (OnRestart != null)
                OnRestart();

            m_meshList.Clear();
            Physics.ClearColliders();

            GC.Collect();

            Start();
        }
        private static void Start()
        {
            game.VSync = VSyncMode.On;
            game.Title = "Tank Game";

            Time.Init();

            player1 = new Tank(1, 1, 1, -new Vector2(battlefieldSize.X * 0.5f - 3, battlefieldSize.Y * 0.5f - 2), new Vector2(1, 0), Color.Red, new InputScheme(InputScheme.Preset.Player1), PhysicsLayer.Player1);
            player2 = new Tank(1, 1, 1, new Vector2(battlefieldSize.X * 0.5f - 3, battlefieldSize.Y * 0.5f - 2), new Vector2(-1, 0), Color.Blue, new InputScheme(InputScheme.Preset.Player2), PhysicsLayer.Player2);

            GenerateObstacles();
        }
        private static void Resize()
        {
            GL.Viewport(0, 0, game.Width, game.Height);
            //Should Probably reinitialize the game since the battlefield size has changed

            if(OnResize != null)
                OnResize();
        }
        private static void Update()
        {
            Time.UpdateTime();

            if (!game.Focused)
                return;

            if (OnUpdate != null)
                OnUpdate();

            //game.Title = "Tank Game : " + (1 / Time.deltatime).ToString();

            if (game.Keyboard[Key.Escape])
                game.Exit();

            if (game.Keyboard[Key.Enter])
                Restart();

            Debug.UpdateLog();        
        }
        private static void RenderFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            #region PROJECTION MATRIX
            var projMat = Matrix4.CreateOrthographic(battlefieldSize.X, battlefieldSize.Y, 0.1f, 500);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projMat);
            #endregion

            #region CAMERA MATRIX
            GL.MatrixMode(MatrixMode.Modelview);
            var lookMat     = Matrix4.LookAt(new Vector3(0, 0, -10), new Vector3(0, 0, 1), Vector3.UnitY);
            var modelMat    = Matrix4.CreateRotationY(0);
            lookMat         = modelMat * lookMat;
            GL.LoadMatrix(ref lookMat);
            #endregion

            for (int i = 0; i < m_meshList.Count; ++i) m_meshList[i].Draw();

            for (int i = 0; i < m_debugMeshList.Count; ++i) m_debugMeshList[i].Draw();
            m_debugMeshList.Clear();

            game.SwapBuffers();
        }
        private static void GenerateObstacles()
        {
            List<Obstacle> obsList = LevelGeneration.GenerateGrid(battlefieldSize * 0.5f - new Vector2(2,2), -battlefieldSize * 0.5f + new Vector2(2,2), (int)Math.Floor(battlefieldSize.X) / 6, 4, 0.75f);

            Debug.Log((obsList[obsList.Count - 1].position - player1.position).ToString());
            Debug.Log((obsList[obsList.Count - 1].position - player2.position).ToString());

            obsList[obsList.Count - 1].DestroyImmediate();
            obsList.RemoveAt(obsList.Count - 1);

            obsList[0].DestroyImmediate();
            obsList.RemoveAt(0);

            obsList.CullRandom(0.35f);
        }
        #endregion
    }
}
