#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

/* 
 * 3D test
 * parent loading, skybox, multitexturing
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
        private int _oldMouseX, _oldMouseY;
        Camera cam = new Camera();
        Font font = new Font(FontFamily.GenericSansSerif, 24.0f);
        TextPrinter text = new TextPrinter();

        Sky skybox = new Sky("sky");
        Mesh obj;
        Texture tex = new Texture();

        public Game2(int width, int height) : base(width, height, GraphicsMode.Default, "3D test: loads obj file") { }

        /// <summary>Load resources here.</summary>
        public override void OnLoad(EventArgs e)
        {
            GL.ClearColor(System.Drawing.Color.Blue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.CullFace);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.Normalize);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            obj = new ObjModel("scene", "skene.obj", 20, 20, 20);
            skybox.LoadSkybox("sky/sky2_", "jpg", 100);
            tex = Texture.Load("1.png");

            cam.Position.Y = 10;

            Util.Set3DMode();
        }

        #region OnUnload
        public override void OnUnload(EventArgs e)
        {
            font.Dispose();
            tex.Dispose();

            skybox.Dispose();
            obj.Dispose();

            Util.ClearArrays(); // tuhoa kaikki loput jos jotain objekteja jäi tuhoamatta
        }
        #endregion

        /// <summary>
        /// Called when your window is resized. Set your Frontport here. It is also
        /// a good place to set Up your projection Matrix (which probably changes
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
        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            cam.UpdateXZ();
            skybox.Render();
            Frustum.CalculateFrustum();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusDstAlpha);

            tex.Bind(1); // otetaan texture toiseen textureunittiin
            VBO.UseTexUnits(true, true); // multitexture
            obj.Render();
            VBO.UseTexUnits(true, false); // toinen unitti pois käytöstä
            tex.UnBind(1); // texture pois

            GL.Disable(EnableCap.Blend);

            Texture.ActiveUnit(0);
            text.Begin();
            text.Print("3D test -- objs: " + Settings.NumOfObjects, font, Color.White);
            text.End();

            SwapBuffers();
        }

    }
}
