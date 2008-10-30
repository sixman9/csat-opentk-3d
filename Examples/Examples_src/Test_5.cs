#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
// collision detection test
// 2 autoa liikkuu ja jos ohjattava törmää toiseen, sekin pysähtyy.
// bounding boxeilla tsekataan, välillä autot menee toistensa läpi.

// valo + sumu

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
    public struct CarInfo
    {
        public float speed;
        public float up, down, max;
    }

    class Game5 : GameWindow
    {
        Camera cam = new Camera();
        ITextPrinter printer = new TextPrinter();
        TextureFont font = new TextureFont(new Font(FontFamily.GenericSerif, 24.0f));

        Skybox skybox = new Skybox();
        Object3D car = new Object3D();
        Object3D car2 = new Object3D();
        Object2D groundPlane = new Object2D();

        CarInfo carinfo;

        Group world = new Group("world");

        Light light = new Light();

        public Game5(int width, int height) : base(width, height, GraphicsMode.Default, "Collision detection") { }

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
            light.position = new Vector3(10, 100, 10);
            light.UpdateColor();
            light.SetLight(true);
            Light.Add(light, 0); // lisää valo

            carinfo.up = 0.2f;
            carinfo.down = 0.1f;
            carinfo.max = 5;

            car = new Object3D("car.obj", 2, 2, 2);
            car.fixRotation.Y = -90;
            car.rotation.Y = 90;

            car2 = new Object3D("truck.obj", 1, 1, 1);
            car2.rotation.Y = 90;
            car2.position.X = -10;
            car2.position.Y = -0.2f;
            car2.position.Z = -15;

            skybox.Load("sky/sky_", "jpg", 100);

            groundPlane.Load("2.jpg");

            cam.position.Z = 15;
            cam.position.Y = 2;

            Mouse.ButtonDown += MouseButtonDown;
            Mouse.ButtonUp += MouseButtonUp;

            // lisätään autot maailmaan että coll.det. onnistuu
            world.Add(car);
            world.Add(car2);

            Util.Set3DMode();


            Fog.CreateFog(FogMode.Exp, 20, 200, 0.02f);
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

            // auton ohjaus
            float speed = (float)(5 * e.Time);

            if (Keyboard[Key.Left])
                if (carinfo.speed != 0)
                    if (carinfo.speed < 0)
                        car.rotation.Y -= 20 * speed;
                    else
                        car.rotation.Y += 20 * speed;

            if (Keyboard[Key.Right])
                if (carinfo.speed != 0)
                    if (carinfo.speed < 0)
                        car.rotation.Y += 20 * speed;
                    else
                        car.rotation.Y -= 20 * speed;

            if (Keyboard[Key.Up])
            {
                carinfo.speed += carinfo.up;
                if (carinfo.speed > carinfo.max) carinfo.speed = carinfo.max;
            }
            else
            {
                if (Keyboard[Key.Down])
                {
                    carinfo.speed -= carinfo.up;
                    if (carinfo.speed < -carinfo.max / 2) carinfo.speed = -carinfo.max / 2;
                }
                else
                {
                    if (carinfo.speed < 0) carinfo.speed += carinfo.down;
                    else carinfo.speed -= carinfo.down;

                    if (Math.Abs(carinfo.speed) <= 0.5f) carinfo.speed = 0;
                }
            }

            // kameran ohjaus (kameralle ei tässä esimerkissä törmäystarkistusta)
            float spd = 1;
            if (Keyboard[Key.ShiftLeft]) spd = 2;
            if (Keyboard[Key.W]) cam.MoveXZ(spd, 0);
            if (Keyboard[Key.S]) cam.MoveXZ(-spd, 0);
            if (Keyboard[Key.A]) cam.MoveXZ(0, -spd);
            if (Keyboard[Key.D]) cam.MoveXZ(0, spd);
            if (Keyboard[Key.R]) cam.position.Y++;
            if (Keyboard[Key.F]) if (cam.position.Y > 1) cam.position.Y--;
            if (mouseButtons[(int)MouseButton.Left])
            {
                cam.TurnXZ(Mouse.XDelta);
                cam.LookUpXZ(Mouse.YDelta);
            }
            int tmp = Mouse.XDelta; tmp = Mouse.YDelta; // joo, kikkailu. vaatii tän rivin!

            // Tsekkaa törmätäänkö:
            Vector3 oldPos = car.position; // alkup paikka talteen
            car.MoveXZ(speed * carinfo.speed); // laske uusi paikka
            if (world.CheckCollisionBB(oldPos, car.position, ref car) == true) // törmäys
            {
                car.position = oldPos; // palauta vanha paikka
                carinfo.speed = 0;
            }
            else
            {
                // auto 2.
                oldPos = car2.position; // alkup paikka talteen
                car2.MoveXZ(-speed * 2); // laske uusi paikka
                if (world.CheckCollisionBB(oldPos, car2.position, ref car2) == true) // törmäys
                {
                    car2.position = oldPos; // palauta vanha paikka
                }
                else car2.rotation.Y += 1f;
            }
        }

        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            skybox.Render(cam);
            cam.UpdateXZ();
            Frustum.CalculateFrustum();

            world.Render(); // car ja car2 on lisätty worldiin joten tämä renderoi ne.

            groundPlane.Render3D(0, -0.2f, 0, 90, 0, 0, 1, 1);

            Light.Disable();
            printer.Begin();
            if (MainClass.UseFonts) printer.Draw("Collision detection -- objs: " + Settings.NumOfObjects, font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);

            SwapBuffers();
        }

    }
}
