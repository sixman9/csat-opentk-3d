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
using OpenTK.Graphics;
using OpenTK.Math;

/*
 * Ns = Phong specularTex component. Ranges from 0 to 200.
 * Kd = Diffuse color weighted by the diffuseTex coefficient.
 * Ka = Ambient color weighted by the ambientTex coefficient.
 * Ks = Specular color weighted by the specularTex coefficient.
 * d = Dissolve factor (pseudo-transparency). This is set to the internal face transparency value.
 * illum 2 = Diffuse and specularTex shading model.
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
        public Material()
        {
            if (!materials.Contains("defaultMaterial")) materials.Add("defaultMaterial", this);
        }

        /// <summary>
        /// materiaali taulukko. tänne tulee kaikki materiaalit. jokaisella materiaalilla pitää olla eri nimi.
        /// </summary>
        static Hashtable materials = new Hashtable();

        /// <summary>
        /// materiaalin nimi
        /// </summary>
        public string name = null;

        static string curMaterial = "";

        /// <summary>
        /// glsl ohjelman nimi. objektille voidaan asettaa ohjelma editoimalla materiaalitiedostoa
        /// ja laittaa haluamaan matskuun  map_Ka Shader_(Shader-tiedoston nimi ilman päätettä)
        /// </summary>
        public string shaderName = "";

        /*
        /// texturet
        */
        public Texture diffuseTex = null;
        public Texture specularTex = null;
        public Texture ambientTex = null;
        public Texture bumpTex = null;
        public Texture opacityTex = null;

        /*
        /// väriarvot
        */
        public float phongSpec = 5; // Ns: 0-200
        public Vector4 diffuseColor = new Vector4(0.5f, 0.5f, 0.5f, 1); // diffuse color
        public Vector4 ambientColor = new Vector4(0.1f, 0.1f, 0.1f, 1); // ambient color
        public Vector4 specularColor = new Vector4(0.5f, 0.5f, 0.5f, 1); // specular color
        public Vector4 emissionColor = new Vector4(0.1f, 0.1f, 0.1f, 1);
        public float dissolve = 1; // transparency

        public void Dispose()
        {
            if (diffuseTex != null) diffuseTex.Dispose();
            if (specularTex != null) specularTex.Dispose();
            if (ambientTex != null) ambientTex.Dispose();
            if (bumpTex != null) bumpTex.Dispose();
            if (opacityTex != null) opacityTex.Dispose();
        }

        public static Material GetMaterial(string name)
        {
            return (Material)materials[name];
        }

        public static void Dispose(string name)
        {
            Material mi = GetMaterial(name);
            if (mi != null)
            {
                mi.Dispose();
                materials.Remove(name);
            }
        }
        public static void DisposeAll()
        {
            IDictionaryEnumerator en = materials.GetEnumerator();
            while (en.MoveNext())
            {
                Material m = (Material)en.Value;
                m.Dispose();
            }

            materials.Clear();
        }

        /// <summary>
        /// lataa materiaalitiedot
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="loadTextures"></param>
        public static void Load(string fileName, bool loadTextures)
        {
            // tiedosto muistiin
            string data = new System.IO.StreamReader(fileName).ReadToEnd();

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
                    if (tmpmat != null && materials.Contains(tmpmat.name) == false) materials.Add(tmpmat.name, tmpmat);
                    tmpmat = new Material();
                    tmpmat.name = ln[1];
                    Log.WriteDebugLine("MaterialName: " + tmpmat.name, 2);
                    continue;
                }


                // Diffuse color texture map
                if (ln[0] == "map_Kd")
                {
                    tmpmat.diffuseTex = new Texture();
                    if (loadTextures == true) tmpmat.diffuseTex = Texture.Load(ln[1].ToLower());
                    continue;
                }
                // Specular color texture map
                if (ln[0] == "map_Ks")
                {
                    tmpmat.specularTex = new Texture();
                    if (loadTextures == true) tmpmat.specularTex = Texture.Load(ln[1].ToLower());
                    continue;
                }
                // Ambient color texture map TAI shaderin nimi
                if (ln[0] == "map_Ka")
                {
                    // esim: Shader_cartoon  niin ladataan objektille cartoon.vert ja cartoon.frag shaderit.
                    if (ln[1].Contains("Shader_"))
                    {
                        // ota shaderin nimi
                        tmpmat.shaderName = ln[1].Substring(7);
                    }
                    else //Ambient color texture
                    {
                        tmpmat.ambientTex = new Texture();
                        if (loadTextures == true) tmpmat.ambientTex = Texture.Load(ln[1].ToLower());
                    }
                    continue;
                }
                // Bump color texture map
                if (ln[0] == "map_Bump")
                {
                    tmpmat.bumpTex = new Texture();
                    if (loadTextures == true) tmpmat.bumpTex = Texture.Load(ln[1].ToLower());
                    continue;
                }
                // Opacity color texture map
                if (ln[0] == "map_d")
                {
                    tmpmat.opacityTex = new Texture();
                    if (loadTextures == true) tmpmat.opacityTex = Texture.Load(ln[1].ToLower());
                    continue;
                }

                // Phong specularTex component
                if (ln[0] == "Ns")
                {
                    tmpmat.phongSpec = Util.GetFloat(ln[1]);
                    continue;
                }
                // Ambient color
                if (ln[0] == "Ka")
                {
                    tmpmat.ambientColor = new Vector4(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]), 1);
                    continue;
                }

                // Diffuse color
                if (ln[0] == "Kd")
                {
                    tmpmat.diffuseColor = new Vector4(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]), 1);
                    continue;
                }
                // Specular color
                if (ln[0] == "Ks")
                {
                    tmpmat.specularColor = new Vector4(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]), 1);
                    continue;
                }

                // Dissolve factor (pistetty alphaks)
                if (ln[0] == "d")
                {
                    // alpha value
                    tmpmat.dissolve = Util.GetFloat(ln[1]);
                    tmpmat.diffuseColor.W = tmpmat.dissolve;
                    tmpmat.ambientColor.W = tmpmat.dissolve;
                    tmpmat.specularColor.W = tmpmat.dissolve;
                    continue;
                }
            }
            if (tmpmat != null && materials.Contains(tmpmat.name) == false) materials.Add(tmpmat.name, tmpmat);
        }

        /// <summary>
        /// jos vaikka materiaalin tietoja muuttaa, ja se on käytössä, tällä saa uudet asetukset käyttöön.
        /// </summary>
        /// <param name="name"></param>
        public static void ForceSetMaterial(string name)
        {
            curMaterial = "";
            SetMaterial(name);
        }

        /// <summary>
        /// aseta name-niminen materiaali ellei jo käytössä
        /// </summary>
        /// <param name="name"></param>
        public static void SetMaterial(string name)
        {
            if (name == null) return;

            Material mat = (Material)materials[name];
            if (mat == null) return;
            if (mat.diffuseTex != null)
            {
                mat.diffuseTex.Bind();
            }

            if (curMaterial == name) return;

            curMaterial = name;

            GL.Materialv(MaterialFace.Front, MaterialParameter.Ambient, mat.ambientColor);
            GL.Materialv(MaterialFace.Front, MaterialParameter.Diffuse, mat.diffuseColor);
            GL.Materialv(MaterialFace.Front, MaterialParameter.Specular, mat.specularColor);
            GL.Materialv(MaterialFace.Front, MaterialParameter.Emission, mat.emissionColor);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, mat.phongSpec);
        }
    }
}
