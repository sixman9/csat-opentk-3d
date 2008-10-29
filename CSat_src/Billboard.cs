#region --- License ---
/*
Copyright (C) 2008 mjt[matola@sci.fi]

This file is part of CSat - small C# 3D-library

CSat is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
 
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.
 
You should have received a copy of the GNU Lesser General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.


-mjt,  
email: matola@sci.fi
*/
#endregion

// billboard tut:
//  http://www.lighthouse3d.com/opengl/billboarding/index.php3?billCheat

using OpenTK.Graphics;

namespace CSat
{
    public class Billboard : Object2D
    {
        public Billboard()
        {
        }
        public Billboard(string textureFileName)
        {
            Load(textureFileName);
        }

        public static float AlphaMin = 0.1f;

        static float[] modelview = new float[16];
        public static void BillboardBegin(Texture tex, float x, float y, float z, float size)
        {
            int i, j;
            size *= 0.01f;

            GL.Enable(EnableCap.Texture2D);
            tex.Bind();

            GL.Disable(EnableCap.CullFace);
            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            SetBlend();
            GL.PushMatrix();
            GL.Translate(x, y, z);

            GL.GetFloat(GetPName.ModelviewMatrix, modelview);

            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    if (i == j) modelview[i * 4 + j] = 1;
                    else modelview[i * 4 + j] = 0;
                }
            }
            GL.LoadMatrix(modelview);
            GL.Scale(size, size, size);
        }

        public static void BillboardEnd()
        {
            GL.PopAttrib();
            GL.PopMatrix();
        }

        public static void BillboardRender(Object2D obj)
        {
            obj.Render();
        }

        public new void Render()
        {
            RenderBillboard(position.X, position.Y, position.Z, view.X);
        }

        public void RenderBillboard(float x, float y, float z, float size)
        {
            BillboardBegin(texture, x, y, z, size);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, AlphaMin);
            vbo.Render();
            BillboardEnd();
        }

    }
}
