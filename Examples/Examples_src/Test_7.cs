#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
// BitmapFont luokan testaus.
//
// Fonttikuvat on luotu IrrFontToolilla  
// (tulee <a href="http://irrlicht.sourceforge.net/">Irrlicht</a> 3d-enginen mukana)
// jolloin kirjainten ei tarvitse olla saman levyisiä.
// Löytyy myös (atm): https://archon2160.pbwiki.com/f/IrrFontTool.exe

using System;
using CSat;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Math;
using OpenTK.Platform;

namespace CSatExamples
{

    class Game7 : GameWindow
    {
        Camera cam = new Camera();
        const int PART = 5;

        static Random random = new Random();

        BitmapFont bfont = new BitmapFont();

        Object2D[] partObj = new Object2D[3];

        Particles explosion = new Particles();
        ParticleEngine particles = new ParticleEngine(); // engine huolehtii että partikkelit renderoidaan oikeassa järjestyksessä

        public Game7(int width, int height) : base(width, height, GraphicsMode.Default, "Bitmap Font") { }

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

            partObj[1] = new Object2D("fire.png");

            explosion.SetObject(partObj[1], true); // läpinäkyvä

            cam.Position.Z = 100;
            cam.Position.Y = 2;

            Mouse.ButtonDown += MouseButtonDown;
            Mouse.ButtonUp += MouseButtonUp;

            bfont.Load("fonts/times14.png");

            SetupParticles(true, true, true);

            particles.Add(explosion, null);

            Util.Set3DMode();
        }

        bool[] mouseButtons = new bool[5];
        void MouseButtonDown(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = true; }
        void MouseButtonUp(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = false; }

        #region OnUnload
        public override void OnUnload(EventArgs e)
        {
            partObj[1].Dispose();
            bfont.Dispose();
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
            UpdateParticles((float)e.Time);

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
        }

        float textPos = 0;
        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            cam.UpdateXZ();
            GL.Disable(EnableCap.Lighting);
            Util.RenderGrid();

            // tapa1:
            // particles -partikkeliengine hoitaa sinne lisättyjen partikkelien
            // renderoinnista. se sorttaa ne, hoitaa takaisinkutsut ym.
            particles.Render();

            // tapa2:
            // renderoidaan 1 partikkelirykelmä, ei sortata, ei takaisinkutsua.
            GL.Translate(-50, -5, 0); // siirretään savun alle tämä. voi vertailla toiseen.
            explosion.Render();
            GL.Translate(50, 5, 0);

            Texture.ActiveUnit(0);

            bfont.Write3D(1, 0, "\nNew line..\n   stupid test.");
            bfont.Write3D(10, 10, "Using coordinates,\nblaa bluu böä.");
            bfont.Write3D(5, -10, "The end.");

            // 2d jutut kantsii pistää aina viimeisenä

            Util.Set2DMode();
            bfont.Write(21, textPos += (float)e.Time * 100, "First BitmapFont test!");
            Util.Set3DMode();

            SwapBuffers();
        }

        void SetupParticles(bool test, bool explosion, bool smoke)
        {
            if (explosion)
            {
                for (int q = 0; q < PART; q++)
                {
                    Vector3 pos = new Vector3(50 + (float)(random.NextDouble() * 1), 5 + (float)(random.NextDouble() * 1), 0);
                    Vector3 dir = new Vector3(0.5f * (0.5f - (float)(random.NextDouble())), 0.5f * (0.5f - (float)(random.NextDouble())), 0.5f * (0.5f - (float)(random.NextDouble())));
                    Vector3 grav = new Vector3(0, 0, 0);
                    float life = 2;
                    float size = (float)(random.NextDouble() * 16 + 15);

                    this.explosion.AddParticle(ref pos, ref dir, ref grav, life, size, new Vector4(0.8f, 0, 0, 0.5f));
                }
            }
        }

        void UpdateParticles(float time)
        {
            if (explosion.NumOfParticles == 0) SetupParticles(false, true, false);
            explosion.Update(time);

        }


    }
}
