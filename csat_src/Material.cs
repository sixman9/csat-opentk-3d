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
using OpenTK.Graphics;
using OpenTK.Math;

/*
 * Ns = Phong SpecularTex component. Ranges from 0 to 200.
 * Kd = Diffuse color weighted by the DiffuseTex coefficient.
 * Ka = Ambient color weighted by the AmbientTex coefficient.
 * Ks = Specular color weighted by the SpecularTex coefficient.
 * d = Dissolve factor (pseudo-transparency). This is set to the internal face transparency value.
 * illum 2 = Diffuse and SpecularTex shading model.
 * map_Kd = Diffuse color texture map.
 * map_Ks = Specular color texture map.
 * map_Ka = Ambient color texture map.
 * map_Bump = Bump texture map.
 * map_d = Opacity texture map.
 */

namespace CSat
{
    public class Material
    {
        /// <summary>
        /// materiaalin nimi
        /// </summary>
        string name;

        static string curMaterial = "";

        /// <summary>
        /// glsl ohjelman nimi. objektille voidaan asettaa ohjelma editoimalla materiaalitiedostoa
        /// ja laittaa haluamaan matskuun  map_Ka Shader_(Shader-tiedoston nimi ilman päätettä)
        /// </summary>
        public string ShaderName = "";

        List<Material> materials = new List<Material>();

        /*
        /// texturet
         */
        public Texture DiffuseTex = null;
        public Texture SpecularTex = null;
        public Texture AmbientTex = null;
        public Texture BumpTex = null;
        public Texture OpacityTex = null;

        /*
        /// väriarvot
         */
        public float PhongSpec = 5; // Ns: 0-200
        public Vector4 DiffuseColor = new Vector4(0.5f, 0.5f, 0.5f, 1); // Diffuse color
        public Vector4 AmbientColor = new Vector4(0.1f, 0.1f, 0.1f, 1); // Ambient color
        public Vector4 SpecularColor = new Vector4(0.5f, 0.5f, 0.5f, 1); // Specular color
        public Vector4 EmissionColor = new Vector4(0.1f, 0.1f, 0.1f, 1);
        public float Dissolve = 1; // transparency

        public Material()
        {
        }
        public Material(string materialName)
        {
            name = materialName;
        }

        public void Dispose()
        {
            if (DiffuseTex != null) DiffuseTex.Dispose();
            if (SpecularTex != null) SpecularTex.Dispose();
            if (AmbientTex != null) AmbientTex.Dispose();
            if (BumpTex != null) BumpTex.Dispose();
            if (OpacityTex != null) OpacityTex.Dispose();
        }

        /// <summary>
        /// lataa materiaalitiedot
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="loadTextures"></param>
        public void Load(string fileName, bool loadTextures)
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(fileName))
            {
                // tiedosto muistiin
                string data = file.ReadToEnd();

                // pilko se
                string[] lines = data.Split('\n');

                Material tmpmat = null;

                for (int q = 0; q < lines.Length; q++)
                {
                    string line = lines[q];
                    line = line.Trim('\r', '\t', ' ');

                    if (line.StartsWith("#")) continue;

                    string[] ln = line.Split(' '); // pilko datat

                    if (ln[0] == "newmtl")
                    {
                        if (tmpmat != null) materials.Add(tmpmat);
                        tmpmat = new Material();
                        tmpmat.name = ln[1];
                        Log.WriteDebugLine("MaterialName: " + tmpmat.name);
                        continue;
                    }


                    // Diffuse color texture map
                    if (ln[0] == "map_Kd")
                    {
                        tmpmat.DiffuseTex = new Texture();
                        if (loadTextures == true) tmpmat.DiffuseTex = Texture.Load(ln[1].ToLower());
                        continue;
                    }
                    // Specular color texture map
                    if (ln[0] == "map_Ks")
                    {
                        tmpmat.SpecularTex = new Texture();
                        if (loadTextures == true) tmpmat.SpecularTex = Texture.Load(ln[1].ToLower());
                        continue;
                    }
                    // Ambient color texture map TAI shaderin nimi
                    if (ln[0] == "map_Ka")
                    {
                        // esim: Shader_cartoon  niin ladataan objektille cartoon.vert ja cartoon.frag shaderit.
                        if (ln[1].Contains("Shader_"))
                        {
                            // ota shaderin nimi
                            tmpmat.ShaderName = ln[1].Substring(7);
                        }
                        else //Ambient color texture
                        {
                            tmpmat.AmbientTex = new Texture();
                            if (loadTextures == true) tmpmat.AmbientTex = Texture.Load(ln[1].ToLower());
                        }
                        continue;
                    }
                    // Bump color texture map
                    if (ln[0] == "map_Bump")
                    {
                        tmpmat.BumpTex = new Texture();
                        if (loadTextures == true) tmpmat.BumpTex = Texture.Load(ln[1].ToLower());
                        continue;
                    }
                    // Opacity color texture map
                    if (ln[0] == "map_d")
                    {
                        tmpmat.OpacityTex = new Texture();
                        if (loadTextures == true) tmpmat.OpacityTex = Texture.Load(ln[1].ToLower());
                        continue;
                    }

                    // Phong SpecularTex component
                    if (ln[0] == "Ns")
                    {
                        tmpmat.PhongSpec = Util.GetFloat(ln[1]);
                        continue;
                    }
                    // Ambient color
                    if (ln[0] == "Ka")
                    {
                        tmpmat.AmbientColor = new Vector4(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]), 1);
                        continue;
                    }

                    // Diffuse color
                    if (ln[0] == "Kd")
                    {
                        tmpmat.DiffuseColor = new Vector4(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]), 1);
                        continue;
                    }
                    // Specular color
                    if (ln[0] == "Ks")
                    {
                        tmpmat.SpecularColor = new Vector4(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]), 1);
                        continue;
                    }

                    // Dissolve factor (pistetty alphaks)
                    if (ln[0] == "d")
                    {
                        // alpha value
                        tmpmat.Dissolve = Util.GetFloat(ln[1]);
                        tmpmat.DiffuseColor.W = tmpmat.Dissolve;
                        tmpmat.AmbientColor.W = tmpmat.Dissolve;
                        tmpmat.SpecularColor.W = tmpmat.Dissolve;
                        continue;
                    }
                }
                if (tmpmat != null) materials.Add(tmpmat);
            }
        }

        /// <summary>
        /// jos materiaalin tietoja muuttaa, ja se on käytössä, tällä saa uudet asetukset käyttöön.
        /// </summary>
        public void ForceSetMaterial()
        {
            curMaterial = "";
            SetMaterial();
        }

        /// <summary>
        /// aseta materiaali ellei jo käytössä
        /// </summary>
        public void SetMaterial()
        {
            if (curMaterial == name) return;
            curMaterial = name;

            if (DiffuseTex != null) DiffuseTex.Bind();

            GL.Materialv(MaterialFace.Front, MaterialParameter.Ambient, AmbientColor);
            GL.Materialv(MaterialFace.Front, MaterialParameter.Diffuse, DiffuseColor);
            GL.Materialv(MaterialFace.Front, MaterialParameter.Specular, SpecularColor);
            GL.Materialv(MaterialFace.Front, MaterialParameter.Emission, EmissionColor);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, PhongSpec);
        }

        public void SetMaterial(string name)
        {
            Material mat = GetMaterial(name);
            mat.SetMaterial();
        }

        public Material GetMaterial(string name)
        {
            if (this.name == name) return this;
            for (int q = 0; q < materials.Count; q++)
            {
                if (materials[q].name == name) return materials[q];
            }
            return null;
        }
    }
}
