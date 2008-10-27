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
        public Object3D(string fileName, VBO vbo)
        {
            Load(fileName, 1, 1, 1, vbo);
        }
        public Object3D(string fileName, float xs, float ys, float zs, VBO vbo)
        {
            Load(fileName, xs, ys, zs, vbo);
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

        Mesh[] meshes;
        public Mesh[] Meshes
        {
            get { return meshes; }
        }

        Vector3[] vertex, normal;
        Vector2[] uv;

        public Vector3[] Vertex
        {
            get { return vertex; }
        }

        // ladataanko texturet
        bool textured = true;
        public bool Textured
        {
            set { textured = value; }
        }

        Vbo vboData;

        BoundingVolume objectBoundingVolume = new BoundingVolume(); // objektin alue
        public BoundingVolume ObjectBoundingVolume { get { return objectBoundingVolume; } }

        public void Dispose()
        {
            vboData.vbo.Dispose();

            for (int q = 0; q < meshes.Length; q++)
            {
                Material.Dispose(Meshes[q].materialName);
            }

        }


        bool staticObject = false;
        // renderointi erilailla, tämä nopeampi (meshejä ei voi animoida)
        public void LoadStatic(string fileName, VBO vbo)
        {
            staticObject = true;
            Load(fileName, vbo);
        }
        public void LoadStatic(string fileName, float xs, float ys, float zs, VBO vbo)
        {
            staticObject = true;
            Load(fileName, xs, ys, zs, vbo);
        }


        /**
         *  lataa .obj tiedosto
         */
        public void Load(string fileName, VBO vbo)
        {
            Load(fileName, 1, 1, 1, vbo);
        }

        /**
         *  lataa .obj tiedosto
         */
        public void Load(string fileName, float xs, float ys, float zs, VBO vbo)
        {
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

            name = fileName; // objektin nimeksi sama kuin tiedostonimi
            fileName = Settings.DataDir + fileName;

            int totalVerts = 0;

            // tiedosto muistiin
            string data = new System.IO.StreamReader(fileName).ReadToEnd();
            {
                data = data.Replace('\r', ' ');

                // pilko se
                string[] lines = data.Split('\n');

                int numOfVerts = 0, numOfUV = 0, numOfMeshes = 0, numOfNormals = 0; // lukumäärät
                int cvert = 0, cuv = 0, cnorm = 0; // countterit
                int curMesh = -1;

                // käsitellään rivi kerrallaan, ensin laskemalla tarvittavat,
                // eli montako meshiä, vertex lkm, uv lkm, normal lkm
                for (int q = 0; q < lines.Length; q++)
                {
                    string[] ln = lines[q].Split(' '); // pilko datat

                    if (ln[0] == "o" || ln[0] == "g" || (ln[0] == "usemtl" && lines[q - 1].StartsWith("f"))) numOfMeshes++;
                    if (ln[0] == "v") numOfVerts++;
                    if (ln[0] == "vt") numOfUV++;
                    if (ln[0] == "vn") numOfNormals++;
                }
                // varataan tilaa
                meshes = new Mesh[numOfMeshes];
                vertex = new Vector3[numOfVerts];
                normal = new Vector3[numOfNormals];
                uv = new Vector2[numOfUV];

                // lue kaikki datat objektiin ja indexit mesheihin
                for (int q = 0; q < lines.Length; q++)
                {
                    string line = lines[q];
                    if (line.StartsWith("#")) continue;
                    string[] ln = line.Split(' '); // pilko datat
                    if (ln[0] == "v") // vertex x y z
                    {
                        float x = (Util.GetFloat(ln[1]) - meshes[curMesh].pivotPoint.X) * xs;
                        float y = (Util.GetFloat(ln[2]) - meshes[curMesh].pivotPoint.Y) * ys;
                        float z = (Util.GetFloat(ln[3]) - meshes[curMesh].pivotPoint.Z) * zs;
                        vertex[cvert++] = new Vector3(x, y, z);
                        continue;
                    }

                    if (ln[0] == "vn") // normal x y z
                    {
                        normal[cnorm++] = new Vector3(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]));
                        continue;
                    }

                    if (ln[0] == "vt") // texcoord u v
                    {
                        uv[cuv++] = new Vector2(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]));
                        continue;
                    }

                    // uusi objekti
                    if (ln[0] == "o" || ln[0] == "g")
                    {
                        curMesh++;
                        meshes[curMesh] = new Mesh();

                        meshes[curMesh].name = ln[1];
                        meshes[curMesh].object3d = this;

                        // seuraavalla rivillä on #POS jos käytetty obj2 exportteria
                        if (lines[q + 1].Contains("#POS"))
                        {
                            // otetaan meshin paikka talteen (pivot point)
                            string[] spos = lines[q + 1].Split(' ');
                            meshes[curMesh].pivotPoint = new Vector3(Util.GetFloat(spos[1]), Util.GetFloat(spos[2]), Util.GetFloat(spos[3]));

                            Log.WriteDebugLine(meshes[curMesh].name + " POS: " + meshes[curMesh].pivotPoint.ToString());
                        }

                        continue;
                    }

                    // materiaali
                    if (ln[0] == "usemtl")
                    {
                        // jos kesken meshin materiaali vaihtuu, luodaan uusi mesh johon loput facet
                        if (lines[q - 1].StartsWith("f"))
                        {
                            curMesh++;
                            meshes[curMesh] = new Mesh();

                            meshes[curMesh].pivotPoint = meshes[curMesh - 1].pivotPoint;

                            meshes[curMesh].object3d = this;
                        }

                        meshes[curMesh].materialName = ln[1];
                        continue;
                    }

                    if (ln[0] == "f")
                    {
                        totalVerts += 3;
                        meshes[curMesh].AddFace(line);
                        continue;
                    }

                    // materiaalitiedosto
                    if (ln[0] == "mtllib")
                    {
                        try
                        {
                            Material.Load(dir + ln[1], textured);
                        }
                        catch (Exception e)
                        {
                            Log.WriteDebugLine(e.ToString());
                        }
                    }
                }
            }


            /*
             * kamat vbo:hon:
             * 
             *  jos on alunperin luotu vbo ja annettu parametrina,
             *  käytetään sitä, muuten luodaan objektin kaikkien meshien
             *  viemän verran tilaa ja kopsataan objekti sinne.
             * 
             */
            if (vbo == null)
            {
                vboData.vbo = new VBO();
                vbo = vboData.vbo;

                // varataan objektille (kaikille mesheille) tilaa
                vbo.AllocVBO(totalVerts, totalVerts, BufferUsageHint.StaticDraw);
            }
            else
            {
                vboData.vbo = vbo;
            }

            // kopsataan datat
            for (int q = 0; q < meshes.Length; q++)
            {
                meshes[q].vboInfo = vbo.LoadVBO(vertex, normal, uv, null, null, null, meshes[q]);

                // ota bbox/sphere joka meshille. laskee myös meshien keskipisteet.
                meshes[q].meshBoundingVolume.CalcMeshBounds(this);
            }

            // ota bounding box/sphere objektille
            objectBoundingVolume.CalcObjectBounds(this);

            Log.WriteDebugLine("Object " + name + "  meshes: " + meshes.Length);
        }


        // renderoi objekti, joko static tai animoitava, riippuen kummalla tavalla latas
        public new void Render()
        {
            if (staticObject) RenderStatic();
            else RenderMeshes();
        }


        /**
         * renderoi meshit
         */
        public void RenderMeshes()
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

            // jos objektia käännetty
            Matrix4 rotationMatrix = new Matrix4();
            Vector3 rot = -(rotation + fixRotation);
            if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
            {
                rot = rot * MathExt.HalfPI;
                Matrix4 mx = Matrix4.RotateX(rot.X);
                Matrix4 my = Matrix4.RotateY(rot.Y);
                Matrix4 mz = Matrix4.RotateZ(rot.Z);
                Matrix4 outm0;
                Matrix4.Mult(ref mx, ref my, out outm0);
                Matrix4.Mult(ref outm0, ref mz, out rotationMatrix);
            }

            vboData.vbo.BeginRender();
            for (int q = 0; q < meshes.Length; q++)
            {
                Mesh m = (Mesh)meshes[q];

                // tarkista onko objekti näkökentässä
                if (Frustum.ObjectInFrustum(wpos.X, wpos.Y, wpos.Z, objectBoundingVolume))
                {
                    // jos objektia käännetty, pitää laskea mesheille uudet keskipisteet
                    Vector3 vout, center = m.center;
                    if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
                    {
                        vout = MathExt.VectorMatrixMult(ref center, ref rotationMatrix);
                        center = vout;
                    }

                    // onko meshi näkökentässä
                    if (Frustum.ObjectInFrustum(wpos.X + center.X, wpos.Y + center.Y, wpos.Z + center.Z, m.meshBoundingVolume))
                    {
                        GL.PushMatrix();
                        GL.Translate(m.pivotPoint);
                        GL.Rotate(m.rotation.W, m.rotation.X, m.rotation.Y, m.rotation.Z);

                        if (m.doubleSided) GL.Disable(EnableCap.CullFace);
                        Material.SetMaterial(m.materialName);
                        vboData.vbo.Render(m.vboInfo);
                        if (m.doubleSided) GL.Enable(EnableCap.CullFace);
                        Settings.NumOfObjects++;

                        GL.PopMatrix();


                    }
                }

            }
            vboData.vbo.EndRender();

            // renderoidaan myös kaikki childit
            if (objects.Count != 0)
            {
                base.RenderTree();
            }

            GL.PopMatrix();
        }


        /**
         * renderoi staticobject eli meshejä ei voi erikseen animoida (pyöritellä ym)
         */
        public void RenderStatic()
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

            // jos objektia käännetty
            Matrix4 rotationMatrix = new Matrix4();
            Vector3 rot = -(rotation + fixRotation);
            if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
            {
                rot = rot * MathExt.HalfPI;
                Matrix4 mx = Matrix4.RotateX(rot.X);
                Matrix4 my = Matrix4.RotateY(rot.Y);
                Matrix4 mz = Matrix4.RotateZ(rot.Z);
                Matrix4 outm0;
                Matrix4.Mult(ref mx, ref my, out outm0);
                Matrix4.Mult(ref outm0, ref mz, out rotationMatrix);
            }

            vboData.vbo.BeginRender();
            for (int q = 0; q < meshes.Length; q++)
            {
                Mesh m = (Mesh)meshes[q];

                // tarkista onko objekti näkökentässä
                if (Frustum.ObjectInFrustum(wpos.X, wpos.Y, wpos.Z, objectBoundingVolume))
                {
                    // jos objektia käännetty, pitää laskea mesheille uudet keskipisteet
                    Vector3 vout, center = m.center;
                    if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
                    {
                        vout = MathExt.VectorMatrixMult(ref center, ref rotationMatrix);
                        center = vout;
                    }

                    // onko meshi näkökentässä
                    if (Frustum.ObjectInFrustum(wpos.X + center.X, wpos.Y + center.Y, wpos.Z + center.Z, m.meshBoundingVolume))
                    {
                        if (m.doubleSided) GL.Disable(EnableCap.CullFace);
                        Material.SetMaterial(m.materialName);
                        vboData.vbo.Render(m.vboInfo);
                        if (m.doubleSided) GL.Enable(EnableCap.CullFace);
                        Settings.NumOfObjects++;
                    }
                }

            }
            vboData.vbo.EndRender();

            // renderoidaan myös kaikki childit
            if (objects.Count != 0)
            {
                base.RenderTree();
            }

            GL.PopMatrix();
        }

        /**
         * renderoi haluttu mesh
         */
        public void Render(int meshNumber)
        {
            if (meshNumber < 0 || meshNumber >= meshes.Length) return;

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

            // jos objektia käännetty
            Matrix4 rotationMatrix = new Matrix4();
            Vector3 rot = -(rotation + fixRotation);
            if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
            {
                rot = rot * MathExt.HalfPI;
                Matrix4 mx = Matrix4.RotateX(rot.X);
                Matrix4 my = Matrix4.RotateY(rot.Y);
                Matrix4 mz = Matrix4.RotateZ(rot.Z);
                Matrix4 outm0;
                Matrix4.Mult(ref mx, ref my, out outm0);
                Matrix4.Mult(ref outm0, ref mz, out rotationMatrix);
            }

            vboData.vbo.BeginRender();
            //for (int q = 0; q < meshes.Length; q++)
            int q = meshNumber;
            {
                Mesh m = (Mesh)meshes[q];

                // tarkista onko objekti näkökentässä
                if (Frustum.ObjectInFrustum(wpos.X, wpos.Y, wpos.Z, objectBoundingVolume))
                {
                    // jos objektia käännetty, pitää laskea mesheille uudet keskipisteet
                    Vector3 vout, center = m.center;
                    if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
                    {
                        vout = MathExt.VectorMatrixMult(ref center, ref rotationMatrix);
                        center = vout;
                    }

                    // onko meshi näkökentässä
                    if (Frustum.ObjectInFrustum(wpos.X + center.X, wpos.Y + center.Y, wpos.Z + center.Z, m.meshBoundingVolume))
                    {
                        Material.SetMaterial(m.materialName);
                        vboData.vbo.Render(m.vboInfo);

                        Settings.NumOfObjects++;
                    }

                }

            }
            vboData.vbo.EndRender();

            // renderoidaan myös kaikki childit
            if (objects.Count != 0)
            {
                base.RenderTree();
            }

            GL.PopMatrix();
        }



        // voi erikseen valita mitä texture unitteja käytetään jos multitexture
        public void UseTextureUnits(bool t0, bool t1, bool t2)
        {
            if (textured) vboData.vbo.UseTextureUnits(t0, t1, t2);
            else vboData.vbo.UseTextureUnits(false, false, false);
        }

        /**
         *  1==box, 2==sphere
         *  
         */
        public void BoundingMode(byte mode)
        {
            for (int q = 0; q < meshes.Length; q++) meshes[q].meshBoundingVolume.Mode = mode;
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
                for (int q = 0; q < meshes.Length; q++)
                {
                    meshes[q].doubleSided = doubleSided;
                }
            }
            else meshes[meshNumber].doubleSided = doubleSided;
        }

        /// <summary>
        /// yksittäisen meshin asento. xyz akseli ja w angle
        /// </summary>
        /// <param name="meshNumber"></param>
        /// <param name="rotation"></param>
        public void SetMeshRotation(int meshNumber, Vector4 rotation)
        {
            meshes[meshNumber].rotation = rotation;
        }


    }

    public class Mesh
    {
        public string name = ""; // meshin nimi
        public string materialName = ""; // materiaalin nimi

        public BoundingVolume meshBoundingVolume = new BoundingVolume();
        public Vector3 center = new Vector3(0, 0, 0);
        public Vector3 pivotPoint = new Vector3(0, 0, 0);
        public Vector4 rotation = new Vector4(0, 0, 0, 0);

        public VBOInfo vboInfo; // vbo aloituspaikka ja pituus
        public GLSL shader = new GLSL();

        public Object3D object3d = null; // linkki objektiin mihin tämä mesh kuuluu

        public bool doubleSided = false;

        // indexit
        public ArrayList vertexInd = new ArrayList(), normalInd = new ArrayList(), uvInd = new ArrayList();

        public void AddFace(string line)
        {
            // ota talteen  f rivi:
            // f vertex/uv/normal vertex/uv/normal vertex/uv/normal
            // eli esim: f 4/4/2 5/5/3 7/2/4  
            // tosin voi olla ilman texcoordeikin eli f 4/4 5/4 6/4   ja voi olla ilman / merkkejä.
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
