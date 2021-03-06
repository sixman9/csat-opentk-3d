﻿#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
// particles

using System;
using System.Drawing;
using CSat;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{
    class Game3 : GameWindow
    {
        private int _oldMouseX, _oldMouseY;
        Camera cam = new Camera();
        const int PART = 100; // montako partikkelia laitetaan savuun, räjähdykseen ja testiin

        static Random random = new Random();

        Font font = new Font(FontFamily.GenericSansSerif, 24.0f);
        OpenTK.Graphics.TextPrinter text = new OpenTK.Graphics.TextPrinter();

        Object2D[] partObj = new Object2D[3];

        Particles test = new Particles("planets");
        Particles explosion = new Particles("explosion");
        Particles smoke = new Particles("smoke");

        ParticleEngine particles = new ParticleEngine(); // engine huolehtii että partikkelit renderoidaan oikeassa järjestyksessä

        public Game3(int width, int height) : base(width, height, OpenTK.Graphics.GraphicsMode.Default, "Particles test") { }

        /// <summary>Load resources here.</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(0.2f, 0.2f, 0.2f, 1);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.CullFace);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Disable(EnableCap.Lighting);

            partObj[0] = new Object2D("planetImg", "1.png");
            partObj[1] = new Object2D("fireImg", "fire.png");
            partObj[2] = new Object2D("smokeImg", "smoke.png");

            test.SetObject(partObj[0], false); // ei läpinäkyvä
            explosion.SetObject(partObj[1], true); // läpinäkyvä
            smoke.SetObject(partObj[2], true); // kuten tämäkin

            cam.Position.Z = 100;
            cam.Position.Y = 2;

            particles.Add(test, null);
            particles.Add(explosion, new ParticleCallback(RenderParticleCallback));
            particles.Add(smoke, null);
            SetupParticles(true, true, true);

            Util.Set3DMode();

        }

        #region OnUnload
        protected override void OnUnload(EventArgs e)
        {
            font.Dispose();
            for (int q = 0; q < 3; q++) partObj[q].Dispose();
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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            cam.UpdateXZ();

            GL.Disable(EnableCap.Lighting);

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
            GL.Color4(1f, 1, 1, 1);
            text.Begin();
            text.Print("Particles demo", font, Color.White);
            text.End();

            Util.RenderGrid();

            SwapBuffers();
        }

        void SetupParticles(bool test, bool explosion, bool smoke)
        {
            if (test)
            {
                for (int q = 0; q < PART; q++)
                {
                    Vector3 pos = new Vector3(-50 + (float)(random.NextDouble() * 3), 5 + (float)(random.NextDouble() * 3), 0);
                    Vector3 dir = new Vector3(0.4f + (float)(random.NextDouble() * 0.1f), 0.4f + (float)(random.NextDouble() * 0.1f), 0.4f + (float)(random.NextDouble() * 0.1f));
                    Vector3 grav = new Vector3(0, -0.01f, 0);
                    float life = (float)(random.NextDouble() * 1000 + 5000);
                    float size = 2;

                    this.test.AddParticle(ref pos, ref dir, ref grav, life, size, new Vector4(1, 1, 1, 1));
                }
            }
            if (explosion)
            {
                for (int q = 0; q < PART; q++)
                {
                    Vector3 pos = new Vector3(50 + (float)(random.NextDouble() * 1), 5 + (float)(random.NextDouble() * 1), 0);
                    Vector3 dir = new Vector3(0.5f * (0.5f - (float)(random.NextDouble())), 0.5f * (0.5f - (float)(random.NextDouble())), 0.5f * (0.5f - (float)(random.NextDouble())));
                    Vector3 grav = new Vector3(0, 0, 0);
                    float life = 2;
                    float size = (float)(random.NextDouble() * 10 + 6);

                    this.explosion.AddParticle(ref pos, ref dir, ref grav, life, size, new Vector4(0.3f, 0, 0, 0.5f));
                }
            }
            if (smoke)
            {
                for (int q = 0; q < PART; q++)
                {
                    Vector3 pos = new Vector3(0, 5 + (float)(random.NextDouble() * 10), 0);
                    Vector3 dir = new Vector3(-0.05f + (float)(random.NextDouble() * 0.1f), 0.1f, -0.05f + (float)(random.NextDouble() * 0.1f));
                    Vector3 grav = new Vector3(0, 0, 0);
                    float life = 1 + (float)(random.NextDouble() * 4);
                    float size = 10;
                    this.smoke.AddParticle(ref pos, ref dir, ref grav, life, size, new Vector4(0.4f, 0.4f, 0.4f, 0.2f));
                }
            }
        }

        // partikkeliengine kutsuu tätä, asetettu räjähdykseen (halutaan muuttaa sen väriä)       
        void RenderParticleCallback(Particle p)
        {
            // nyt voi tehdä joka partikkelille mitä haluaa, esim asettaa alphan lifeksi.
            float tc = p.life / 2;
            GL.Color4(1f, tc, tc, tc);
        }

        void UpdateParticles(float time)
        {
            if (test.NumOfParticles == 0) SetupParticles(true, false, false);
            if (explosion.NumOfParticles == 0) SetupParticles(false, true, false);

            test.Update(time * 1000);
            explosion.Update(time);

            if (smoke.NumOfParticles < PART)
            {
                Vector3 pos = new Vector3(0, 5 + (float)(random.NextDouble() * 5), 0);
                Vector3 dir = new Vector3(-0.05f + (float)(random.NextDouble() * 0.1f), 0.2f, -0.05f + (float)(random.NextDouble() * 0.1f));
                Vector3 grav = new Vector3(0, 0, 0);
                float life = 3;
                float size = 15;
                smoke.AddParticle(ref pos, ref dir, ref grav, life, size, new Vector4(0.5f, 0.5f, 0.5f, 0.1f));
            }
            smoke.Update(time);
        }
    }
}
