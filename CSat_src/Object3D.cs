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
    public class Object3D : ObjectInfo, ICloneable
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

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public Object3D Clone()
        {
            Object3D o = (Object3D)this.MemberwiseClone();
            o.objects = new ArrayList(objects);
            return o;
        }

        public MeshData mesh = new MeshData();

        Vector3[] vertex;
        Vector3[] normal;
        Vector2[] uv;

        public Vector3[] Vertex
        {
            get { return vertex; }
        }

        // ladataanko texturet
        public static bool Textured = true;

        VBO vbo;

        public BoundingVolume objectGroupBoundingVolume = null; // objektin alue

        public void Dispose()
        {
            for (int q = 0; q < objects.Count; q++)
            {
                Object3D child = (Object3D)objects[q];
                Material.Dispose(child.mesh.materialName);
                child.vbo.Dispose();
            }
        }

        /// <summary>
        /// lataa .obj tiedosto
        /// </summary>
        /// <param name="fileName"></param>
        public void Load(string fileName)
        {
            Load(fileName, 1, 1, 1);
        }

        /// <summary>
        /// lataa .obj tiedosto
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
                        float x = (Util.GetFloat(ln[1]) - child.position.X) * xs;
                        float y = (Util.GetFloat(ln[2]) - child.position.Y) * ys;
                        float z = (Util.GetFloat(ln[3]) - child.position.Z) * zs;
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
                        // child osoittamaan parentissa oleviin vertexeihin, uvihin ja normaleihin
                        child.vertex = parent.vertex;
                        child.normal = parent.normal;
                        child.uv = parent.uv;

                        // seuraavalla rivill‰ on #POS jos k‰ytetty obj2 exportteria
                        if (lines[q + 1].Contains("#POS"))
                        {
                            // otetaan meshin paikka talteen
                            string[] spos = lines[q + 1].Split(' ');
                            child.position = new Vector3(Util.GetFloat(spos[1]), Util.GetFloat(spos[2]), Util.GetFloat(spos[3]));
                            Log.WriteDebugLine(child.name + " POS: " + child.position.ToString());
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
                            Vector3 tmpPos = child.position;

                            child = new Object3D();
                            child.position = tmpPos; // samaa objektia, niin sama position

                            // child osoittamaan parentissa oleviin vertexeihin, uvihin ja normaleihin
                            child.vertex = parent.vertex;
                            child.normal = parent.normal;
                            child.uv = parent.uv;
                        }

                        child.mesh.materialName = ln[1];
                        continue;
                    }

                    if (ln[0] == "f")
                    {
                        child.mesh.AddFace(line);
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

            // luo joka objektille vbo ja kopsaa datat sinne
            for (int q = 0; q < parent.objects.Count; q++)
            {
                child = (Object3D)parent.objects[q];
                child.vbo = new VBO();
                child.vbo.DataToVBO(parent.vertex, parent.normal, parent.uv, null, null, null, ref child.mesh);

                child.mesh.boundingVolume.CalcMeshBounds(child);
            }

            // objektiryhm‰n bbox/sphere
            parent.objectGroupBoundingVolume = new BoundingVolume();
            parent.objectGroupBoundingVolume.CalcObjectBounds(parent);

            Log.WriteDebugLine("Object " + parent.name + "  meshes: " + parent.objects.Count);

        }

        /// <summary>
        /// renderoi objekti
        /// </summary>
        public new void Render()
        {
            GL.PushMatrix();

            // liikuta haluttuun kohtaan
            GL.Translate(position.X, position.Y, position.Z);
            GL.Rotate(rotation.X, 1, 0, 0);
            GL.Rotate(rotation.Y, 0, 1, 0);
            GL.Rotate(rotation.Z, 0, 0, 1);

            // korjaa asento
            GL.Rotate(fixRotation.X, 1, 0, 0);
            GL.Rotate(fixRotation.Y, 0, 1, 0);
            GL.Rotate(fixRotation.Z, 0, 0, 1);

            // jos objektia k‰‰nnetty
            Matrix4 rotationMatrix = new Matrix4();
            Vector3 rot = -(rotation + fixRotation);
            if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
            {
                rot = rot * MathExt.PiOver180;
                Matrix4 mx = Matrix4.RotateX(rot.X);
                Matrix4 my = Matrix4.RotateY(rot.Y);
                Matrix4 mz = Matrix4.RotateZ(rot.Z);
                Matrix4 outm0;
                Matrix4.Mult(ref mx, ref my, out outm0);
                Matrix4.Mult(ref outm0, ref mz, out rotationMatrix);
            }
            
            // childin child ... renderoidaan myˆs kaikki childit
            base.RenderTree();

            // jos lˆytyy rendattavaa
            if (vbo != null)
            {
                vbo.BeginRender();

                // tarkista onko objekti n‰kˆkent‰ss‰
                //if (Frustum.ObjectInFrustum(wpos.X, wpos.Y, wpos.Z, objectGroupBoundingVolume))
                {
                    // jos objektia k‰‰nnetty, pit‰‰ laskea mesheille uudet keskipisteet
                    Vector3 vout, center = mesh.center;
                    if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
                    {
                        vout = MathExt.VectorMatrixMult(ref center, ref rotationMatrix);
                        center = vout;
                    }

                    // onko meshi n‰kˆkent‰ss‰
                    //if (Frustum.ObjectInFrustum(wpos.X + child.mesh.center.X, wpos.Y + child.mesh.center.Y, wpos.Z + child.mesh.center.Z, child.mesh.boundingVolume))
                    {
                        if (mesh.doubleSided) GL.Disable(EnableCap.CullFace);
                        Material.SetMaterial(mesh.materialName);
                        vbo.Render();
                        if (mesh.doubleSided) GL.Enable(EnableCap.CullFace);
                        Settings.NumOfObjects++;
                    }
                }
                vbo.EndRender();

            }

            GL.PopMatrix();
        }

        /// <summary>
        /// ota haluttu mesh.
        /// </summary>
        /// <param name="index"></param>
        public Object3D GetObject(int index)
        {
            return (Object3D)objects[index];
        }
        /// <summary>
        /// ota haluttu mesh nimen perusteella.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Object3D GetObject(string name)
        {
            for (int q = 0; q < objects.Count; q++)
            {
                Object3D o = (Object3D)objects[q];
                if (o.name == name) return (Object3D)objects[q];
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
            for (int q = 0; q < objects.Count; q++)
            {
                Object3D child = (Object3D)objects[q];
                if (Textured) child.vbo.UseTextureUnits(t0, t1, t2);
                else child.vbo.UseTextureUnits(false, false, false);
            }
        }


        /// <summary>
        /// valitse testausmoodi: BoundingVolume.None, BoundingVolume.Box, BoundingVolume.Sphere
        /// </summary>
        /// <param name="mode"></param>
        public void BoundingMode(byte mode)
        {
            for (int q = 0; q < objects.Count; q++)
            {
                Object3D child = (Object3D)objects[q];
                child.mesh.boundingVolume.Mode = mode;
            }
        }


        /// <summary>
        /// Asettaa meshien 2 puolisuuden, eli ei cullata polyja.
        /// </summary>
        /// <param name="meshNumber">halutun meshin index, tai -1 niin kaikki</param>
        /// <param name="doubleSided">true/false asetetaanko/poistetaanko 2 puolisuus </param>
        public void SetDoubleSided(int meshNumber, bool doubleSided)
        {
            if (meshNumber == -1)
            {
                for (int q = 0; q < objects.Count; q++)
                {
                    Object3D child = (Object3D)objects[q];
                    child.mesh.doubleSided = doubleSided;
                }
            }
            else
            {
                Object3D child = (Object3D)objects[meshNumber];
                child.mesh.doubleSided = doubleSided;
            }
        }
    }

    public class MeshData
    {
        public string name = ""; // meshin nimi
        public string materialName = ""; // materiaalin nimi

        public BoundingVolume boundingVolume = new BoundingVolume();
        public Vector3 center = new Vector3(0, 0, 0);
        public GLSL shader = new GLSL();

        public bool doubleSided = false;

        // indexit
        public ArrayList vertexInd = new ArrayList(), normalInd = new ArrayList(), uvInd = new ArrayList();

        public void AddFace(string line)
        {
            // ota talteen  f rivi:
            // f vertex/uv/normal vertex/uv/normal vertex/uv/normal
            // eli esim: f 4/4/2 5/5/3 7/2/4  
            // tosin voi olla ilman texcoordeikin eli f 4/4 5/4 6/4   ja voi olla ilman / merkkej‰.
            int dv = 0;
            for (int t = 0; t < line.Length; t++) if (line[t] == '/') dv++;
            line = line.Replace("/", " ");
            string[] ln = line.Split(' ');

            if (dv == 0 || dv == 3) // luultavasti nyt ei ole texturecoordinaatteja
            {
                vertexInd.Add(Int32.Parse(ln[1]) - 1);
                vertexInd.Add(Int32.Parse(ln[3]) - 1);
                vertexInd.Add(Int32.Parse(ln[5]) - 1);

                normalInd.Add(Int32.Parse(ln[2]) - 1);
                normalInd.Add(Int32.Parse(ln[4]) - 1);
                normalInd.Add(Int32.Parse(ln[6]) - 1);
            }
            else // kaik mukana
            {
                vertexInd.Add(Int32.Parse(ln[1]) - 1);
                vertexInd.Add(Int32.Parse(ln[4]) - 1);
                vertexInd.Add(Int32.Parse(ln[7]) - 1);

                uvInd.Add(Int32.Parse(ln[2]) - 1);
                uvInd.Add(Int32.Parse(ln[5]) - 1);
                uvInd.Add(Int32.Parse(ln[8]) - 1);

                normalInd.Add(Int32.Parse(ln[3]) - 1);
                normalInd.Add(Int32.Parse(ln[6]) - 1);
                normalInd.Add(Int32.Parse(ln[9]) - 1);
            }

        }
    }
}
