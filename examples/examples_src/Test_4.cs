#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
// md5 lataus ja animointi

using System;
using System.Drawing;
using CSat;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{
    class Game4 : GameWindow
    {
        private int _oldMouseX, _oldMouseY;
        Camera cam = new Camera();
        Font font = new Font(FontFamily.GenericSansSerif, 24.0f);
        TextPrinter text = new TextPrinter();

        Mesh model;

        public Game4(int width, int height) : base(width, height, GraphicsMode.Default, "Animation test") { }

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
            GL.Enable(EnableCap.Normalize);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Texture2D);

            // model.UseExt(".jpg"); // jos .mesh tiedostossa textureilla ei ole päätettä
            model = new AnimatedModel("ugly", "Ugly/Ukko.mesh");
            model.LoadAnim("walk", "Ugly/Ukko_walk.anim");
            model.LoadAnim("act1", "Ugly/Ukko_action1.anim");
            model.LoadAnim("act2", "Ugly/Ukko_action2.anim");
            model.LoadAnim("act3", "Ugly/Ukko_action3.anim");
            model.FixRotation.X = -90; // ukko on "makaavassa" asennossa joten nostetaan se fixRotationilla pystyyn.
            model.FixRotation.Z = 180; // säätöä.. katse eteen päin!

            cam.Position.Y = 3;
            cam.Position.Z = 10;

            Util.Set3DMode();

            Log.WriteDebugLine("Animation test\nkeys:\n arrows, 1,2 : actions");
        }

        #region OnUnload
        public override void OnUnload(EventArgs e)
        {
            font.Dispose();
            Util.ClearArrays();
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

            // örvelön liikuttaminen
            float speed = (float)(3 * e.Time);
            if (Keyboard[Key.Left]) model.Rotation.Y += speed * 20;
            if (Keyboard[Key.Right]) model.Rotation.Y -= speed * 20;

            if (Keyboard[Key.Number1] == false && Keyboard[Key.Number2] == false)
            {
                if (Keyboard[Key.Up])
                {
                    model.MoveXZ(speed);
                    model.UseAnimation("walk");
                }
                else
                    if (Keyboard[Key.Down])
                    {
                        model.MoveXZ(-speed);
                        model.UseAnimation("walk");
                    }
                    else model.UseAnimation("act2"); // äijä vapaalla
            }
            if (Keyboard[Key.Number1]) model.UseAnimation("act1"); ;
            if (Keyboard[Key.Number2]) model.UseAnimation("act3");

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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            cam.UpdateXZ();
            Frustum.CalculateFrustum();

            model.Update((float)e.Time);
            model.Render();

            Texture.ActiveUnit(0);
            text.Begin();
            text.Print("Animation test\nkeys:\n arrows, 1,2 : actions", font, Color.White);
            text.End();

            SwapBuffers();
        }

    }
}
