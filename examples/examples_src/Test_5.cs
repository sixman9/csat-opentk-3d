#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
// collision detection test
// 2 autoa liikkuu ja jos ohjattava törmää toiseen, sekin pysähtyy.
// bounding boxeilla tsekataan. tosin välillä autot menee toistensa läpi. enemmän tarkistuksia pitäisi olla.

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
        private int _oldMouseX, _oldMouseY;
        Camera cam = new Camera();
        Font font = new Font(FontFamily.GenericSansSerif, 24.0f);
        TextPrinter text = new TextPrinter();

        Sky skybox = new Sky("skybox");
        Mesh car, car2;
        Object2D groundPlane = new Object2D("ground");

        CarInfo carinfo;

        Node world = new Node("world");

        Light light = new Light("light");

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
            light.Position = new Vector3(10, 100, 10);
            light.UpdateColor();
            light.SetLight(true);
            world.Add(light); // lisää valo

            skybox.LoadSkybox("sky/sky_", "jpg", 100);
            world.Add(skybox);

            carinfo.up = 0.2f;
            carinfo.down = 0.1f;
            carinfo.max = 5;

            car = new ObjModel("car", "car.obj", 2, 2, 2);
            car.FixRotation.Y = -90;
            car.Rotation.Y = 90;

            car2 = new ObjModel("truck", "truck.obj", 1, 1, 1);
            car2.Rotation.Y = 90;
            car2.Position.X = -10;
            car2.Position.Y = -0.2f;
            car2.Position.Z = -15;

            groundPlane.Load("2.jpg");
            groundPlane.Position = new Vector3(0, -0.2f, 0);
            groundPlane.Rotation = new Vector3(90, 0, 0);
            world.Add(groundPlane);

            cam.Position.Z = 15;
            cam.Position.Y = 2;

            // lisätään autot maailmaan että coll.det. onnistuu
            world.Add(car);
            world.Add(car2);

            Util.Set3DMode();

            Fog.CreateFog(FogMode.Exp, 20, 200, 0.02f);
        }

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
        protected override void OnResize(EventArgs e)
        {
            Util.Resize(Width, Height, 1.0, 1000);
        }

        /// <summary>
        /// Called when it is time to setup the next frame.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Exit();

            // auton ohjaus
            float speed = (float)(5 * e.Time);

            if (Keyboard[Key.Left])
                if (carinfo.speed != 0)
                    if (carinfo.speed < 0)
                        car.Rotation.Y -= 20 * speed;
                    else
                        car.Rotation.Y += 20 * speed;

            if (Keyboard[Key.Right])
                if (carinfo.speed != 0)
                    if (carinfo.speed < 0)
                        car.Rotation.Y += 20 * speed;
                    else
                        car.Rotation.Y -= 20 * speed;

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
            if (Keyboard[Key.R]) cam.Position.Y++;
            if (Keyboard[Key.F]) if (cam.Position.Y > 1) cam.Position.Y--;
            if (Mouse[MouseButton.Left])
            {
                cam.TurnXZ(Mouse.X - _oldMouseX);
                cam.LookUpXZ(Mouse.Y - _oldMouseY);
            }
            _oldMouseX = Mouse.X; _oldMouseY = Mouse.Y;

            // Tsekkaa törmätäänkö:
            Vector3 oldPos = car.Position; // alkup paikka talteen
            car.MoveXZ(speed * carinfo.speed); // laske uusi paikka
            if (Intersection.CheckCollisionBB_Poly(ref world, oldPos, car.Position, ref car) == true) // törmäys
            {
                Log.WriteDebugLine("car1 poks!");
                car.Position = oldPos; // palauta vanha paikka
                carinfo.speed = 0;
            }
            else
            {
                // auto 2.
                oldPos = car2.Position; // alkup paikka talteen
                car2.MoveXZ(-speed * 2); // laske uusi paikka

                if (Intersection.CheckCollisionBB_Poly(ref world, oldPos, car2.Position, ref car2) == true) // törmäys
                {
                    Log.WriteDebugLine("poks!");
                    car2.Position = oldPos; // palauta vanha paikka
                }
                else car2.Rotation.Y += 1f;
            }

        }

        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            cam.UpdateXZ();
            Frustum.CalculateFrustum();

            world.Render(); // skybox, groundplane, car ja car2 on lisätty worldiin joten tämä renderoi ne.

            Light.Disable();
            text.Begin();
            text.Print("Collision detection -- objs: " + Settings.NumOfObjects, font, Color.White);
            text.End();
            Light.Enable();

            SwapBuffers();
        }

    }
}
