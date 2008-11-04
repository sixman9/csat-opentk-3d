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

using System.Collections;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    class SortedList
    {
        public float len = 0;
        public Particle part;
        public SortedList(float l, Particle p)
        {
            len = l; part = p;
        }
    }

    public delegate void ParticleCallback(Particle part);

    public class ParticleEngine
    {
        public static float AlphaMin = 0.1f;
        ArrayList part = new ArrayList();

        public void Add(Particles particles, ParticleCallback particleCallback)
        {
            part.Add(particles);
            particles.callBack = particleCallback;
        }

        public void Render()
        {
            Camera cam = Camera.cam;

            List<SortedList> slist = new List<SortedList>();

            GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, AlphaMin);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusDstAlpha);

            // j‰rjestet‰‰n taulukko kauimmaisesta l‰himp‰‰n. pit‰‰ rendata siin‰ j‰rjestyksess‰.
            // vain l‰pikuultavat pit‰‰ j‰rjest‰‰. l‰pikuultamattomat renderoidaan samantien.
            for (int q = 0; q < part.Count; q++)
            {
                GL.PushMatrix();
                Particles curpar = (Particles)part[q];
                GL.Translate(curpar.Position);

                for (int w = 0; w < curpar.NumOfParticles; w++)
                {
                    Particle p = curpar.GetParticle(w);

                    if (p.isTranslucent == true) // listaan renderoitavaks myˆhemmin
                    {
                        Vector3 rp = p.pos + curpar.Position;
                        float len = (cam.Position - rp).LengthSquared;
                        slist.Add(new SortedList(len, p));
                    }
                    else // rendataan se nyt, ei lis‰t‰ sortattavaks
                    {
                        Billboard.BillboardBegin(p.obj.Texture2D, p.pos.X, p.pos.Y, p.pos.Z, p.size);
                        GL.Color4(p.color);
                        if (p.callBack != null) p.callBack(p);
                        Billboard.BillboardRender(p.obj);
                        Billboard.BillboardEnd();
                    }

                }
                GL.PopMatrix();

            }
            GL.Disable(EnableCap.AlphaTest);

            slist.Sort(delegate(SortedList z1, SortedList z2) { return z2.len.CompareTo(z1.len); });

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            // rendataan l‰pikuultavat
            GL.DepthMask(false); // ei kirjoiteta zbufferiin
            for (int q = 0; q < slist.Count; q++)
            {
                Particle p = slist[q].part;
                GL.Color4(p.color);
                Billboard.BillboardBegin(p.obj.Texture2D, p.pos.X, p.pos.Y, p.pos.Z, p.size);
                if (p.callBack != null) p.callBack(p);
                Billboard.BillboardRender(p.obj);
                Billboard.BillboardEnd();
            }
            GL.DepthMask(true);

            GL.Disable(EnableCap.Blend);
        }

    }

    public struct Particle
    {
        public Object2D obj; // vbo, texture
        public Vector3 pos; // paikka
        public Vector3 dir; // suunta (ja nopeus)
        public Vector3 gravity;  // mihin suuntaan vedet‰‰n
        public float life; // kauanko partikkeli el‰‰
        public float size; // partikkelin koko		
        public bool isTranslucent; // l‰pikuultava (jos on, pit‰‰ sortata)
        public Vector4 color; // v‰ri
        public ParticleCallback callBack;
    }

    public class Particles : ObjectInfo
    {
        public bool IsTranslucent = false;
        Object2D obj = null;
        public ParticleCallback callBack = null;

        ArrayList parts = new ArrayList();

        public Particle GetParticle(int q)
        {
            return (Particle)parts[q];
        }

        public int NumOfParticles
        {
            get { return parts.Count; }
        }

        float size = 10;
        public float Size
        {
            get { return size; }
            set { size = value; }
        }

        public void AddParticle(ref Vector3 pos, ref Vector3 dir, ref Vector3 gravity, float life, float size, Vector4 color)
        {
            Particle p;
            p.pos = pos;
            p.dir = dir;
            p.gravity = gravity;
            p.life = life;
            p.size = size;
            p.obj = obj;
            p.callBack = callBack;
            p.isTranslucent = IsTranslucent;
            p.color = color;
            this.size = size * 0.01f;
            parts.Add(p);
        }

        /// <summary>
        /// aseta partikkeliobjekti.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="IsTranslucent">jos true, partikkelit on l‰pikuultavia (pit‰‰ sortata)</param>
        public void SetObject(Object2D obj, bool isTranslucent)
        {
            this.obj = obj;
            this.IsTranslucent = isTranslucent;
        }

        /// <summary>
        /// p‰ivit‰ partikkelit
        /// </summary>
        /// <param name="time"></param>
        public void Update(float time)
        {
            for (int q = 0; q < parts.Count; q++)
            {
                Particle p = (Particle)parts[q];

                p.life -= time;
                if (p.life < 0) // kuoleeko partikkeli
                {
                    parts.RemoveAt(q); // poista se
                    continue;
                }

                p.pos += p.dir;
                p.dir += p.gravity;

                parts.RemoveAt(q);
                parts.Insert(q, p);
            }
        }

        /// <summary>
        /// piirr‰ partikkelit. ei sortata eik‰ ole callbackia.
        /// </summary>
        public new void Render()
        {
            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);

            GL.DepthMask(false);
            GL.Enable(EnableCap.Texture2D);
            obj.Texture2D.Bind();

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            int i, j;
            float[] modelview = new float[16];

            GL.PushMatrix();
            GL.Translate(Position);

            for (int q = 0; q < parts.Count; q++)
            {
                GL.PushMatrix();
                Particle p = (Particle)parts[q];
                GL.Color4(p.color);
                GL.Translate(p.pos.X, p.pos.Y, p.pos.Z);
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

                Billboard.BillboardRender(obj);
                GL.PopMatrix();
            }
            GL.PopMatrix();
            GL.DepthMask(true);
            GL.PopAttrib();
            GL.Color4(1f, 1, 1, 1);
        }


        /// <summary>
        /// piirr‰ partikkelit. sortataan ja callback.
        /// </summary>
        public void RenderSorted()
        {
            Camera cam = Camera.cam;

            List<SortedList> slist = new List<SortedList>();

            GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, ParticleEngine.AlphaMin);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusDstAlpha);

            // j‰rjestet‰‰n taulukko kauimmaisesta l‰himp‰‰n. pit‰‰ rendata siin‰ j‰rjestyksess‰.
            // vain l‰pikuultavat pit‰‰ j‰rjest‰‰. l‰pikuultamattomat renderoidaan samantien.
            GL.PushMatrix();
            Particles curpar = this;
            GL.Translate(curpar.Position);

            for (int w = 0; w < curpar.NumOfParticles; w++)
            {
                Particle p = curpar.GetParticle(w);

                if (p.isTranslucent == true) // listaan renderoitavaks myˆhemmin
                {
                    Vector3 rp = p.pos + curpar.Position;
                    float len = (cam.Position - rp).LengthSquared;
                    slist.Add(new SortedList(len, p));
                }
                else // rendataan se nyt, ei lis‰t‰ sortattavaks
                {
                    Billboard.BillboardBegin(p.obj.Texture2D, p.pos.X, p.pos.Y, p.pos.Z, p.size);
                    GL.Color4(p.color);
                    if (p.callBack != null) p.callBack(p);
                    Billboard.BillboardRender(p.obj);
                    Billboard.BillboardEnd();
                }

            }
            GL.PopMatrix();

            GL.Disable(EnableCap.AlphaTest);

            slist.Sort(delegate(SortedList z1, SortedList z2) { return z2.len.CompareTo(z1.len); });

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            // rendataan l‰pikuultavat
            GL.DepthMask(false); // ei kirjoiteta zbufferiin
            for (int q = 0; q < slist.Count; q++)
            {
                Particle p = ((SortedList)slist[q]).part;
                GL.Color4(p.color);
                Billboard.BillboardBegin(p.obj.Texture2D, p.pos.X, p.pos.Y, p.pos.Z, p.size);
                if (p.callBack != null) p.callBack(p);
                Billboard.BillboardRender(p.obj);
                Billboard.BillboardEnd();
            }
            GL.DepthMask(true);

            GL.Disable(EnableCap.Blend);
        }

    }
}
