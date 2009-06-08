#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

using System;
using System.Drawing;
using CSat;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{
    class Game0 : GameWindow
    {
        private int _oldMouseX, _oldMouseY;
        Camera cam = new Camera();
        Font font = new Font(FontFamily.GenericSansSerif, 24.0f);
        TextPrinter text = new TextPrinter();

        public Game0(int width, int height) : base(width, height, GraphicsMode.Default, "xxx") { }

        public override void OnLoad(EventArgs e)
        {
            GL.ClearColor(System.Drawing.Color.Blue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.CullFace);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            Util.Set3DMode();

            // lataa kamat


        }

        public override void OnUnload(EventArgs e)
        {
            font.Dispose();
            Util.ClearArrays();
        }

        protected override void OnResize(EventArgs e)
        {
            Util.Resize(Width, Height, 1.0, 1000);
        }

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
        
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            cam.UpdateXZ();
            //Light.UpdateLights();
            //Frustum.CalculateFrustum();

            // rendataas jotain


            Texture.ActiveUnit(0);
            text.Begin();
            text.Print("blaa bluu: " + Settings.NumOfObjects, font, Color.White);
            text.End();

            SwapBuffers();
        }

    }
}
