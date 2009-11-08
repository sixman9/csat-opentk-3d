#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
/*
 *  ladataan objekteja ja md5 malleja.
 *  liitetään objekteja toisiinsa ja pyöritellään.
 *
 *  objektien kloonaus Clone käskyllä. jakaa saman datan paitsi paikka/asentotiedot.
 *  asetetaan myös valo. se pitää päivittää kameran päivityksen jälkeen.
 *
 * 
 */
using System;
using System.Drawing;
using CSat;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{
    class Game8 : GameWindow
    {
        private int _oldMouseX, _oldMouseY;
        Camera cam = new Camera();

        Node world = new Node(); // tänne lisäillään kaikki kamat

        Sky skybox = new Sky("sky");
        Mesh[] obj = new Mesh[10];

        Mesh uglyModel;

        Font font = new Font(FontFamily.GenericSansSerif, 24.0f);
        OpenTK.Graphics.TextPrinter text = new OpenTK.Graphics.TextPrinter();

        Light light = new Light("light");

        public Game8(int width, int height) : base(width, height, OpenTK.Graphics.GraphicsMode.Default, "Scene") { }

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
            GL.Enable(EnableCap.ColorMaterial);

            Light.Enable();
            light.Position = new Vector3(50, 80, -100);
            light.SetLight(true);
            world.Add(light); // lisää valo (aurinko)

            // Lataukset
            skybox.LoadSkybox("sky/sky2_", "jpg", 100);
            world.Add(skybox); // skybox aina ekana koska se on kaikkien takana

            obj[0] = new ObjModel("head", "Head/head.obj"); // ladataan kerran
            obj[0].Position.Z = 0;

            // tehdään muutama kopio, siirretään vierekkäin
            obj[1] = obj[0].Clone();
            obj[1].Position.X = 5;

            obj[2] = obj[0].Clone();
            obj[2].Position.X = 10;

            obj[3] = obj[0].Clone();
            obj[3].Position.X = 15;

            obj[4] = obj[0].Clone();
            obj[4].Position.X = 20;

            obj[5] = new ObjModel("arm", "Head/arm.obj");
            obj[1].Add(obj[5]); // pää nro 2:seen lisätään käsi

            // lasketaan groupille bounding volume
            obj[1].Boundings.CreateBoundingVolume(obj[1]);

            // lisää kamat worldiin
            for (int q = 0; q < 4; q++) world.Add(obj[q]);

            // rumilus
            uglyModel = new AnimatedModel("ukko", "Ugly/Ukko.mesh");
            uglyModel.LoadAnim("act", "Ugly/Ukko_action2.anim");
            uglyModel.UseAnimation("act");
            uglyModel.Position.X = -4;
            uglyModel.FixRotation.X = -90; // ukko on "makaavassa" asennossa joten nostetaan se fixRotationilla pystyyn.
            world.Add(uglyModel);

            Mesh scene = new ObjModel("scene", "scene1.obj", 20, 20, 20);
            scene.Position.X = 10;
            // katos (sen nimi on toi None_Material__9) pitää pistää 2 puoliseks, muuten se ei näy alhaalta päin
            scene.SetDoubleSided("None_Material__9", true);
            // samoin seinät 2 puolisiks (blenderistä ja tutkimalla .obj tiedostoa selviää nämä nimet)
            scene.SetDoubleSided("None_Material__12", true);
            world.Add(scene); // lisää scene

            cam.Position.Y = 5;
            cam.Position.Z = 15;

            Util.Set3DMode();
        }

        #region OnUnload
        protected override void OnUnload(EventArgs e)
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

            // pöydät pyörimään
            obj[0].Rotation.Y += (float)e.Time * 100;
            obj[1].Rotation.Y -= (float)e.Time * 50;
            obj[2].Rotation.Y -= (float)e.Time * 10;
            obj[3].Rotation.Y += (float)Math.Sin((float)e.Time * 100) * 10;

            // ja hullu ukko huittisista pyörimään myös
            uglyModel.Rotation.Y -= (float)e.Time * 50;
            uglyModel.Update((float)e.Time);

            obj[5].Rotation.Y += (float)e.Time * 100;
        }

        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Settings.NumOfObjects = 0;
            base.OnRenderFrame(e);

            cam.UpdateXZ();
            Light.UpdateLights(); // päivitä valot kameran asettamisen jälkeen
            Frustum.CalculateFrustum();

            world.Render(); // hoitaa koko world-puun renderoinnin.

            Light.Disable();
            Texture.ActiveUnit(0);
            text.Begin();
            text.Print("objs: " + Settings.NumOfObjects, font, Color.White);
            text.End();
            Light.Enable();

            SwapBuffers();
        }

    }
}
