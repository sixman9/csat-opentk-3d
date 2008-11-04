#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

// camera collision, walking in the city
// voit kävellä kaupungilla. 
// TAB vaihtaa näkymän silmistäpäin / äijän takaa
using System;
using System.Drawing;
using CSat;
using CSLoader;
using OpenTK;
using OpenTK.Math;
using OpenTK.Graphics;
using OpenTK.Input;
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
        AnimatedModel uglyModel; // tätä käytetään kun ugly lisätään worldiin.

        Object3D carPath = new Object3D();

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
            world.Add(uglyModel);
            uglyModel.FixRotation.X = -90; // ukko on "makaavassa" asennossa joten nostetaan se fixRotationilla pystyyn.
            uglyModel.FixRotation.Z = 180; // säätöä.. katse eteen päin!

            car.Load("car.obj", 7, 7, 7);
            world.Add(car);

            const float SC = 100;
            city.Load("city.obj", SC, SC, SC);
            world.Add(city);

            // lataa reitit
            carPath.Load("carpath.obj", SC, SC, SC); // sama skaalaus ku cityssä
            car.FollowPath(ref carPath, true, true);

            cam.Position.Y = 60;

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

            Vector3 origCamPos = cam.Position; // ensin alkuperäinen talteen

            // ohjaus
            float spd = 1;
            if (Keyboard[Key.ShiftLeft]) spd = 2;
            if (Keyboard[Key.W]) cam.MoveXZ(spd, 0);
            if (Keyboard[Key.S]) cam.MoveXZ(-spd, 0);
            if (Keyboard[Key.A]) cam.MoveXZ(0, -spd);
            if (Keyboard[Key.D]) cam.MoveXZ(0, spd);
            if (Keyboard[Key.R]) cam.Position.Y++;
            if (Keyboard[Key.F]) cam.Position.Y--;

            // kameran päivityksen jälkeen tarkistetaan onko orig->uus liike mahdollinen (törmätäänkö johonkin?)
            // ellei, palautetaan orig kameraan.
            CheckMove(ref origCamPos, ref cam.Position, ref city);

            if (mouseButtons[(int)MouseButton.Left])
            {
                cam.TurnXZ(Mouse.XDelta);
                cam.LookUpXZ(Mouse.YDelta);
            }
            int tmp = Mouse.XDelta; tmp = Mouse.YDelta;

            // laske kameralle Y (luodaan vektori kamerasta kauas alaspäin ja otetaan leikkauspiste. sit leikkauspisteen y asetetaan kameraan)
            Vector3 tmpV = Camera.cam.Position;
            tmpV.Y = -10000;
            if (Intersection.CheckIntersection(ref Camera.cam.Position, ref tmpV, ref city))
            {
                Camera.cam.Position.Y = Intersection.intersection.Y + 3;
            }


            // laske autolle Y (reitti ei seuraa maaston korkeutta oikein niin lasketaan se sitten tässä).
            car.UpdatePath((float)e.Time * 0.1f);
            car.Position.Y = 1000;
            tmpV = car.Position;
            tmpV.Y = -10000;
            if (Intersection.CheckIntersection(ref car.Position, ref tmpV, ref city))
            {
                car.Position.Y = Intersection.intersection.Y;
            }



        }

        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            cam.UpdateXZ();
            Frustum.CalculateFrustum();
            world.Render();

            Texture.ActiveUnit(0);
            printer.Begin();
            if (MainClass.UseFonts) printer.Draw("objs: " + Settings.NumOfObjects, font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);

            SwapBuffers();
        }

    }
}
