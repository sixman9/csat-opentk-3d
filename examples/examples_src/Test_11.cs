#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

using System;
using System.Drawing;
using CSat;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{
    class Game11 : GameWindow
    {
        private int _oldMouseX, _oldMouseY;
        Sky skydome = new Sky("sky");

        Camera cam = new Camera("Camera");
        Font font = new Font(FontFamily.GenericSansSerif, 24.0f);
        OpenTK.Graphics.TextPrinter text = new OpenTK.Graphics.TextPrinter();
        public Game11(int width, int height) : base(width, height, OpenTK.Graphics.GraphicsMode.Default, "City") { }

        Node world = new Node(); // tänne lisäillään kaikki kamat
        Mesh city, car;
        Path carPath;

        byte mode = 0;
        bool flying = false;
        bool tab = false;

        protected override void OnLoad(EventArgs e)
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

            Util.Set3DMode();

            skydome.LoadSkydome("sky/space.jpg", 1f);
            world.Add(skydome); // skydome aina ekana koska se on kaikkien takana

            car = new ObjModel("car", "car.obj", 7, 7, 7);
            car.FixRotation.Y = -90;
            world.Add(car);

            const float Scale = 110;
            city = new ObjModel("city", "city.obj", Scale, Scale, Scale);
            world.Add(city);

            // lataa reitit
            carPath = new Path("carpath", "carpath.obj", Scale, Scale, Scale); // sama skaalaus ku cityssä
            carPath.FollowPath(car, true, true);

            cam.Position.Y = 60;
            cam.Front.Z = -10;
            cam.Update6DOF();

            // car path on tehty xz tasossa (tai jotain y arvoja säädetty mutta menee aika pieleen),
            // joten korjataan ne y arvot. tämä katsoo joka vertexin kohdalta y arvon ja lisää siihen yp:n (tässä 0).
            carPath.FixPathY(0, ref city);
        }

        protected override void OnUnload(EventArgs e)
        {
            font.Dispose();
            Util.ClearArrays(); // poistaa kaikki materiaalit ja texturet
        }

        protected override void OnResize(EventArgs e)
        {
            Util.Resize(Width, Height, 1, 1000);
        }

        public void CheckMove(ref Vector3 orig, ref Vector3 newpos, ref Mesh obj)
        {
            if (Intersection.CheckIntersection(ref orig, ref newpos, ref obj))
                newpos = orig;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Exit();

            if (Keyboard[Key.Tab])
            {
                if (tab == false)
                {
                    tab = true;
                    mode++;
                    if (mode == 2) mode = 0;
                }
            }
            else tab = false;

            Vector3 origCamPos = cam.Position; // ensin alkuperäinen paikka talteen

            // ohjaus
            float spd = 1f;
            if (Keyboard[Key.ShiftLeft]) spd = 2;
            if (Keyboard[Key.W])
            {
                cam.MoveForward(spd, !flying);
            }
            if (Keyboard[Key.S])
            {
                cam.MoveForward(-spd, !flying);
            }
            if (Keyboard[Key.A])
            {
                cam.StrafeRight(-spd, !flying);
            }
            if (Keyboard[Key.D])
            {
                cam.StrafeRight(spd, !flying);
            }

            // kameran päivityksen jälkeen tarkistetaan onko orig->uus liike mahdollinen (törmätäänkö johonkin?)
            // ellei, palautetaan orig kameraan.
            CheckMove(ref origCamPos, ref cam.Position, ref city);

            if (Mouse[MouseButton.Left])
            {
                cam.RotateY(-(Mouse.X - _oldMouseX));
                cam.RotateX(-(Mouse.Y - _oldMouseY));
            }
            _oldMouseX = Mouse.X; _oldMouseY = Mouse.Y;

            cam.Up = new Vector3(0, 1, 0);

            Vector3 tmpV;
            // laske kameralle Y (luodaan vektori kamerasta kauas alaspäin ja otetaan leikkauspiste. sit leikkauspisteen y asetetaan kameraan)
            // (jos mode!=1 eli lentomoodi)
            if (mode != 1)
            {
                tmpV = cam.Position;
                tmpV.Y = -10000;
                if (Intersection.CheckIntersection(ref cam.Position, ref tmpV, ref city))
                {
                    cam.Position.Y = Intersection.IntersectionPoint.Y + 3;
                }
            }

            // laske autolle Y (reitti ei seuraa maaston korkeutta oikein niin lasketaan se sitten tässä).
            carPath.UpdatePath((float)e.Time * 0.4f);
            car.Position.Y = 1000;
            tmpV = car.Position;
            tmpV.Y = -10000;
            if (Intersection.CheckIntersection(ref car.Position, ref tmpV, ref city))
            {
                car.Position.Y = Intersection.IntersectionPoint.Y + 0.7f;
            }

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            flying = (mode==1) ? true : false;

            cam.Update6DOF();

            Frustum.CalculateFrustum();
            world.Render();

            Texture.ActiveUnit(0);
            text.Begin();
            text.Print("mode: " + (mode==0 ? "walking" : "flying") + "  (TAB changes mode)\nobjs: " + Settings.NumOfObjects, font, Color.White);
            text.End();

            SwapBuffers();
        }

    }
}
