#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
/* 
 * 2D testaus
 * 
 */

using System;
using System.Drawing;
using CSat;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Platform;

namespace CSatExamples
{
    class Game1 : GameWindow
    {
        Font font = new Font(FontFamily.GenericSansSerif, 24.0f);
        TextPrinter text = new TextPrinter();

        Object2D t1 = new Object2D("image");
        Object2D[] tx = new Object2D[3];
        float angle = 0;

        public Game1(int width, int height) : base(width, height, GraphicsMode.Default, "2D test") { }

        /// <summary>Load resources here.</summary>
        public override void OnLoad(EventArgs e)
        {
            GL.ClearColor(System.Drawing.Color.Blue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.CullFace);
            GL.ShadeModel(ShadingModel.Smooth);

            // lataa kuva
            t1.Load("back.jpg");

            // lataa 3 kuvaa ja pistä ne samaan vbo:hon (niiden kokotiedot)
            string[] images = { "1.png", "2.png", "3.png" };
            for (int q = 0; q < 3; q++)
            {
                tx[q] = new Object2D("image_" + q);
                tx[q].Load(images[q]);
            }

            Util.Set2DMode();
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
            {
                Exit();
            }
        }

        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            t1.Draw(Width / 2 + 20, Height / 2, 0, 2, 2);

            tx[0].Draw(Width / 2, Height / 2, angle, 2 * (float)Math.Sin((float)angle), 2 * (float)Math.Sin((float)angle));
            tx[1].Draw(100, Height - 50, angle, 1, 1);
            tx[2].Draw(Width - 100, 100, -angle, 1, 1);

            angle += (float)e.Time * 5;

            Texture.ActiveUnit(0);

            text.Begin();
            text.Print("2D test", font, Color.White);
            text.End();

            SwapBuffers();
        }

    }
}
