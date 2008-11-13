#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

// camera collision, walking in the city
// voit kävellä kaupungilla. 
// 6dof camera
// TAB vaihtaa näkymän silmistäpäin / äijän takaa  / lentomoodi

// auton reitti on vähän miten sattuu joten siihen pieni korjaus:
//  xz tasossa tein sen reitin ja jaksanu alkaa säätää y arvoja blenderissä joten etsitään 
// oikea y xz kohdalta ja korjataan samalla pathi.

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
    class Game11 : GameWindow
    {
        Skydome skydome = new Skydome();

        Camera cam = new Camera();
        ITextPrinter printer = new TextPrinter();
        TextureFont font = new TextureFont(new Font(FontFamily.GenericSerif, 24.0f));
        public Game11(int width, int height) : base(width, height, GraphicsMode.Default, "City") { }

        Group world = new Group("world"); // tänne lisäillään kaikki kamat
        Object3D city = new Object3D();
        Object3D car = new Object3D();
        MD5Model model = new MD5Model();
        MD5Model.MD5Animation[] anim = new MD5Model.MD5Animation[5];
        AnimatedModel uglyModel; // tätä käytetään kun ugly rendataan

        Object3D carPath = new Object3D();

        byte mode = 0;
        bool flying = false, moving = false;
        bool tab = false;

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(0, 0, 0, 0);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.CullFace);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            Mouse.ButtonDown += MouseButtonDown;
            Mouse.ButtonUp += MouseButtonUp;
            Util.Set3DMode();

            skydome.Load("sky/space.jpg", 1f);
            world.Add(skydome); // skydome aina ekana koska se on kaikkien takana

            model.Load("Ugly/Ukko.mesh");
            model.LoadAnim("Ugly/Ukko_walk.anim", ref anim[0]);
            uglyModel = new AnimatedModel((IModel)model);
            uglyModel.FixRotation.X = -90; // ukko on "makaavassa" asennossa joten nostetaan se fixRotationilla pystyyn.
            uglyModel.FixRotation.Z = 180; // säätöä.. katse eteen päin!

            car.Load("car.obj", 7, 7, 7);
            car.FixRotation.Y = 90;
            world.Add(car);

            const float SC = 100;
            city.Load("city.obj", SC, SC, SC);
            world.Add(city);

            // lataa reitit
            carPath.Load("carpath.obj", SC, SC, SC); // sama skaalaus ku cityssä
            car.FollowPath(ref carPath, true, true);
            car.MakeCurve(4); // tehdään reitistä spline

            cam.Position.Y = 60;
            cam.Front.Z = -10;
            cam.Update6DOF();

            // car path on tehty aikalailla xz tasossa (tai jotain y arvoja säädetty mutta menee aika pieleen),
            // joten korjataan ne y arvot. tämä katsoo joka vertexin kohdalta y arvon ja lisää siihen yp:n (tässä 0).
            car.FixPathY(0, ref city);
        }



        bool[] mouseButtons = new bool[5];
        void MouseButtonDown(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = true; }
        void MouseButtonUp(MouseDevice sender, MouseButton button) { mouseButtons[(int)button] = false; }

        public override void OnUnload(EventArgs e)
        {
            font.Dispose();
            Util.ClearArrays(); // poistaa kaikki materiaalit ja texturet
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            Util.Resize(e.Width, e.Height, 1.0, 1000);
        }

        public void CheckMove(ref Vector3 orig, ref Vector3 newpos, ref Object3D obj)
        {
            if (Intersection.CheckIntersection(ref orig, ref newpos, ref obj))
                newpos = orig;
        }

        public override void OnUpdateFrame(UpdateFrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Exit();
            if (Keyboard[Key.Tab])
            {
                if (tab == false)
                {
                    tab = true;
                    mode++;
                    if (mode == 3) mode = 0;

                    if (mode == 1)
                    {
                        uglyModel.Add(cam); // kiinnitä kamera ukkoon
                    }
                    else
                    {
                        uglyModel.Remove(cam);
                    }

                }
            }
            else tab = false;

            Vector3 origCamPos = cam.Position; // ensin alkuperäinen paikka talteen
            moving = false;
            // ohjaus
            float spd = 0.2f;
            if (Keyboard[Key.ShiftLeft]) spd = 1;
            if (Keyboard[Key.W])
            {
                moving = true;
                cam.MoveForward(spd, !flying);
            }
            if (Keyboard[Key.S])
            {
                moving = true;
                cam.MoveForward(-spd, !flying);
            }
            if (Keyboard[Key.A])
            {
                moving = true;
                cam.StrafeRight(-spd, !flying);
            }
            if (Keyboard[Key.D])
            {
                moving = true;
                cam.StrafeRight(spd, !flying);
            }
            //if (Keyboard[Key.X]) cam.RotateZ(-spd*2); // Roll
            //if (Keyboard[Key.Z]) cam.RotateZ(spd*2);

            // kameran päivityksen jälkeen tarkistetaan onko orig->uus liike mahdollinen (törmätäänkö johonkin?)
            // ellei, palautetaan orig kameraan.
            CheckMove(ref origCamPos, ref cam.Position, ref city);

            if (mouseButtons[(int)MouseButton.Left])
            {
                cam.RotateX(-Mouse.YDelta);
                cam.RotateY(-Mouse.XDelta);

                if (mode == 1) uglyModel.Rotation.Y += Mouse.XDelta;
                // TODO kun kameraa käännetään, ukon pitää kääntyä ja kameran
                // pitää pysyä sen takana.
            }
            int tmp = Mouse.XDelta; tmp = Mouse.YDelta;

            cam.Up = new Vector3(0, 1, 0);

            Vector3 tmpV;
            // laske kameralle Y (luodaan vektori kamerasta kauas alaspäin ja otetaan leikkauspiste. sit leikkauspisteen y asetetaan kameraan)
            // (jos mode!=2 eli lentomoodi)
            if (mode != 2)
            {
                tmpV = Camera.cam.Position;
                tmpV.Y = -10000;
                if (Intersection.CheckIntersection(ref Camera.cam.Position, ref tmpV, ref city))
                {
                    Camera.cam.Position.Y = Intersection.intersection.Y + 3;
                }

                if (mode == 1)
                {
                    //Camera.cam.Position.X = Camera.cam.Matrix[12]; // TODO kameran paikka lasketaan matrixiin (Objectinfossa/groupissa)
                    //Camera.cam.Position.Z = Camera.cam.Matrix[14]; ..pitäisi laskea jos se on liitetty ukkoon (nyt ei laske)
                }
            }

            // laske autolle Y (reitti ei seuraa maaston korkeutta oikein niin lasketaan se sitten tässä).
            car.UpdatePath((float)e.Time * 2.5f);
            car.Position.Y = 1000;
            tmpV = car.Position;
            tmpV.Y = -10000;
            if (Intersection.CheckIntersection(ref car.Position, ref tmpV, ref city))
            {
                car.Position.Y = Intersection.intersection.Y + 0.7f;
            }

        }

        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            flying = false;
            if (mode == 1)
            {
                uglyModel.Position = cam.Position;

                // muutetaan hieman kameran paikkaa, asetetaan kohta takas
                cam.Position.Y += 7;
                cam.Position.Z += 10;
            }
            else if (mode == 2) // lentomoodi
            {
                flying = true;
            }
            cam.Update6DOF();

            Frustum.CalculateFrustum();
            world.Render();

            if (mode == 1)
            {
                // takas alkup paikkaan
                cam.Position.Y -= 7;
                cam.Position.Z -= 10;

                if (moving) model.Update((float)e.Time * 15, ref anim[0]);
                uglyModel.Render();
            }

            Texture.ActiveUnit(0);
            printer.Begin();
            if (MainClass.UseFonts) printer.Draw("mode: " + mode + "  objs: " + Settings.NumOfObjects, font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);

            SwapBuffers();
        }

    }
}
