#region --- MIT License ---
/* 
 * This file is part of CSat - small C# 3D-library
 * 
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 ***
 * -mjt,
 * email: matola@sci.fi
 */
#endregion

/* 
 * kuvien lataus ja piirto.
 * 
 * billboard tut:
 *  http://www.lighthouse3d.com/opengl/billboarding/index.php3?billCheat
 * 
 */
using System;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public class Object2D : Node
    {
        public Vector2 Scale = new Vector2(1, 1);
        protected Texture texture = null;
        public Texture Texture2D
        {
            get { return texture; }
        }

        protected VBO vbo = null;

        public Object2D(string name)
        {
            Name = name;
        }
        public Object2D(string name, string fileName)
        {
            Name = name;
            Load(fileName);
        }

        void Bind()
        {
            texture.Bind();
        }

        /// <summary>
        /// lataa kuva.
        /// </summary>
        /// <param name="fileName"></param>
        public void Load(string fileName)
        {
            texture = Texture.Load(fileName);

            int[] ind = new int[] { 0, 1, 3, 1, 2, 3 };

            int w = texture.Width / 2;
            int h = texture.Height / 2;

            Vector3[] vs = new Vector3[]
			{
				new Vector3(-w, -h,0),
				new Vector3(-w, h,0),
				new Vector3(w, h,0),
				new Vector3(w, -h,0)
			};

            Vector2[] uv = new Vector2[]
			{
				new Vector2(1,0),
				new Vector2(1,1),
				new Vector2(0,1),
				new Vector2(0,0)
			};

            Vector3[] norm = new Vector3[]
			{
				new Vector3(0, 0, 1),
				new Vector3(0, 0, 1),
				new Vector3(0, 0, 1),
				new Vector3(0, 0, 1)
			};

            vbo = new VBO();
            vbo.DataToVBO(vs, ind, norm, uv);

            // scale
            Front.X = 1;
            Front.Y = 1;

        }

        public override void Dispose()
        {
            Log.WriteDebugLine("Disposed: " + Name);

            if (texture != null) texture.Dispose();
            if (vbo != null) vbo.Dispose();

            texture = null;
            vbo = null;
        }
        
        protected override void RenderObject()
        {
            RenderMesh();
        }

        public override void Render()
        {
            base.Render(); // renderoi objektin ja kaikki siihen liitetyt objektit
        }

        public void RenderMesh()
        {
            Translate();
            Render(Scale);
        }

        public void Render(Vector2 scale)
        {
            GL.PushMatrix();

            GL.Disable(EnableCap.CullFace);
            GL.Scale(scale.X, scale.Y, 1);

            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            if (vbo != null)
            {
                texture.Bind();
                vbo.Render();
            }

            GL.PopAttrib();
            GL.PopMatrix();
        }

        /// <summary>
        /// piirrä 2d-kuva. [0,0] kohta on vasen ylänurkka (jos asetettu 2d-mode)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rotate"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        public void Draw(int x, int y, float rotate, float sx, float sy)
        {
            GL.PushMatrix();

            GL.Translate(x, Util.ScreeenHeight - y, 0);
            GL.Rotate(rotate, 0, 0, 1);
            GL.Scale(sx, sy, 1);

            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            if (vbo != null)
            {
                texture.Bind();
                vbo.Render();
            }
            GL.PopAttrib();
            GL.PopMatrix();
        }

        public void RenderVBO()
        {
            vbo.Render();
        }
    }
}
