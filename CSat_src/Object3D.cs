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
 * lataa obj tiedostoja
 *
 */
using System;
using System.Collections;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public class Object3D : ObjectInfo, IModel, ICloneable
    {
        public Object3D()
        {
        }

        public Object3D(string fileName)
        {
            Load(fileName, 1, 1, 1);
        }

        public Object3D(string fileName, float xs, float ys, float zs)
        {
            Load(fileName, xs, ys, zs);
        }

        Object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <summary>
        /// Palauttaa objektin kloonin.
        /// </summary>
        /// <returns></returns>
        public Object3D Clone()
        {
            Object3D clone = (Object3D)this.MemberwiseClone();

            // eri grouppi eli kloonatut objektit voi lis‰ill‰ grouppiin mit‰ tahtoo
            // sen vaikuttamatta alkuper‰iseen.
            clone.objects = new ArrayList(Objects);
            clone.MaterialName = MaterialName;

            for (int q = 0; q < Objects.Count; q++)
            {
                Object ob = Objects[q];

                Object3D child = (Object3D)Objects[q];
                clone.Objects[q] = child.Clone();
            }
            return clone;
        }

        BoundingVolume objBoundingVolume = new BoundingVolume();
        public BoundingVolume GetBoundingVolume()
        {
            return objBoundingVolume;
        }

        Vector3[] vertex;
        Vector3[] normal;
        Vector2[] uv;

        public Vector3[] Vertex
        {
            get { return vertex; }
        }

        /// <summary>
        /// ladataanko texturet
        /// </summary>
        public static bool Textured = true;

        VBO vbo;

        /// <summary>
        /// tuhoa objekti
        /// </summary>
        public void Dispose()
        {
            for (int q = 0; q < Objects.Count; q++)
            {
                Object3D child = (Object3D)Objects[q];
                Material.Dispose(child.MaterialName);
                child.vbo.Dispose();
            }
        }

        /// <summary>
        /// lataa .obj/.obj2 tiedosto
        /// </summary>
        /// <param name="fileName"></param>
        public void Load(string fileName)
        {
            Load(fileName, 1, 1, 1);
        }

        /// <summary>
        /// lataa .obj/.obj2 tiedosto
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="xs"></param>
        /// <param name="ys"></param>
        /// <param name="zs"></param>
        public void Load(string fileName, float xs, float ys, float zs)
        {
            Object3D parent = this;

            Object3D child = null;

            string dir = Settings.DataDir;
            if (fileName.Contains("\\"))
            {
                int l = fileName.LastIndexOf("\\");
                dir = dir + fileName.Substring(0, l + 1);
            }
            else if (fileName.Contains("/"))
            {
                int l = fileName.LastIndexOf("/");
                dir = dir + fileName.Substring(0, l + 1);
            }

            parent.name = fileName; // objektin nimeksi sama kuin tiedostonimi
            fileName = Settings.DataDir + fileName;
            bool path = false; // jos reitti

            // tiedosto muistiin
            string data = new System.IO.StreamReader(fileName).ReadToEnd();
            {
                data = data.Replace('\r', ' ');

                // pilko se
                string[] lines = data.Split('\n');

                int numOfVerts = 0, numOfUV = 0, numOfMeshes = 0, numOfNormals = 0; // lukum‰‰r‰t
                int cvert = 0, cuv = 0, cnorm = 0; // countterit

                // k‰sitell‰‰n rivi kerrallaan, ensin laskemalla tarvittavat,
                // eli montako meshi‰, vertex lkm, uv lkm, normal lkm
                for (int q = 0; q < lines.Length; q++)
                {
                    string[] ln = lines[q].Split(' '); // pilko datat

                    if (ln[0] == "o" || ln[0] == "g" || (ln[0] == "usemtl" && lines[q - 1].StartsWith("f"))) numOfMeshes++;
                    if (ln[0] == "v") numOfVerts++;
                    if (ln[0] == "vt") numOfUV++;
                    if (ln[0] == "vn") numOfNormals++;
                }
                // varataan tilaa
                parent.vertex = new Vector3[numOfVerts];
                parent.normal = new Vector3[numOfNormals];
                parent.uv = new Vector2[numOfUV];

                // lue kaikki datat objektiin ja indexit mesheihin
                for (int q = 0; q < lines.Length; q++)
                {
                    string line = lines[q];
                    if (line.StartsWith("#")) continue;
                    string[] ln = line.Split(' '); // pilko datat
                    if (ln[0] == "v") // vertex x y z
                    {
                        float x = (Util.GetFloat(ln[1]) - child.Position.X) * xs;
                        float y = (Util.GetFloat(ln[2]) - child.Position.Y) * ys;
                        float z = (Util.GetFloat(ln[3]) - child.Position.Z) * zs;
                        parent.vertex[cvert++] = new Vector3(x, y, z);
                        continue;
                    }

                    if (ln[0] == "vn") // normal x y z
                    {
                        parent.normal[cnorm++] = new Vector3(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]));
                        continue;
                    }

                    if (ln[0] == "vt") // texcoord u v
                    {
                        parent.uv[cuv++] = new Vector2(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]));
                        continue;
                    }

                    // uusi objekti
                    if (ln[0] == "o" || ln[0] == "g")
                    {
                        if (child != null) parent.Add(child); // talteen

                        child = new Object3D();
                        child.name = ln[1];

                        // Nimess‰ voi olla ohjeita mit‰ muuta halutaan, esim:
                        // * Path_reitti1  jolloin ei ladata objektia mutta reitti jota pitkin kamera/objektit voi kulkea.
                        // * BBox_nimi/BSphere_nimi jolloin t‰m‰ onkin nimi-objektin bounding box/sphere.
                        if (child.name.Contains("Path_"))
                        {
                            path = true;
                        }
                        else if (child.name.Contains("BBox_") || child.name.Contains("BSphere_"))
                        {
                            // TODO
                        }

                        // child osoittamaan parentissa oleviin vertexeihin, uvihin ja normaleihin
                        child.vertex = parent.vertex;
                        child.normal = parent.normal;
                        child.uv = parent.uv;

                        // seuraavalla rivill‰ on #POS jos k‰ytetty obj2 exportteria
                        if (lines[q + 1].Contains("#POS"))
                        {
                            // otetaan meshin paikka talteen
                            string[] spos = lines[q + 1].Split(' ');
                            child.Position = new Vector3(Util.GetFloat(spos[1]), Util.GetFloat(spos[2]), Util.GetFloat(spos[3]));
                            Log.WriteDebugLine(child.name + " POS: " + child.Position.ToString(), 2);
                        }

                        continue;
                    }

                    // materiaali
                    if (ln[0] == "usemtl")
                    {
                        // jos kesken meshin materiaali vaihtuu, luodaan uusi obj johon loput facet
                        if (lines[q - 1].StartsWith("f"))
                        {
                            parent.Add(child);
                            Vector3 tmpPos = child.Position;

                            child = new Object3D();
                            child.Position = tmpPos; // samaa objektia, niin sama Position

                            // child osoittamaan parentissa oleviin vertexeihin, uvihin ja normaleihin
                            child.vertex = parent.vertex;
                            child.normal = parent.normal;
                            child.uv = parent.uv;
                        }

                        child.MaterialName = ln[1];
                        continue;
                    }

                    if (ln[0] == "f")
                    {
                        child.AddFace(line);
                        continue;
                    }

                    // materiaalitiedosto
                    if (ln[0] == "mtllib")
                    {
                        try
                        {
                            Material.Load(dir + ln[1], Textured);
                        }
                        catch (Exception e)
                        {
                            Log.WriteDebugLine(e.ToString());
                        }
                    }
                }
            }

            if (child != null) parent.Add(child);

            // pathille ei luoda objektia, se on vain kasa vertexej‰
            if (path == false)
            {
                // luo joka objektille vbo ja kopsaa datat sinne
                for (int q = 0; q < parent.Objects.Count; q++)
                {
                    child = (Object3D)parent.Objects[q];
                    child.vbo = new VBO();
                    child.vbo.DataToVBO(parent.vertex, parent.normal, parent.uv, null, null, ref child);

                    child.MeshBoundingVolume.CalcMeshBounds(child);

                    // lataa glsl koodit
                    string shader = Material.GetMaterial(child.MaterialName).shaderName;
                    if (shader != "")
                    {
                        child.Shader = new GLSL();
                        child.Shader.Load(shader + ".vert", shader + ".frag");
                        child.UseShader = true;
                    }

                    if (Material.GetMaterial(child.MaterialName).dissolve < 1.0f) IsTranslucent = true;

                }
                CalcBoundingVolumes();
                Log.WriteDebugLine("Object: " + parent.name + "  meshes: " + parent.Objects.Count, 1);

            }
            else Log.WriteDebugLine("Path: " + parent.name, 1);

        }

        /// <summary>
        /// renderoi objekti
        /// </summary>
        public new void Render()
        {
            visibleObjects.Clear();
            translucentObjects.Clear();

            GL.PushMatrix(); // kameramatrix talteen. seuraavat laskut frustum cullingia varten
            GL.LoadIdentity();
            CalculateWorldCoords(this);
            GL.PopMatrix(); // kameraan takas

            CalculateCoords(this);

            RenderArrays();
        }

        /// <summary>
        /// pelkk‰ renderointi.
        /// </summary>
        public void RenderFast()
        {
            if (vbo != null)
            {
                Material.SetMaterial(MaterialName);
                if (Shader != null && UseShader) Shader.UseProgram();
                if (DoubleSided) GL.Disable(EnableCap.CullFace);

                vbo.BeginRender();
                vbo.Render();
                vbo.EndRender();

                if (DoubleSided) GL.Enable(EnableCap.CullFace);
                if (Shader != null && UseShader) Shader.RemoveProgram();
                Settings.NumOfObjects++;
            }
        }

        /// <summary>
        /// hae objekti
        /// </summary>
        /// <param name="index"></param>
        public Object3D GetObject(int index)
        {
            return (Object3D)Objects[index];
        }

        /// <summary>
        /// ota haluttu objekti nimen perusteella.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Object3D GetObject(string name)
        {
            for (int q = 0; q < Objects.Count; q++)
            {
                Object3D o = (Object3D)Objects[q];
                if (o.name == name) return (Object3D)Objects[q];
            }
            return null;
        }

        /// <summary>
        /// voi erikseen valita mit‰ texture unitteja k‰ytet‰‰n jos multitexture
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public void UseTextureUnits(bool t0, bool t1, bool t2)
        {
            for (int q = 0; q < Objects.Count; q++)
            {
                Object3D child = (Object3D)Objects[q];
                if (Textured) child.vbo.UseTextureUnits(t0, t1, t2);
                else child.vbo.UseTextureUnits(false, false, false);
            }
        }

        /// <summary>
        /// laske objektin ja childien viem‰ alue
        /// </summary>
        public void CalcBoundingVolumes()
        {
            GetBoundingVolume().CalcBounds(this);
        }


        /// <summary>
        /// valitse testausmoodi: BoundingVolume.None, BoundingVolume.Box, BoundingVolume.Sphere
        /// </summary>
        /// <param name="mode"></param>
        public void SetBoundingMode(byte mode)
        {
            for (int q = 0; q < Objects.Count; q++)
            {
                Object3D child = (Object3D)Objects[q];
                child.MeshBoundingVolume.Mode = mode;
            }
        }


        /// <summary>
        /// Asettaa meshien 2 puolisuuden, eli ei cullata polyja.
        /// </summary>
        /// <param name="meshNumber">halutun meshin nimi, tai * niin kaikki</param>
        /// <param name="DoubleSided">true/false asetetaanko/poistetaanko 2 puolisuus </param>
        public void SetDoubleSided(string name, bool doubleSided)
        {
            if (name == "*") // muuta kaikki 2 puoliseks
            {
                for (int q = 0; q < Objects.Count; q++)
                {
                    Object3D child = (Object3D)Objects[q];
                    child.DoubleSided = doubleSided;
                }
            }
            else
            {
                Object3D child = GetObject(name);
                if (child == null)
                {
                    throw new Exception("Object " + name + " not found.");
                }
                child.DoubleSided = doubleSided;
            }
        }

        /// <summary>
        /// lataa shaderit.
        /// jos meshnamessa on * merkki, ladataan shaderi kaikkiin mesheihin
        /// joissa on fileName nimess‰, eli esim  box*  lataa box1, box2, jne mesheihin shaderin.
        /// </summary>
        /// <param name="meshName"></param>
        /// <param name="vertexShader"></param>
        /// <param name="fragmentShader"></param>
        public void LoadShader(string meshName, string vertexShader, string fragmentShader)
        {
            for (int q = 0; q < Objects.Count; q++)
            {
                Object3D child = (Object3D)Objects[q];

                if (meshName.Contains("*"))
                {
                    meshName = meshName.Trim('*');
                    if (child.name.Contains(meshName))
                    {
                        child.Shader = new GLSL();
                        child.Shader.Load(vertexShader, fragmentShader);
                        child.UseShader = true;
                    }
                }
                else if (child.name.Equals(meshName))
                {
                    child.Shader = new GLSL();
                    child.Shader.Load(vertexShader, fragmentShader);
                    child.UseShader = true;
                }
            }
        }

        /// <summary>
        /// lataa shaderit ja k‰yt‰ koko objektissa.
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
            for (int q = 0; q < Objects.Count; q++)
            {
                Object3D child = (Object3D)Objects[q];
                if (use == true)
                {
                    child.Shader = new GLSL();
                    child.Shader.Load(vertexShader, fragmentShader);
                }
                child.UseShader = use;
            }
        }

        // mesh kohtaset

        public int GetNumOfTriangles()
        {
            return VertexInd.Count / 3;
        }
        public void GetTriangle(int triNum, ref Vector3[] tri)
        {
            triNum *= 3;
            tri[0] = vertex[(int)VertexInd[triNum]];
            tri[1] = vertex[(int)VertexInd[triNum + 1]];
            tri[2] = vertex[(int)VertexInd[triNum + 2]];
        }

        /// <summary>
        /// onko l‰pikuultava
        /// </summary>
        public bool IsTranslucent = false;

        public string MaterialName = "";

        public BoundingVolume MeshBoundingVolume = new BoundingVolume();

        public ArrayList VertexInd = new ArrayList(), NormalInd = new ArrayList(), UvInd = new ArrayList();

        public void AddFace(string line)
        {
            // ota talteen  f rivi:
            // f vertex/uv/normal vertex/uv/normal vertex/uv/normal
            // eli esim: f 4/4/2 5/5/3 7/2/4  
            // tarkistetaan jos ilman texcoordei eli f 4/4 5/4 6/4  tai f 2//3 jne tai ilman / merkkej‰.
            int dv = 0;
            for (int t = 0; t < line.Length; t++) if (line[t] == '/') dv++;
            if (line.Contains("//")) dv = 0; // ei ole texindexei

            line = line.Replace("//", " ");
            line = line.Replace("/", " ");
            string[] ln = line.Split(' ');

            if (dv == 0 || dv == 3) // luultavasti nyt ei ole texturecoordinaatteja
            {
                VertexInd.Add(Int32.Parse(ln[1]) - 1);
                VertexInd.Add(Int32.Parse(ln[3]) - 1);
                VertexInd.Add(Int32.Parse(ln[5]) - 1);

                NormalInd.Add(Int32.Parse(ln[2]) - 1);
                NormalInd.Add(Int32.Parse(ln[4]) - 1);
                NormalInd.Add(Int32.Parse(ln[6]) - 1);
            }
            else // kaik mukana
            {
                VertexInd.Add(Int32.Parse(ln[1]) - 1);
                VertexInd.Add(Int32.Parse(ln[4]) - 1);
                VertexInd.Add(Int32.Parse(ln[7]) - 1);

                UvInd.Add(Int32.Parse(ln[2]) - 1);
                UvInd.Add(Int32.Parse(ln[5]) - 1);
                UvInd.Add(Int32.Parse(ln[8]) - 1);

                NormalInd.Add(Int32.Parse(ln[3]) - 1);
                NormalInd.Add(Int32.Parse(ln[6]) - 1);
                NormalInd.Add(Int32.Parse(ln[9]) - 1);
            }
        }

    }
}
