﻿using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;
using Utils;

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
            startTime = Environment.TickCount;
            lastTime  = timeSinceStartUp;
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
        public static GameWindow game = new GameWindow();

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
        public static void RemoveMeshesFromRenderStack(List<Mesh> m)
        {
            for (int i = 0; i < m.Count; ++i)
            {
                if (m_meshList.Contains(m[i]))
                    m_meshList.Remove(m[i]);
            }             
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
        private static void Start()
        {
            game.VSync = VSyncMode.On;
            game.Title = "Tank Game";

            Time.Init();

            Tank tank = new Tank(1, 1, 1, -new Vector2(battlefieldSize.X * 0.4f, battlefieldSize.Y * 0.4f), new Vector2(1, 0), Color.Red, new InputScheme(InputScheme.Preset.Player1));
            Tank dank = new Tank(1, 1, 1, new Vector2(battlefieldSize.X * 0.4f, battlefieldSize.Y * 0.4f), new Vector2(-1, 0), Color.Blue, new InputScheme(InputScheme.Preset.Player2));
        }
        private static void Resize()
        {
            GL.Viewport(0, 0, game.Width, game.Height);
            //Should Probably reinitialize the game since the battlefield size has changed
        }
        private static void Update()
        {
            Time.UpdateTime();

            if (game.Focused)
            {
                if (OnUpdate != null) OnUpdate();

                if (game.Keyboard[Key.Escape])
                    game.Exit();
            }
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

            game.SwapBuffers();
        }
        #endregion
    }
}
