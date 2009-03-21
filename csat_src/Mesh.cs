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

using System.Collections.Generic;
using System;
using System.Collections;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public class Mesh : Node, ICloneable
    {
        protected Vertex[] vertices;
        public Vertex[] Vertices
        {
            get
            {
                return vertices;
            }
        }
        /// <summary>
        /// tämä on 0,1,2,3,4,..
        /// </summary>
        protected int[] indices;

        /// <summary>
        /// onko rendattava objekti. pelkkä node (paikkatieto) ei ole jolloin tämä false
        /// </summary>
        public bool IsRendObj = true;
        public bool LookAtNextPoint = false;

        public BoundingVolume Boundings;
        public bool DoubleSided = false;
        public bool IsTranslucent = false;
        public GLSL Shader = null;
        public string MaterialName;
        protected Material material;
        public Material GetMaterial()
        {
            return material;
        }
        public Material GetMaterial(string name)
        {
            return material.GetMaterial(name);
        }
        public virtual List<ObjModel> Meshes()
        {
            return null;
        }

        public virtual void SetDoubleSided(string name, bool doublesided)
        {
        }

        /// <summary>
        /// lataa animaation.
        /// </summary>
        /// <param name="animName">animaation tunniste esim walk</param>
        /// <param name="fileName">animaatiotiedoston nimi</param>
        public virtual void LoadAnim(string animName, string fileName)
        {
        }
        /// <summary>
        /// päivitä animaatiota time:llä
        /// </summary>
        /// <param name="time"></param>
        public virtual void Update(float time)
        {
        }

        /// <summary>
        /// aseta haluttu animaatio
        /// </summary>
        /// <param name="animName">animaation tunniste, jota käytetty latauksen yhdeydessä</param>
        public virtual void UseAnimation(string animName)
        {
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <summary>
        /// Palauttaa objektin kloonin.
        /// </summary>
        /// <returns></returns>
        public Mesh Clone()
        {
            Mesh clone = (Mesh)this.MemberwiseClone();

            // eri grouppi eli kloonattuihin objekteihin voi lisäillä muita objekteja
            // sen vaikuttamatta alkuperäiseen.
            clone.objects = new List<Node>(objects);
            clone.MaterialName = MaterialName;

            CloneTree(clone);

            return clone;
        }

        void CloneTree(Mesh clone)
        {
            for (int q = 0; q < objects.Count; q++)
            {
                Mesh child = (Mesh)objects[q];
                clone.objects[q] = child.Clone();

                if (child.objects.Count > 0) child.CloneTree(child);
            }
        }

        /// <summary>
        /// lataa shaderit.
        /// jos meshnamessa on * merkki, ladataan shaderi kaikkiin mesheihin
        /// joissa on fileName nimessä, eli esim  box*  lataa box1, box2, jne mesheihin shaderin.
        /// </summary>
        /// <param name="meshName"></param>
        /// <param name="vertexShader"></param>
        /// <param name="fragmentShader"></param>
        public void LoadShader(string meshName, string vertexShader, string fragmentShader)
        {
            for (int q = 0; q < objects.Count; q++)
            {
                Mesh child = (Mesh)objects[q];

                if (meshName.Contains("*"))
                {
                    meshName = meshName.Trim('*');
                    if (child.Name.Contains(meshName))
                    {
                        child.Shader = new GLSL();
                        child.Shader.Load(vertexShader, fragmentShader);
                    }
                }
                else if (child.Name.Equals(meshName))
                {
                    child.Shader = new GLSL();
                    child.Shader.Load(vertexShader, fragmentShader);
                }
            }
        }

        /// <summary>
        /// lataa shaderit ja käytä koko objektissa.
        /// </summary>
        /// <param name="vertexShader"></param>
        /// <param name="fragmentShader"></param>
        public void LoadShader(string vertexShader, string fragmentShader)
        {
            bool use = true;
            if (vertexShader == "" && fragmentShader == "")
            {
                use = false;
            }

            for (int q = 0; q < objects.Count; q++)
            {
                Mesh child = (Mesh)objects[q];

                if (use == true)
                {
                    child.Shader = new GLSL();
                    child.Shader.Load(vertexShader, fragmentShader);
                }
                else
                {
                    child.Shader = null;
                }
            }
        }
    }
}
