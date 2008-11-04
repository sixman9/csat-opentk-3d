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

/* 
 * kuvien lataus ja piirto.
 * 
 * billboard tut:
 *  http://www.lighthouse3d.com/opengl/billboarding/index.php3?billCheat
 * 
 *  render2d piirt‰‰ kuvat niin ett‰ 0-piste on vasemmassa yl‰nurkassa.
 * 
 */

using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public class Object2D : ObjectInfo
    {
        public Object2D()
        {
        }
        public Object2D(string textureFileName)
        {
            Load(textureFileName);
        }

        protected Texture texture = new Texture();
        public Texture Texture2D
        {
            get { return texture; }
        }

        protected VBO vbo;

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
            name = fileName;

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

            vbo = new VBO();
            vbo.DataToVBO(vs, ind, null, uv, null, null, null);

            // scale
            View.X = 1;
            View.Y = 1;

        }

        public void Dispose()
        {
            texture.Dispose();
        }

        /// <summary>
        /// aseta haluttu kohta, haluttu asento ja koko (sx==1 sy==1 niin normaali koko) x,y on kuvan keskikohta
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rotate"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        public void Set(int x, int y, float rotate, float sx, float sy)
        {
            Position.X = x;
            Position.Y = y;
            Rotation.Z = rotate;
            View.X = sx;
            View.Y = sy;
        }
        public void Set(float x, float y, float z, float rx, float ry, float rz, float sx, float sy)
        {
            Position.X = x;
            Position.Y = y;
            Position.Z = z;
            Rotation.X = rx;
            Rotation.Y = ry;
            Rotation.Z = rz;
            View.X = sx;
            View.Y = sy;
        }

        /// <summary>
        /// voi erikseen valita mit‰ texture unittei k‰ytet‰‰n jos multitexture
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public void UseTextureUnits(bool t0, bool t1, bool t2)
        {
            vbo.UseTextureUnits(t0, t1, t2);
        }

        public void Render3D()
        {
            Render3D(Position.X, Position.Y, Position.Z,
                Rotation.X, Rotation.Y, Rotation.Z,
                View.X, View.Y);
        }

        /// <summary>
        /// renderoi 2d tason 3d maailmaan
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="rx"></param>
        /// <param name="ry"></param>
        /// <param name="rz"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        public void Render3D(float x, float y, float z, float rx, float ry, float rz, float sx, float sy)
        {
            GL.PushMatrix();

            GL.Disable(EnableCap.CullFace);

            GL.Translate(x, y, z);
            GL.Rotate(rx, 1, 0, 0);
            GL.Rotate(ry, 0, 1, 0);
            GL.Rotate(rz, 0, 0, 1);
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

        public void Render2D(int x, int y, float rotate, float sx, float sy, VBO vbo)
        {
            Set(x, y, rotate, sx, sy);
            Render2D();
        }
        public void Render2D(int x, int y, float rotate, float sx, float sy)
        {
            Set(x, y, rotate, sx, sy);
            Render2D();
        }

        public void Render2D()
        {
            GL.PushMatrix();

            GL.Translate(Position.X, Util.ScreeenHeight - Position.Y, 0);
            GL.Rotate(Rotation.Z, 0, 0, 1);
            GL.Scale(View.X, View.Y, 1);

            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            texture.Bind();

            vbo.Render();

            GL.PopAttrib();

            GL.PopMatrix();
        }

        public void RenderVBO()
        {
            vbo.Render();
        }
    }
}
