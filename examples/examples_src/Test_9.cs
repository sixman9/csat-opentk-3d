#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
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
        private int _oldMouseX, _oldMouseY;
        Camera cam = new Camera();
        Font font = new Font(FontFamily.GenericSansSerif, 24.0f);
        TextPrinter text = new TextPrinter();

        const int OBJS = 20;

        Mesh toonBall, toonStar;
        Mesh[] objs = new Mesh[OBJS];

        Node world = new Node();
        Light light = new Light("light");

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
            light.Rotation = new Vector3(0, 0, -40);

            toonBall = new ObjModel("ball", "ball.obj");
            toonStar = new ObjModel("star", "star.obj");

            world.Add(light); // lisää valo (aurinko)
            light.Add(toonBall.Clone()); // lisää valoon pallo, liikkuu valon mukana

            Random random = new Random();
            for (int q = 0; q < OBJS; q++)
            {
                // nämä käyttää toon shaderia (kirjoitettu .mtl tiedostoon)
                if (q < OBJS / 2) objs[q] = toonBall.Clone();
                else objs[q] = toonStar.Clone();

                objs[q].Position.X = random.Next(20) - 10;
                objs[q].Position.Y = random.Next(20) - 10;
                objs[q].Position.Z = random.Next(20) - 10;

                // arvotaan, käytetäänkö lightingiä vai ei mitään
                if (random.Next(10) < 5) objs[q].LoadShader("lighting.vert", "lighting.frag");
                else objs[q].LoadShader("", ""); // ei shaderia
            }

            for (int q = 0; q < OBJS; q++) toonBall.Add(objs[q]);

            world.Add(toonBall);
            
            cam.Position.Y = 1;
            cam.Position.Z = 2;

            Util.Set3DMode();
        }

        #region OnUnload
        public override void OnUnload(EventArgs e)
        {
            font.Dispose();
            Util.ClearArrays(); // poistaa kaikki materiaalit ja texturet

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
            Util.Resize(Width, Height, 1.0, 1000);
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

            // toonballia pyörittämällä liikutetaan kaikkia objekteja koska ne on liitetty siihen.
            toonBall.Rotation += new Vector3(1.2f, 1.5f, 0.3f);

            // valolla on samat metodit liikuttamiseen kuin objekteilla joten 
            // pisteetään valo liikkumaan ympyrää
            light.MoveXZ(4);
            light.Rotation.Y += 4f;
        }

        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            Settings.NumOfObjects = 0;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            cam.UpdateXZ();
            Light.UpdateLights(); // päivitä valot kameran asettamisen jälkeen
            Frustum.CalculateFrustum();

            world.Render();

            Texture.ActiveUnit(0);

            Light.Disable();
            text.Begin();
            text.Print("Objs: " + Settings.NumOfObjects, font, Color.White);
            text.End();
            Light.Enable();

            SwapBuffers();
        }

    }
}
