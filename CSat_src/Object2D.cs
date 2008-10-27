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
            Load(textureFileName, null);
        }

        protected Texture tex2d = new Texture();
        public Texture Tex2D // jos halutaan texturetiedot 2d-objektista esim bindausta varten
        {
            get { return tex2d; }
        }

        /** structi joka lˆytyy object3d.cs:st‰. pit‰‰ ylh‰‰ll‰ tiedot aloituskohdasta
         * ja pituudesta.
         */
        protected Vbo o2d;
        public Vbo O2D
        {
            get { return o2d; }
        }

        void Bind()
        {
            tex2d.Bind();
        }

        /**
         * lataa kuva.
         * 
         * jos vbo==null, varataan sen verran kun tarvis, muuten lis‰t‰‰n
         * valmiiksi varattuun vbo:hon (varattu AllocVBO:lla)
         * 
         */
        public void Load(string fileName, VBO vbo)
        {
            name = fileName;

            tex2d = Texture.Load(fileName);

            int[] ind = new int[] { 0, 1, 3, 1, 2, 3 };

            int w = tex2d.Width / 2;
            int h = tex2d.Height / 2;

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

            // ei aikaisemmin varattua aluetta, varataan t‰ss‰
            if (vbo == null)
            {
                o2d.vbo = new VBO();
                vbo = o2d.vbo;
                vbo.AllocVBO(4, 6, BufferUsageHint.StaticDraw);
            }
            else o2d.vbo = vbo;

            o2d.vi = vbo.LoadVBO(vs, ind, null, uv, null, null, null);

            // scale
            view.X = 1;
            view.Y = 1;

        }

        public void Dispose()
        {
            tex2d.Dispose();
        }

        /** 
         * aseta haluttu kohta, haluttu asento ja koko (sx==1 sy==1 niin normaali koko)
         * x,y on kuvan keskikohta
         */
        public void Set(int x, int y, float rotate, float sx, float sy)
        {
            position.X = x;
            position.Y = y;
            rotation.Z = rotate;
            view.X = sx;
            view.Y = sy;
        }
        public void Set(float x, float y, float z, float rx, float ry, float rz, float sx, float sy)
        {
            position.X = x;
            position.Y = y;
            position.Z = z;
            rotation.X = rx;
            rotation.Y = ry;
            rotation.Z = rz;
            view.X = sx;
            view.Y = sy;
        }

        /**
         * aseta blend 
         */
        public static void SetBlend()
        {
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        // voi erikseen valita mit‰ texture unittei k‰ytet‰‰n jos multitexture
        public void UseTextureUnits(bool t0, bool t1, bool t2)
        {
            o2d.vbo.UseTextureUnits(t0, t1, t2);
        }

        public void Render3D()
        {
            Render3D(position.X, position.Y, position.Z,
                rotation.X, rotation.Y, rotation.Z,
                view.X, view.Y);
        }

        // renderoi 2d tason 3d maailmaan
        public void Render3D(float x, float y, float z, float rx, float ry, float rz, float sx, float sy)
        {
            Render3D(x, y, z, rx, ry, rz, sx, sy, null);
        }

        // renderoi 2d tason 3d maailmaan
        public void Render3D(float x, float y, float z, float rx, float ry, float rz, float sx, float sy, VBO vbo)
        {
            GL.PushMatrix();

            GL.Disable(EnableCap.CullFace);

            GL.Translate(x, y, z);
            GL.Rotate(rx, 1, 0, 0);
            GL.Rotate(ry, 0, 1, 0);
            GL.Rotate(rz, 0, 0, 1);
            GL.Scale(sx, sy, 1);

            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            SetBlend();

            tex2d.Bind();
            if (vbo == null) o2d.vbo.Render(); else vbo.Render(o2d.vi);
            GL.PopAttrib();

            // renderoidaan myˆs kaikki childit
            for (int c = 0; c < objects.Count; c++)
            {
                Group g = (Group)objects[c];
                g.Render();
            }

            GL.PopMatrix();
        }

        /**
         * piirr‰ kuva 
         */
        public void Render2D()
        {
            Render2D(null);
        }

        public void Render2D(int x, int y, float rotate, float sx, float sy, VBO vbo)
        {
            Set(x, y, rotate, sx, sy);
            Render2D(vbo);
        }
        public void Render2D(int x, int y, float rotate, float sx, float sy)
        {
            Set(x, y, rotate, sx, sy);
            Render2D(null);
        }


        /**
         * piirr‰ kuvajoukko vbo:sta.
         * kutsuvassa metodissa k‰yd‰‰n kuvajoukko l‰pi
         *   for(q=0; q<pics.Length; q++) texture[q].Render(pictures);
         * jossa pictures on vbo
         */
        public void Render2D(VBO vbo)
        {
            GL.PushMatrix();

            GL.Translate(position.X, Util.ScreeenHeight-position.Y, 0);
            GL.Rotate(rotation.Z, 0, 0, 1);
            GL.Scale(view.X, view.Y, 1);

            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            SetBlend();
            tex2d.Bind();
            if (vbo == null) o2d.vbo.Render(); else vbo.Render(o2d.vi);
            GL.PopAttrib();
            GL.PopMatrix();
        }


    }
}
