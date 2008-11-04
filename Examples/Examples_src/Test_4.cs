#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
// md5 lataus ja animointi

using System;
using System.Drawing;
using CSat;
using CSLoader; // md5 lataus
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{
    class Game4 : GameWindow
    {
        Camera cam = new Camera();
        ITextPrinter printer = new TextPrinter();
        TextureFont font = new TextureFont(new Font(FontFamily.GenericSerif, 24.0f));

        MD5Model model = new MD5Model();
        MD5Model.MD5Animation[] anim = new MD5Model.MD5Animation[5];
        AnimatedModel mod = new AnimatedModel(); // tähän pitää liittää MD5Model, tää hoitaa rendaukset ym
        int curAnim = 0;

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
            GL.Disable(EnableCap.Lighting);

            // model.UseExt(".jpg"); // jos .obj tiedostossa textureilla ei ole päätettä
            model.Load("Ugly/Ukko.mesh");
            model.LoadAnim("Ugly/Ukko_walk.anim", ref anim[0]);
            model.LoadAnim("Ugly/Ukko_action1.anim", ref anim[1]);
            model.LoadAnim("Ugly/Ukko_action2.anim", ref anim[2]);
            model.LoadAnim("Ugly/Ukko_action3.anim", ref anim[3]);
            mod.SetAnimatedModel(model);
            mod.FixRotation.X = -90; // ukko on "makaavassa" asennossa joten nostetaan se fixRotationilla pystyyn.
            mod.FixRotation.Z = 180; // säätöä.. katse eteen päin!

            cam.Position.Y = 3;
            cam.Position.Z = 10;
            Mouse.ButtonDown += MouseButtonDown;
            Mouse.ButtonUp += MouseButtonUp;

            GL.Enable(EnableCap.Texture2D);

            Util.Set3DMode();

            Log.WriteDebugLine("Animation test\nkeys:\n arrows, 1,2 : actions");
        }

        bool[] mouseButtons = new bool[5];
        void MouseButtonDown(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = true; }
        void MouseButtonUp(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = false; }

        #region OnUnload
        public override void OnUnload(EventArgs e)
        {
            font.Dispose();
            model.Dispose();

        }
        #endregion

        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
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
            if (Keyboard[Key.Left]) mod.Rotation.Y += speed * 20;
            if (Keyboard[Key.Right]) mod.Rotation.Y -= speed * 20;

            if (Keyboard[Key.Number1] == false && Keyboard[Key.Number2] == false)
            {
                if (Keyboard[Key.Up])
                {
                    mod.MoveXZ(speed);
                    curAnim = 0;
                }
                else
                    if (Keyboard[Key.Down])
                    {
                        mod.MoveXZ(-speed);
                        curAnim = 0;
                    }
                    else curAnim = 2;
            }
            if (Keyboard[Key.Number1]) curAnim = 1;
            if (Keyboard[Key.Number2]) curAnim = 3;

            // ohjaus
            float spd = 1;
            if (Keyboard[Key.ShiftLeft]) spd = 2;
            if (Keyboard[Key.W]) cam.MoveXZ(spd, 0);
            if (Keyboard[Key.S]) cam.MoveXZ(-spd, 0);
            if (Keyboard[Key.A]) cam.MoveXZ(0, -spd);
            if (Keyboard[Key.D]) cam.MoveXZ(0, spd);
            if (Keyboard[Key.R]) cam.Position.Y++;
            if (Keyboard[Key.F]) cam.Position.Y--;
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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            cam.UpdateXZ();
            Frustum.CalculateFrustum();

            model.Update((float)e.Time, ref anim[curAnim]);
            mod.Render(); // huom! AnimationModelin render (se asettaa ja cullaa)

            Texture.ActiveUnit(0);
            printer.Begin();
            if (MainClass.UseFonts) printer.Draw("Animation test\nkeys:\n arrows, 1,2 : actions", font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);

            SwapBuffers();
        }

    }
}
