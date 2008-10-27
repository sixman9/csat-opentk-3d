#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
/* 
 * 2D testaus
 * 
 *  render2d piirtää kuvat niin että 0-piste on vasemmassa ylänurkassa.
 *
 *  kuvat piirretään niiden keskipisteestä. 
 */

/*
2D piirto:
   // vain 1 kuva
   Object2D t=new ObjecT2D();
   t.Load("kuva.jpg");
   t.Render();  // piirrä


   // kuvajoukko samassa vbo:ssa
   VBO aliens=new VBO();
   aliens.AllocVBO(1000, 1000, BufferUsageHint.StaticDraw);
   Object2D[] t=new Object2D [3];
  
   // ladataan kuvat ja lisätään kuvan tiedot (lev,kor) aliens vbo:hon
   Object2D t[0]=new Object2D(); t[0].Load("alien1.jpg", aliens);
   Object2D t[1]=new Object2D(); t[1].Load("alien2.jpg", aliens);
   Object2D t[2]=new Object2D(); t[2].Load("alien3.jpg", aliens);
   aliens.BeginRender(); // aseta renderointiasetukset
   foreach(Texture t in t) t.Render(aliens);
   aliens.EndRender(); // asetukset pois
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
        ITextPrinter printer = new TextPrinter();
        TextureFont font = new TextureFont(new Font(FontFamily.GenericSerif, 24.0f));

        Object2D t1 = new Object2D();
        Object2D[] tx = new Object2D[3];

        VBO vbot = new VBO();

        public Game1(int width, int height) : base(width, height, GraphicsMode.Default, "2D test") { }

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

            // lataa kuva
            t1.Load("back.jpg", null);

            // lataa 3 kuvaa ja pistä ne samaan vbo:hon (niiden kokotiedot)
            string[] images = { "1.png", "2.png", "3.png" };
            vbot.AllocVBO(1000, 1000, BufferUsageHint.StaticDraw);
            for (int q = 0; q < 3; q++)
            {
                tx[q] = new Object2D();
                tx[q].Load(images[q], vbot);
            }

            Util.Set2DMode();
        }

        #region OnUnload
        public override void OnUnload(EventArgs e)
        {
            font.Dispose();
            t1.Dispose();
            for (int q = 0; q < 3; q++) tx[q].Dispose();
            vbot.Dispose();

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
        }


        float angle = 0;
        int q = 0;
        /// <summary>
        /// Called when it is time to render the next frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        public override void OnRenderFrame(RenderFrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            base.OnRenderFrame(e);

            t1.Render2D(Width / 2 + 20, Height / 2, 0, 2, 2);

            vbot.BeginRender();
            {
                tx[0].Set(Width / 2, Height / 2, angle, 2 * (float)Math.Sin((float)angle), 2 * (float)Math.Sin((float)angle));
                tx[0].Render2D(vbot);
                tx[1].Render2D(100, Height-50, angle, 1, 1, vbot);
                tx[2].Render2D(Width - 100, 100, -angle, 1, 1, vbot);
            }
            vbot.EndRender();

            angle += (float)e.Time * 5;

            Texture.ActiveUnit(0);
            
            printer.Begin();
            if (MainClass.UseFonts) printer.Draw("2D test", font);
            printer.End();

            SwapBuffers();
        }

    }
}
