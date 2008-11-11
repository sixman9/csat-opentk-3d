#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
/*
 *  ladataan objekteja ja md5 malleja.
 *  liitetään objekteja toisiinsa ja pyöritellään.
 *  
 *  objektien kloonaus Clone käskyllä. jakaa saman datan paitsi paikka/asentotiedot.
 * 
 *  ja kaikki renderoituu render käskyllä.
 *  
 *  asetetaan myös valo. se pitää päivittää kameran päivityksen jälkeen.
 *  
 * 
 */
using System;
using System.Drawing;
using CSat;
using CSLoader;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Math;
using OpenTK.Platform;

namespace CSatExamples
{
    class Game8 : GameWindow
    {
        Camera cam = new Camera();

        Group world = new Group("world"); // tänne lisäillään kaikki kamat

        Skybox skybox = new Skybox();
        Object3D[] obj = new Object3D[10];
        MD5Model model = new MD5Model();
        MD5Model.MD5Animation[] anim = new MD5Model.MD5Animation[1];
        AnimatedModel uglyModel; // tätä käytetään kun ugly lisätään worldiin.

        ITextPrinter printer = new TextPrinter();
        TextureFont font = new TextureFont(new Font(FontFamily.GenericSerif, 24.0f));

        Light light = new Light();

        public Game8(int width, int height) : base(width, height, GraphicsMode.Default, "Group test") { }

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

            GL.Enable(EnableCap.ColorMaterial);

            Light.Enable();
            light.Position = new Vector3(50, 80, -100);
            Light.Add(light, 0); // lisää valo (aurinko)

            Mouse.ButtonDown += MouseButtonDown;
            Mouse.ButtonUp += MouseButtonUp;

            // Lataukset
            for (int q = 0; q < 10; q++) obj[q] = new Object3D();

            skybox.Load("sky/sky2_", "jpg", 100);
            world.Add(skybox); // skydome aina ekana koska se on kaikkien takana

            obj[0] = new Object3D("Head/head.obj2"); // ladataan kerran
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

            obj[5] = new Object3D("Head/arm.obj2");
            obj[6] = new Object3D("Head/saw.obj2");
            obj[5].Add(obj[6]); // liitä terä käteen
            obj[1].Add(obj[5]); // ja käsi päähän
            // koska obj[1]:stä ollaan muutettu lisäämällä siihen muuta kamaa, pitää bounding volume laskea uudelleen.
            obj[1].CalcBoundingVolumes();

            // lisää kamat worldiin
            for (int q = 0; q < 4; q++) // ei addata 5 ja 6 koska ne on liitetty jo obj[1]:seen
                world.Add(obj[q]);

            // rumilus
            model.Load("Ugly/Ukko.mesh");
            model.LoadAnim("Ugly/Ukko_action2.anim", ref anim[0]);
            // modelia ei voi liittää worldiin (tai voi mut se ei renderoidu) joten käytetään
            // animatedmodelia johon modelin tiedot ja se lisätään maailmaan.
            uglyModel = new AnimatedModel((IModel)model);
            uglyModel.Position.X = -4;
            uglyModel.FixRotation.X = -90; // ukko on "makaavassa" asennossa joten nostetaan se fixRotationilla pystyyn.
            world.Add(uglyModel);

            obj[4] = new Object3D("scene1.obj", 20, 20, 20);
            obj[4].Position.X = 10;
            // katos (sen nimi on toi None_Material__9) pitää pistää 2 puoliseks, muuten se ei näy alhaalta päin
            obj[4].SetDoubleSided("None_Material__9", true);
            // samoin seinät 2 puolisiks (blenderistä ja tutkimalla .obj tiedostoa selviää nämä nimet)
            obj[4].SetDoubleSided("None_Material__12", true);

            world.Add(obj[4]);

            cam.Position.Y = 5;
            cam.Position.Z = 15;

            Util.Set3DMode();
        }

        bool[] mouseButtons = new bool[5];
        void MouseButtonDown(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = true; }
        void MouseButtonUp(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = false; }

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

            // pöytä pyörimään
            obj[0].Rotation.Y += (float)e.Time * 100;

            obj[1].Rotation.Y -= (float)e.Time * 50;
            obj[2].Rotation.Y -= (float)e.Time * 10;
            obj[3].Rotation.Y += (float)Math.Sin((float)e.Time * 100) * 10;

            // ja hullu ukko huittisista eiku pyörii vaa
            uglyModel.Rotation.Y -= (float)e.Time * 50;

            /*
             *  obj[6].Position määrää pyörivän terän paikan. 
             *  Objects[0] paikassa on itse terä-objekti ja pyörittämällä sen z:aa, se pyörii
             *  nollapisteensä ympäri.
             *  
             *  eli:
             *  root <-paikka, Rotation, .-
             *    child <-terä-objekti + paikka, Rotation, .-
             */
            // pyörivä terä
            // ((Object3D)obj[6].Objects[0]).Rotation.Z = obj[0].Rotation.Y * 5; // 1.tapa
            obj[6].GetObject(0).Rotation.Z = obj[0].Rotation.Y * 5; // 2.tapa


            // arm
            obj[5].Rotation.Y = obj[1].Rotation.Y / 5;

            // TODO FIX---
            // prob: seuraavat ei toimi vielä, eli vaikka pyörivä terä on kädessä, se ei liiku kun kättä liikuttaa.
            /*obj[5].GetObject(0).Rotation.Z = obj[1].Rotation.Y / 2;
            obj[5].GetObject(3).Rotation.Z = -obj[1].Rotation.Y / 2;
            obj[5].Rotation.Z = obj[1].Rotation.Y / 5; // tällä tavalla pysyy kädessä mutta pyöritys menee pöydän 0 pisteen ympäri.
             */
        }

        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Settings.NumOfObjects = 0;
            base.OnRenderFrame(e);

            cam.UpdateXZ();
            Frustum.CalculateFrustum();

            Light.UpdateLights(); // päivitä valot kameran asettamisen jälkeen

            model.Update((float)e.Time, ref anim[0]);

            world.Render(); // hoitaa koko world-puun renderoinnin.

            Light.Disable();
            Texture.ActiveUnit(0);
            printer.Begin();
            if (MainClass.UseFonts) printer.Draw("objs: " + Settings.NumOfObjects, font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);
            Light.Enable();

            SwapBuffers();
        }

    }
}
