#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
/**
 *  ladataan objekteja ja md5 malleja.
 *  liitetään objekteja toisiinsa ja pyöritellään.
 *  
 *  objektien kloonaus Clone käskyllä. jakaa saman datan paitsi paikka/asentotiedot.
 * 
 *  ja kaikki renderoituu render käskyllä.
 *  
 *  asetetaan myös valo. se pitää päivittää kameran päivityksen jälkeen.
 *  
 * md5 mallille ei lasketa vielä normaaleja joten se näyttää vaihtavan väriä kameran mukaan.. TODO
 * 
 */
using System;
using System.Drawing;
using CSat;
using CSLoader;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;
using OpenTK.Input;
using OpenTK.Platform;
using System.Collections;

namespace CSatExamples
{
    class Game8 : GameWindow
    {
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

            Light.Enable();
            light.position = new Vector3(-100, 100, 0);
            light.UpdateColor(0);
            light.SetLight(0, true);
            Light.Add(light); // lisää valo


            Mouse.ButtonDown += MouseButtonDown;
            Mouse.ButtonUp += MouseButtonUp;

            // Lataukset
            skybox.Load("sky2_", "jpg", 100);
            world.Add(skybox); // skybox aina ekana koska se on kaikkien takana

            obj[0] = new Object3D("Head/head.obj2", null); // ladataan kerran
            obj[0].position.Z = -2;

            // tehdään muutama kopio, siirretään vierekkäin
            obj[1] = obj[0].Clone();
            obj[1].position.X = 5;

            obj[2] = obj[0].Clone();
            obj[2].position.X = 10;

            obj[3] = obj[0].Clone();
            obj[3].position.X = 15;

            obj[4] = obj[0].Clone();
            obj[4].position.X = 20;

            obj[5] = new Object3D("Head/arm.obj2", null);
            obj[6] = new Object3D("Head/saw.obj2", null);
            obj[5].Add(obj[6]); // liitä saha käteen
            obj[1].Add(obj[5]); // ja käsi toiseen päähän           

            // lisää kamat worldiin
            for (int q = 0; q < 4; q++) // EI 5 ja 6 koska ne on liitetty jo
                world.Add(obj[q]);

            // rumilus
            model.Load("Ugly/Ukko.mesh", null);
            model.LoadAnim("Ugly/Ukko_action2.anim", ref anim[0]);
            model.position.X = -4;
            model.fixRotation.X = -90; // ukko on "makaavassa" asennossa joten nostetaan se fixRotationilla pystyyn.

            // modelia ei voi liittää worldiin (tai voi mut se ei renderoidu) joten käytetään
            // animatedmodelia johon modelin tiedot ja se lisätään maailmaan.
            uglyModel = new AnimatedModel((IModel)model);
            world.Add(uglyModel);

            obj[4] = new Object3D("scene1.obj", .2f, .2f, .2f, null);
            obj[4].position.X = 10;
            obj[4].fixRotation.X = -90;
            obj[4].SetDoubleSided(8, true); // katos pitää pistää 2 puoliseks, muuten se ei näy alhaalta päin
            world.Add(obj[4]);

            Camera.cam.position.Y = 5;
            Camera.cam.position.Z = 15;

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
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
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
            if (Keyboard[Key.W]) Camera.cam.MoveXZ(spd, 0);
            if (Keyboard[Key.S]) Camera.cam.MoveXZ(-spd, 0);
            if (Keyboard[Key.A]) Camera.cam.MoveXZ(0, -spd);
            if (Keyboard[Key.D]) Camera.cam.MoveXZ(0, spd);
            if (Keyboard[Key.R]) Camera.cam.position.Y++;
            if (Keyboard[Key.F]) Camera.cam.position.Y--;
            if (mouseButtons[(int)MouseButton.Left])
            {
                Camera.cam.TurnXZ(Mouse.XDelta);
                Camera.cam.LookUpXZ(Mouse.YDelta);
            }
            int tmp = Mouse.XDelta; tmp = Mouse.YDelta;

            // pöytä pyörimään
            obj[0].rotation.Y += (float)e.Time * 100;

            obj[1].rotation.Y -= (float)e.Time * 50;
            obj[2].rotation.Y -= (float)e.Time * 10;
            obj[3].rotation.Y += (float)Math.Sin((float)e.Time * 100) * 10;

            // saha
            obj[6].SetMeshRotation(0, new Vector4(0, 0, 1, obj[0].rotation.Y * 5));
            //obj[6].rotation.Z = obj[0].rotation.Y * 5;

            // ja hullu ukko huittisista eiku pyörii vaa
            model.rotation.Y -= (float)e.Time * 100;

            // arm
            obj[5].rotation.Y = obj[1].rotation.Y / 5;

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

            Camera.cam.UpdateXZ();
            Frustum.CalculateFrustum();
            Light.UpdateLights(); // päivitä valot kameran asettamisen jälkeen

            model.Update((float)e.Time, ref anim[0]);

            world.CalculatePositions(); // laske objektien paikat

            world.Render(); // hoitaa koko world-puun renderoinnin.

            Light.Disable();
            Texture.ActiveUnit(0);
            printer.Begin();
            if (MainClass.UseFonts) printer.Draw("objs: " + Settings.NumOfObjects, font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);

            SwapBuffers();
        }

    }
}
