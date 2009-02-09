#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

// camera collision
// voit kävellä kaupungilla.
// 6dof camera

// auton reitti on vähän miten sattuu joten siihen pieni korjaus:
//  xz tasossa tein sen reitin ja jaksanu alkaa säätää y arvoja blenderissä joten etsitään
// oikea y xz kohdalta ja korjataan samalla pathi.

// auto ei vielä katso menosuuntaan vaan liukuu reittiä pitkin.

// -kesken-

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
    class Game11 : GameWindow
    {
        Sky skydome = new Sky("sky");

        Camera cam = new Camera("Camera");
        ITextPrinter printer = new TextPrinter();
        TextureFont font = new TextureFont(new Font(FontFamily.GenericSerif, 24.0f));
        public Game11(int width, int height) : base(width, height, GraphicsMode.Default, "City") { }

        Node world = new Node(); // tänne lisäillään kaikki kamat
        Mesh city, car;
        Mesh uglyModel; // animoitu tyyppi
        Path carPath;

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

            skydome.LoadSkydome("sky/space.jpg", 1f);
            world.Add(skydome); // skydome aina ekana koska se on kaikkien takana

            uglyModel = new AnimatedModel("ugly", "Ugly/Ukko.mesh");
            uglyModel.LoadAnim("walk", "Ugly/Ukko_walk.anim");

            uglyModel.FixRotation.X = -90; // ukko on "makaavassa" asennossa joten nostetaan se fixRotationilla pystyyn.
            uglyModel.FixRotation.Z = 180; // säätöä.. katse eteen päin!

            car = new ObjModel("car", "car.obj", 7, 7, 7);
            car.FixRotation.Y = 90;
            world.Add(car);

            const float SC = 100;
            city = new ObjModel("city", "city.obj", SC, SC, SC);
            world.Add(city);

            // lataa reitit
            carPath = new Path("carpath", "carpath.obj", SC, SC, SC); // sama skaalaus ku cityssä
            Node carInfo = car;
            carPath.FollowPath(ref carInfo, true, true);
            carPath.MakeCurve(4); // tehdään reitistä spline

            cam.Position.Y = 60;
            cam.Front.Z = -10;
            cam.Update6DOF();

            // car path on tehty xz tasossa (tai jotain y arvoja säädetty mutta menee aika pieleen),
            // joten korjataan ne y arvot. tämä katsoo joka vertexin kohdalta y arvon ja lisää siihen yp:n (tässä 0).
            carPath.FixPathY(0, ref city);
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

        public void CheckMove(ref Vector3 orig, ref Vector3 newpos, ref Mesh obj)
        {
            if (Intersection.CheckIntersection(ref orig, ref newpos, ref obj))
                newpos = orig;
        }

        public override void OnUpdateFrame(UpdateFrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Exit();

            /*if (Keyboard[Key.Tab]) TODO
            {
                if (tab == false)
                {
                    tab = true;
                    mode++;
                    if (mode == 3) mode = 0;

                    if (mode == 1)
                    {
                        //uglyModel.Add(cam); // kiinnitä kamera ukkoon 
                    }
                    else
                    {
                        //uglyModel.Remove(cam); 
                    }

                }
            }
            else tab = false;
            */

            Vector3 origCamPos = cam.Position; // ensin alkuperäinen paikka talteen
            moving = false;

            // ohjaus
            float spd = 1f;
            if (Keyboard[Key.ShiftLeft]) spd = 2;
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

            // kameran päivityksen jälkeen tarkistetaan onko orig->uus liike mahdollinen (törmätäänkö johonkin?)
            // ellei, palautetaan orig kameraan.
            CheckMove(ref origCamPos, ref cam.Position, ref city);

            if (mouseButtons[(int)MouseButton.Left] && mode != 1)
            {
                if (mode != 1) cam.RotateX(-Mouse.YDelta);
                cam.RotateY(-Mouse.XDelta);
            }

            int tmp = Mouse.XDelta; tmp = Mouse.YDelta;

            cam.Up = new Vector3(0, 1, 0);

            Vector3 tmpV;
            // laske kameralle Y (luodaan vektori kamerasta kauas alaspäin ja otetaan leikkauspiste. sit leikkauspisteen y asetetaan kameraan)
            // (jos mode!=2 eli lentomoodi)
            if (mode != 2)
            {
                tmpV = cam.Position;
                tmpV.Y = -10000;
                if (Intersection.CheckIntersection(ref cam.Position, ref tmpV, ref city))
                {
                    cam.Position.Y = Intersection.IntersectionPoint.Y + 3;
                }
            }

            // laske autolle Y (reitti ei seuraa maaston korkeutta oikein niin lasketaan se sitten tässä).
            carPath.UpdatePath((float)e.Time * 2.5f);
            car.Position.Y = 1000;
            tmpV = car.Position;
            tmpV.Y = -10000;
            if (Intersection.CheckIntersection(ref car.Position, ref tmpV, ref city))
            {
                car.Position.Y = Intersection.IntersectionPoint.Y + 0.7f;
            }

            if (moving) uglyModel.Update((float)e.Time * 15);

        }

        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            flying = false;
            if (mode == 1) // ukon takaa   TODO ei toimi
            {
                uglyModel.Position = cam.Position;

                Vector3 campos = cam.Position + new Vector3(0, 1, 1); ;

                Glu.LookAt(campos, uglyModel.Position, new Vector3(0, 1, 0));
                Frustum.CalculateFrustum();

                world.Render();
                uglyModel.Render();

            }
            else // mode0 ja mode2
            {

                if (mode == 2) // lentomoodi
                {
                    flying = true;
                }

                cam.Update6DOF();

                Frustum.CalculateFrustum();
                world.Render();
            }

            Texture.ActiveUnit(0);
            printer.Begin();
            printer.Draw("mode: " + mode + "  objs: " + Settings.NumOfObjects, font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);

            SwapBuffers();
        }

    }
}
