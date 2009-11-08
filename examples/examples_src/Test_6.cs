#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
// billboardit

using System;
using System.Drawing;
using CSat;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{
    class Game6 : GameWindow
    {
        private int _oldMouseX, _oldMouseY;
        Camera cam = new Camera();
        Billboard obj = new Billboard("billboard");

        Font font = new Font(FontFamily.GenericSansSerif, 24.0f);
        OpenTK.Graphics.TextPrinter text = new OpenTK.Graphics.TextPrinter();

        public Game6(int width, int height) : base(width, height, OpenTK.Graphics.GraphicsMode.Default, "Billboard test") { }

        /// <summary>Load resources here.</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(System.Drawing.Color.Blue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.CullFace);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.Normalize);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            obj.Load("1.png");

            cam.Position.Z = 100;
            cam.Position.Y = 2;

            Util.Set3DMode();
        }

        #region OnUnload
        protected override void OnUnload(EventArgs e)
        {
            font.Dispose();
            obj.Dispose();
            Util.ClearArrays();
        }
        #endregion

        /// <summary>
        /// Called when your window is resized. Set your Frontport here. It is also
        /// a good place to set Up your projection Matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Contains information on the new Width and Size of the GameWindow.</param>
        protected override void OnResize(EventArgs e)
        {
            Util.Resize(Width, Height, 1, 1000);
        }

        /// <summary>
        /// Called when it is time to setup the next frame.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
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
            if (Keyboard[Key.R]) cam.Position.Y++;
            if (Keyboard[Key.F]) cam.Position.Y--;

            if (Mouse[MouseButton.Left])
            {
                cam.TurnXZ(Mouse.X - _oldMouseX);
                cam.LookUpXZ(Mouse.Y - _oldMouseY);
            }
            _oldMouseX = Mouse.X; _oldMouseY = Mouse.Y;
        }

        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            cam.UpdateXZ();
            GL.Disable(EnableCap.Lighting);

            obj.RenderBillboard(0, 0, 0, 4);

            float rot = cam.Position.X + cam.Position.Y + cam.Position.Z;
            GL.Rotate(rot, 0, 0, 1);
            for (int q = 0; q < 10; q++)
            {
                float x = (float)Math.Sin(q * 0.7);
                float y = (float)Math.Cos(q * 0.7);
                float s = 2f;
                obj.RenderBillboard(10 * x, 10 * y, 0, s);
            }
            GL.Rotate(-rot, 0, 0, 1);

            Util.RenderGrid();

            Texture.ActiveUnit(0);
            text.Begin();
            text.Print("camerapos: " + cam.Position.ToString(), font, Color.White);
            text.End();

            SwapBuffers();
        }

    }
}
