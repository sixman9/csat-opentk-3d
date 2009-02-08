#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

// path testi
// ladataan kaupunki ja camerapath joka liitetään kameraan.

// skyboxin tilalla ladataan puolipallo johon texturointi. enemmän polyja mutta vain 1 texture.


using System;
using System.Drawing;
using CSat;
using OpenTK;
using OpenTK.Math;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{
    class Game10 : GameWindow
    {
        Sky skydome = new Sky("sky");

        Camera cam = new Camera();
        ITextPrinter printer = new TextPrinter();
        TextureFont font = new TextureFont(new Font(FontFamily.GenericSerif, 24.0f));
        public Game10(int width, int height) : base(width, height, GraphicsMode.Default, "Camera path") { }

        Node world = new Node(); // tänne lisäillään kaikki kamat
        Mesh city;
        Path cameraPath;

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

            const float SC = 100;
            city = new ObjModel("city", "city.obj", SC, SC, SC);
            world.Add(city);

            // lataa reitit
            cameraPath = new Path("path", "camerapath.obj", SC, SC, SC); // sama skaalaus ku cityssä

            cameraPath.MakeCurve(3); // tehdään reitistä spline
            Node cam_tmp = cam;
            cameraPath.FollowPath(ref cam_tmp, true, true);
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

        public override void OnUpdateFrame(UpdateFrameEventArgs e)
        {
            if (Keyboard[Key.Escape]) Exit();
            cameraPath.UpdatePath((float)e.Time * 5);
        }

        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            Settings.NumOfObjects = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            Frustum.CalculateFrustum();
            world.Render();

            Texture.ActiveUnit(0);
            printer.Begin();
            printer.Draw("objs: " + Settings.NumOfObjects, font);
            printer.End();
            GL.MatrixMode(MatrixMode.Modelview);

            SwapBuffers();
        }

    }
}
