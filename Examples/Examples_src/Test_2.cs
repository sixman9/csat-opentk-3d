#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

/* 
 * 3D test
 * obj loading, skybox, multitexturing
 * 
 */

using System;
using System.Drawing;
using CSat;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{

    class Game2 : GameWindow
    {
        Camera cam = new Camera();

        ITextPrinter printer = new TextPrinter();
        TextureFont font = new TextureFont(new Font(FontFamily.GenericSerif, 24.0f));

        Skybox skybox = new Skybox();
        Object3D obj = new Object3D();
        Texture tex = new Texture();

        public Game2(int width, int height) : base(width, height, GraphicsMode.Default, "3D test: loads obj file") { }

        /// <summary>Load resources here.</summary>
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(System.Drawing.Color.Blue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.CullFace);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            obj.Load("skene.obj", 20, 20, 20, null);
            skybox.Load("sky2_", "jpg", 100);
            tex = Texture.Load("1.png");

            cam.position.Y = 10;

            Mouse.ButtonDown += MouseButtonDown;
            Mouse.ButtonUp += MouseButtonUp;

            Util.Set3DMode();
        }

        bool[] mouseButtons = new bool[5];
        void MouseButtonDown(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = true; }
        void MouseButtonUp(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = false; }

        #region OnUnload
        public override void OnUnload(EventArgs e)
        {
            font.Dispose();
            tex.Dispose();

            skybox.Dispose();
            obj.Dispose();

        }
        #endregion

        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Contains information on the new Width and Size of the GameWindow.</param>
        protected override void OnResize(ResizeEventArgs e)
        {
            Util.Resize(e.Width, e.Height, 1.0, 1000);
        }

        /// <summary>
        /// Called when it is time to setup the next frame.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        public override void OnUpdateFrame(UpdateFrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Exit();

            // ohjaus
            float spd = 1;
            if (Keyboard[Key.ShiftLeft]) spd = 2;
            if (Keyboard[Key.W]) cam.MoveXZ(spd, 0);
            if (Keyboard[Key.S]) cam.MoveXZ(-spd, 0);
            if (Keyboard[Key.A]) cam.MoveXZ(0, -spd);
            if (Keyboard[Key.D]) cam.MoveXZ(0, spd);
            if (Keyboard[Key.R]) cam.position.Y++;
            if (Keyboard[Key.F]) cam.position.Y--;
            if (mouseButtons[(int)MouseButton.Left])
            {
                cam.TurnXZ(Mouse.XDelta);
                cam.LookUpXZ(Mouse.YDelta);
            }
            int tmp = Mouse.XDelta; tmp = Mouse.YDelta;
        }

        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            skybox.Render(cam);
            cam.UpdateXZ();
            Frustum.CalculateFrustum();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusDstAlpha);
            tex.Bind(1);
            obj.UseTextureUnits(true, true, false);
            obj.Render();
            obj.UseTextureUnits(true, false, false);
            Texture.UnBind(1);
            GL.Disable(EnableCap.Blend);

            Texture.ActiveUnit(0);
            printer.Begin();
            if (MainClass.UseFonts) printer.Draw("3D test -- objs: " + Settings.NumOfObjects, font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);

            SwapBuffers();
        }

    }
}
