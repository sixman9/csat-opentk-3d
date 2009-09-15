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

// billboard tut:
//  http://www.lighthouse3d.com/opengl/billboarding/index.php3?billCheat

using OpenTK.Graphics.OpenGL;

namespace CSat
{
    public class Billboard : Object2D
    {
        public static float AlphaMin = 0.1f;

        public Billboard(string name)
            : base(name)
        {
        }
        public Billboard(string name, string textureFileName)
            : base(name)
        {
            Load(textureFileName);
        }

        public static void BillboardBegin(Texture tex, float x, float y, float z, float size)
        {
            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);

            int i, j;
            size *= 0.01f;

            GL.Enable(EnableCap.Texture2D);
            tex.Bind();

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Lighting);

            GL.PushMatrix();
            GL.Translate(x, y, z);

            GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);

            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    if (i == j) Util.ModelMatrix[i * 4 + j] = 1;
                    else Util.ModelMatrix[i * 4 + j] = 0;
                }
            }
            GL.LoadMatrix(Util.ModelMatrix);
            GL.Scale(size, size, size);
        }

        public static void BillboardEnd()
        {
            GL.PopAttrib();
            GL.PopMatrix();
        }

        public static void BillboardRender(Object2D obj)
        {
            obj.RenderVBO();
        }

        protected override void RenderObject()
        {
            RenderMesh();
        }

        public override void Render()
        {
            base.Render(); // renderoi objektin ja kaikki siihen liitetyt objektit
        }

        public new void RenderMesh()
        {
            RenderBillboard(Position.X, Position.Y, Position.Z, Front.X);
        }

        public void RenderBillboard(float x, float y, float z, float size)
        {
            BillboardBegin(texture, x, y, z, size);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, AlphaMin);
            vbo.Render();
            BillboardEnd();
        }

    }
}
