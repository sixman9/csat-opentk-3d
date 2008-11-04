#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

// GLSL test
// ladataan objekti jossa valmiiksi tieto mikä shaderi ladataan.
// sitten randomilla vaihdetaan joko toiseen shaderiin tai otetaan Shader pois.

using System;
using System.Drawing;
using CSat;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Math;
using OpenTK.Platform;


namespace CSatExamples
{
    class Game9 : GameWindow
    {
        Camera cam = new Camera();
        ITextPrinter printer = new TextPrinter();
        TextureFont font = new TextureFont(new Font(FontFamily.GenericSerif, 24.0f));

        const int OBJS = 20;

        Object3D toonBall = new Object3D(), toonStar = new Object3D();
        Object3D[] objs = new Object3D[OBJS];

        Group world = new Group("world");
        Light light = new Light();

        public Game9(int width, int height) : base(width, height, GraphicsMode.Default, "GLSL test") { }

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

            Light.Enable();
            light.Position = new Vector3(80, 20, 0);
            light.UpdateColor();
            light.SetLight(true);
            Light.Add(light, 0); // lisää valo (aurinko)
            light.Rotation = new Vector3(0, 0, -40);

            Mouse.ButtonDown += MouseButtonDown;
            Mouse.ButtonUp += MouseButtonUp;

            cam.Position.Y = 1;
            cam.Position.Z = 2;

            toonBall.Load("ball.obj");
            toonStar.Load("star.obj");
            Random random = new Random();
            for (int q = 0; q < OBJS; q++)
            {
                // nämä käyttää toon shaderia (kirjoitettu .mtl tiedostoon)
                if (q < OBJS / 2) objs[q] = toonBall.Clone();
                else objs[q] = toonStar.Clone();

                objs[q].Position.X = random.Next(20) - 10;
                objs[q].Position.Y = random.Next(20) - 10;
                objs[q].Position.Z = random.Next(20) - 10;

                // käytetäänkö lightingiä vai ei mitään..random päättäköön.
                if (random.Next(10) < 5) objs[q].LoadShader("lighting.vert", "lighting.frag");
                else objs[q].LoadShader("", ""); // ei shaderia
            }
            for (int q = 0; q < OBJS; q++) toonBall.Add(objs[q]); // lisäätään obut toonballiin joka on maailmankaikkeuden keskipiste

            world.Add(toonBall);

            Util.Set3DMode();
        }

        bool[] mouseButtons = new bool[5];
        void MouseButtonDown(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = true; }
        void MouseButtonUp(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = false; }

        #region OnUnload
        public override void OnUnload(EventArgs e)
        {
            font.Dispose();
            Util.ClearArrays(); // poistaa kaikki materiaalit ja texturet

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

            toonBall.Rotation += new Vector3(1.2f, 1.5f, 0.3f);

            // valolla on samat metodit liikuttamiseen kuin objekteilla joten 
            // pisteetään valo liikkumaan ympäyrää
            light.MoveXZ(4);
            light.Rotation.Y += 4f;
        }

        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            base.OnRenderFrame(e);
            Settings.NumOfObjects = 0;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            cam.UpdateXZ();
            Frustum.CalculateFrustum();
            Light.UpdateLights(); // päivitä valot kameran asettamisen jälkeen

            world.Render();

            // valon paikalle pallo:
            Vector3 tmp = objs[0].Position;
            objs[0].Position = light.Position;
            objs[0].Render();
            objs[0].Position = tmp;

            Texture.ActiveUnit(0);

            Light.Disable();
            printer.Begin();
            if (MainClass.UseFonts) printer.Draw("Objs: " + Settings.NumOfObjects, font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);
            Light.Enable();

            SwapBuffers();
        }

    }
}
