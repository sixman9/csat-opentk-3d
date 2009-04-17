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

/* 
 * lataa mesh tiedostoja
 *
 */
using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public class ObjModel : Mesh
    {
        VBO vbo = null;
        List<ObjModel> meshes = new List<ObjModel>();
        public override List<ObjModel> Meshes()
        {
            return meshes;
        }

        // joka meshin indexit. k‰ytet‰‰n vain latauksessa ja tyhjennet‰‰n sen j‰lkeen.
        private List<int> _vertexInd = new List<int>();
        private List<int> _normalInd = new List<int>();
        private List<int> _uvInd = new List<int>();
        protected List<Vector3> pathData = new List<Vector3>();

        /// <summary>
        /// luo objekti
        /// </summary>
        /// <param name="name">objektin nimi</param>
        public ObjModel(string name)
        {
            Name = name;
        }

        public ObjModel(string name, string fileName)
        {
            Name = name;
            Load(fileName, 1, 1, 1);
        }

        public ObjModel(string name, string fileName, float xs, float ys, float zs)
        {
            Name = name;
            Load(fileName, xs, ys, zs);
        }

        public override void SetDoubleSided(string name, bool doublesided)
        {
            for (int q = 0; q < meshes.Count; q++)
            {
                if (meshes[q].Name == name)
                {
                    meshes[q].DoubleSided = doublesided;
                }
            }
        }


        #region objloader code
        /// <summary>
        /// lataa mesh tiedosto
        /// </summary>
        /// <param name="fileName"></param>
        public void Load(string fileName)
        {
            Load(fileName, 1, 1, 1);
        }

        /// <summary>
        /// lataa mesh tiedosto
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="xs"></param>
        /// <param name="ys"></param>
        /// <param name="zs"></param>
        public void Load(string fileName, float xs, float ys, float zs)
        {
            pathData.Clear();
            ObjModel mesh = null;
            List<Vector3> _vertex = new List<Vector3>();
            List<Vector3> _normal = new List<Vector3>();
            List<Vector2> _uv = new List<Vector2>();

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

            fileName = Settings.DataDir + fileName;
            bool path = false; // jos reitti

            using (System.IO.StreamReader file = new System.IO.StreamReader(fileName))
            {
                // tiedosto muistiin
                string data = file.ReadToEnd();
                data = data.Replace('\r', ' ');

                // pilko se
                string[] lines = data.Split('\n');

                int numOfFaces = 0;
                for (int q = 0; q < lines.Length; q++)
                {
                    string[] ln = lines[q].Split(' '); // pilko datat
                    if (ln[0] == "f") numOfFaces++;
                }

                // lue kaikki datat objektiin ja indexit mesheihin
                for (int q = 0; q < lines.Length; q++)
                {
                    string line = lines[q];
                    if (line.StartsWith("#")) continue;
                    string[] ln = line.Split(' '); // pilko datat
                    if (ln[0] == "v") // vertex x y z
                    {
                        float x = (Util.GetFloat(ln[1]) - mesh.Position.X) * xs;
                        float y = (Util.GetFloat(ln[2]) - mesh.Position.Y) * ys;
                        float z = (Util.GetFloat(ln[3]) - mesh.Position.Z) * zs;

                        if (path) pathData.Add(new Vector3(x, y, z));
                        else _vertex.Add(new Vector3(x, y, z));

                        continue;
                    }

                    if (ln[0] == "vn") // normal x y z
                    {
                        _normal.Add(new Vector3(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3])));
                        continue;
                    }

                    if (ln[0] == "vt") // texcoord U V
                    {
                        _uv.Add(new Vector2(Util.GetFloat(ln[1]), Util.GetFloat(ln[2])));
                        continue;
                    }

                    // uusi objekti
                    if (ln[0] == "o" || ln[0] == "g")
                    {
                        if (mesh != null) meshes.Add(mesh); // talteen
                        mesh = new ObjModel(ln[1]);
                        mesh.material = material;

                        // Nimess‰ voi olla ohjeita mit‰ muuta halutaan, esim:
                        // * Path_reitti1  jolloin ei ladata objektia mutta reitti jota pitkin kamera/objektit voi kulkea.
                        // * BBox_nimi/BSphere_nimi jolloin t‰m‰ onkin nimi-objektin bounding box/sphere.
                        if (mesh.Name.Contains("Path_"))
                        {
                            path = true;
                        }
                        else if (mesh.Name.Contains("BBox_") || mesh.Name.Contains("BSphere_"))
                        {
                            // TODO: bbox_ bsphere_
                        }

                        continue;
                    }

                    // materiaali
                    if (ln[0] == "usemtl")
                    {
                        // jos kesken meshin materiaali vaihtuu, luodaan uusi obu johon loput facet
                        if (lines[q - 1].StartsWith("f"))
                        {
                            meshes.Add(mesh);
                            Vector3 tmpPos = mesh.Position;

                            mesh = new ObjModel(mesh.Name);
                            mesh.material = material;
                            mesh.Position = tmpPos; // samaa objektia, niin sama Position

                        }

                        mesh.MaterialName = ln[1];
                        continue;
                    }

                    if (ln[0] == "f")
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
                        string[] _ln = line.Split(' ');

                        if (dv == 0 || dv == 3) // luultavasti nyt ei ole texturecoordinaatteja
                        {
                            mesh._vertexInd.Add(Int32.Parse(_ln[1]) - 1);
                            mesh._vertexInd.Add(Int32.Parse(_ln[3]) - 1);
                            mesh._vertexInd.Add(Int32.Parse(_ln[5]) - 1);

                            mesh._normalInd.Add(Int32.Parse(_ln[2]) - 1);
                            mesh._normalInd.Add(Int32.Parse(_ln[4]) - 1);
                            mesh._normalInd.Add(Int32.Parse(_ln[6]) - 1);
                        }
                        else // kaikki mukana
                        {
                            mesh._vertexInd.Add(Int32.Parse(_ln[1]) - 1);
                            mesh._vertexInd.Add(Int32.Parse(_ln[4]) - 1);
                            mesh._vertexInd.Add(Int32.Parse(_ln[7]) - 1);

                            mesh._uvInd.Add(Int32.Parse(_ln[2]) - 1);
                            mesh._uvInd.Add(Int32.Parse(_ln[5]) - 1);
                            mesh._uvInd.Add(Int32.Parse(_ln[8]) - 1);

                            mesh._normalInd.Add(Int32.Parse(_ln[3]) - 1);
                            mesh._normalInd.Add(Int32.Parse(_ln[6]) - 1);
                            mesh._normalInd.Add(Int32.Parse(_ln[9]) - 1);
                        }
                        continue;
                    }

                    // materiaalitiedosto
                    if (ln[0] == "mtllib")
                    {
                        try
                        {
                            // ladataan objektille materiaalitiedot (mesheille otetaan talteen materialname joka viittaa sitten n‰ihin materiaaleihin)
                            material = new Material();
                            material.Load(dir + ln[1], Texture.LoadTextures);
                        }
                        catch (Exception e)
                        {
                            Log.WriteDebugLine(e.ToString());
                        }
                    }
                }

                if (mesh != null) meshes.Add(mesh);

                // pathille ei luoda objektia, se on vain kasa vertexej‰
                if (path == false)
                {
                    int cc = 0;

                    vertices = new Vertex[numOfFaces * 3];
                    for (int m = 0; m < meshes.Count; m++)
                    {
                        meshes[m].vertices = new Vertex[meshes[m]._vertexInd.Count];

                        for (int q = 0; q < meshes[m]._vertexInd.Count; q++)
                        {
                            // mesh datat
                            meshes[m].vertices[q].vertex = _vertex[meshes[m]._vertexInd[q]];
                            meshes[m].vertices[q].normal = _normal[meshes[m]._normalInd[q]];
                            if (meshes[m]._uvInd.Count != 0)
                                meshes[m].vertices[q].uv_or_color = new Vector4(_uv[meshes[m]._uvInd[q]].X, _uv[meshes[m]._uvInd[q]].Y,
                                                                                _uv[meshes[m]._uvInd[q]].X, _uv[meshes[m]._uvInd[q]].Y);

                            // pistet‰‰n myˆs objektille kaikki vertexit yhteen klimppiin.

//TODO Miks? vie vaa muistia nii paljo, mesheiss‰ kumminki nuo datat s‰ilytet‰‰n.
// jos t‰‰ on VAIN collisionia varten, siihe ny o helppo tehd‰ se et se menee objektin puun l‰pi
// ni sit ei tartte t‰t‰
                            vertices[cc].vertex = _vertex[meshes[m]._vertexInd[q]];
                            vertices[cc].normal = _normal[meshes[m]._normalInd[q]];
                            if (meshes[m]._uvInd.Count != 0)
                                vertices[cc].uv_or_color = new Vector4(_uv[meshes[m]._uvInd[q]].X, _uv[meshes[m]._uvInd[q]].Y,
                                                                                _uv[meshes[m]._uvInd[q]].X, _uv[meshes[m]._uvInd[q]].Y);
                            cc++;
                        }

                        // index taulukko
                        meshes[m].indices = new int[meshes[m]._vertexInd.Count];
                        for (int q = 0; q < meshes[m]._vertexInd.Count; q++) meshes[m].indices[q] = q;

                        meshes[m].vbo = new VBO();
                        meshes[m].vbo.DataToVBO(meshes[m].vertices, meshes[m].indices);

                        // meshin bounding volume
                        meshes[m].Boundings = new BoundingVolume();
                        meshes[m].Boundings.CreateBoundingVolume(meshes[m]);

                        // lataa glsl koodit
                        string shader = material.GetMaterial(meshes[m].MaterialName).ShaderName;
                        if (shader != "")
                        {
                            mesh.Shader = new GLSL();
                            mesh.Shader.Load(shader + ".vert", shader + ".frag");
                        }
                        if (material.GetMaterial(meshes[m].MaterialName).Dissolve < 1.0f) IsTranslucent = true;

                    }

                    // koko objektin bounding volume
                    IsRendObj = false; // childeiss‰ rendattavat objektit, t‰ss‰ ei ole rendattavaa, vain paikka
                    Boundings = new BoundingVolume();
                    Boundings.CreateBoundingVolume(this);

                    // lis‰t‰‰n objektille meshit childeiks
                    for (int q = 0; q < meshes.Count; q++)
                    {
                        Add(meshes[q]);
                    }

                    Log.WriteDebugLine("Object: " + Name + "  meshes: " + meshes.Count);
                }

                _vertex.Clear();
                _normal.Clear();
                _uv.Clear();
                for (int q = 0; q < meshes.Count; q++)
                {
                    meshes[q]._vertexInd.Clear();
                    meshes[q]._normalInd.Clear();
                    meshes[q]._uvInd.Clear();
                }

            }
        }

        #endregion


        protected override void RenderObject()
        {
            GL.LoadMatrix(Matrix);
            RenderMesh();
        }

        public override void Render()
        {
            base.Render(); // renderoi objektin ja kaikki siihen liitetyt objektit
        }

        public void RenderMesh()
        {
            if (vbo != null)
            {
                material.SetMaterial(MaterialName);
                if (Shader != null) Shader.UseProgram();
                if (DoubleSided) GL.Disable(EnableCap.CullFace);
                vbo.BeginRender();
                vbo.Render();
                vbo.EndRender();

                if (DoubleSided) GL.Enable(EnableCap.CullFace);
                if (Shader != null) Shader.RemoveProgram();
                Settings.NumOfObjects++;
            }
        }
    }
}
