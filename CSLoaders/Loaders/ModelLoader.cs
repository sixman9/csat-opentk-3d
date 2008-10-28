#region --- License ---
/* 
 * ModelLoader.cs
 * 
 * Copyright (c) 2008 mjt[matola@sci.fi]
 * 
 * based on "MD5Mesh Loader" found at http://www.bokebb.com/dev/english/2004/posts/2004105894.shtml
 * and 
 * "md5mesh model loader + animation" found at http://tfc.duke.free.fr/coding/md5-specs-en.html
 *
 * Copyright (c) 2005-2007 David HENRY
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
 */
#endregion

using System;
using System.Collections;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSLoader
{
    public class MD5Model : CSat.ObjectInfo, CSat.IModel
    {
        private bool animated = false;
        CSat.Vbo vboData;
        private CSat.Vertex[] vertices;

        private int numJoints;
        private int numMesh;
        private int meshCount;
        private int numOfFaces;
        private Vector3[] finalVert;
        private Vector3[] normals;
        private ArrayList meshes;
        private int totalNumOfVerts = 0;

        CSat.BoundingVolume bounds = new CSat.BoundingVolume();

        private struct MD5Vertex
        {
            public int startw;
            public int countw;
            public Vector2 uv;
        }
        private struct MD5Weight
        {
            public int joint;
            public float bias;
            public Vector3 pos;
        }
        private struct MD5Mesh
        {
            public CSat.Texture texture;
            public int numVert;
            public int numTris;
            public int numWeights;
            public MD5Vertex[] verts;
            public int[][] faces;
            public MD5Weight[] weights;

            public CSat.VBOInfo vi; // vbo aloituspaikka ja pituus
        }
        private MD5Mesh[] model;

        public struct MD5Joint
        {
            public string name;
            public int parent;
            public Vector3 pos;
            public Quaternion orient;
        }
        private MD5Joint[] baseSkel;

        /* Bounding box */
        public struct MD5BoundingBox
        {
            public Vector3 min;
            public Vector3 max;
        };

        /* Animation data */
        public struct MD5Animation
        {
            public int numFrames;
            public int numJoints;
            public int frameRate;
            public MD5Joint[,] skelFrames;
            public MD5BoundingBox[] bboxes;

            public int curFrame;
            public int nextFrame;
            public float lastTime;
            public float maxTime;

        };

        /* Joint info */
        struct MD5JointInfo
        {
            public string name;
            public int parent;
            public int flags;
            public int startIndex;
        };

        /* Base frame joint */
        struct MD5BaseFrameJoint
        {
            public Vector3 pos;
            public Quaternion orient;
        };

        public void Dispose()
        {
            vboData.vbo.Dispose();

            for (int q = 0; q < model.Length; q++)
            {
                model[q].texture.Dispose();
            }
        }

        private string ext = "";
        // jos shaderin päätettä ei ole md5 tiedostoissa, voi pistää tässä, eli esim .jpg, .tga, .dds ..
        public void UseExt(string ext)
        {
            this.ext = ext;
        }


        int NumOfTextures = 1; // jos käytetään enemmän kuin 1 texturea, tätä pitää muuttaa.
        // voi erikseen valita mitä texture unitteja käytetään jos multitexture
        public void SetTextureUnits()
        {
            switch (numOfFaces)
            {
                case 1:
                    vboData.vbo.UseTextureUnits(true, false, false);
                    break;
                case 2:
                    vboData.vbo.UseTextureUnits(true, true, false);
                    break;
                case 3:
                    vboData.vbo.UseTextureUnits(true, true, true);
                    break;
            }
        }


        /**
         *  lataa md5 malli.
         *  vbo:  varattu vbo tai null jos varataan vain 3d-mallin viemä tila
         */
        public void Load(string fileName, CSat.VBO vbo)
        {
            name = fileName;

            // Initialize everything
            string line, textureName = "";
            int i;
            Buffer t = new Buffer();
            meshCount = 0;
            System.IO.StreamReader file = new System.IO.StreamReader(CSat.Settings.DataDir + fileName);
            while ((line = file.ReadLine()) != null)
            {
                Cleanstring(ref line);

                // Read number of joints
                if (ParseLine(ref t, line, "numJoints %d"))
                {
                    numJoints = t.ibuffer[0];
                    baseSkel = new MD5Joint[numJoints];
                }
                // Read number os meshes
                if (ParseLine(ref t, line, "numMeshes %d"))
                {
                    numMesh = t.ibuffer[0];
                    model = new MD5Mesh[numMesh];

                    meshes = new ArrayList(numMesh);
                }
                // Parse model joints
                if (line.Equals("joints {"))
                {
                    for (i = 0; i < numJoints; i++)
                    {
                        line = file.ReadLine();
                        Cleanstring(ref line);

                        ParseLine(ref t, line, "%s %d ( %f %f %f ) ( %f %f %f )");
                        baseSkel[i].name = t.sbuffer;
                        baseSkel[i].parent = t.ibuffer[0];
                        baseSkel[i].pos.X = t.fbuffer[0];
                        baseSkel[i].pos.Y = t.fbuffer[1];
                        baseSkel[i].pos.Z = t.fbuffer[2];
                        baseSkel[i].orient.XYZ.X = t.fbuffer[3];
                        baseSkel[i].orient.XYZ.Y = t.fbuffer[4];
                        baseSkel[i].orient.XYZ.Z = t.fbuffer[5];
                        CSat.MathExt.ComputeW(ref baseSkel[i].orient);
                    }
                }
                // Parse model meshes
                if (line.Equals("mesh {"))
                {
                    while (!line.Equals("}"))
                    {
                        line = file.ReadLine();
                        Cleanstring(ref line);

                        // Read texture name
                        if (line.StartsWith("shader"))
                        {
                            string str = line.Substring(7);
                            int lp = str.LastIndexOf("/");
                            textureName = str.Substring(lp + 1); // texturen nimi

                            lp = fileName.LastIndexOf("/");
                            string dir = fileName.Substring(0, lp + 1); // md5 tiedoston hakemisto

                            textureName = CSat.Settings.DataDir + dir + textureName;
                            textureName = textureName.Replace(",", ".");
                        }

                        // Read mesh data
                        if (ParseLine(ref t, line, "numverts %d"))
                        {
                            model[meshCount].numVert = t.ibuffer[0];
                            model[meshCount].verts = new MD5Vertex[model[meshCount].numVert];

                            model[meshCount].texture = CSat.Texture.Load(textureName, false);

                            for (i = 0; i < model[meshCount].numVert; i++)
                            {
                                line = file.ReadLine();
                                ParseLine(ref t, line, "vert %d ( %f %f ) %d %d");
                                model[meshCount].verts[t.ibuffer[0]].uv.X = t.fbuffer[0];
                                model[meshCount].verts[t.ibuffer[0]].uv.Y = 1 - t.fbuffer[1];
                                model[meshCount].verts[t.ibuffer[0]].startw = t.ibuffer[1];
                                model[meshCount].verts[t.ibuffer[0]].countw = t.ibuffer[2];
                            }
                        }
                        if (ParseLine(ref t, line, "numtris %d"))
                        {
                            model[meshCount].numTris = t.ibuffer[0];
                            model[meshCount].faces = new int[model[meshCount].numTris][];
                            for (i = 0; i < model[meshCount].numTris; i++)
                            {
                                line = file.ReadLine();
                                ParseLine(ref t, line, "tri %d %d %d %d");

                                model[meshCount].faces[t.ibuffer[0]] = new int[3];
                                model[meshCount].faces[t.ibuffer[0]][0] = t.ibuffer[3]; // poly toisin päin
                                model[meshCount].faces[t.ibuffer[0]][1] = t.ibuffer[2];
                                model[meshCount].faces[t.ibuffer[0]][2] = t.ibuffer[1];
                            }
                        }
                        if (ParseLine(ref t, line, "numweights %d"))
                        {
                            model[meshCount].numWeights = t.ibuffer[0];
                            model[meshCount].weights = new MD5Weight[model[meshCount].numWeights];
                            for (i = 0; i < model[meshCount].numWeights; i++)
                            {
                                line = file.ReadLine();
                                ParseLine(ref t, line, "weight %d %d %f ( %f %f %f )");
                                model[meshCount].weights[t.ibuffer[0]].joint = t.ibuffer[1];
                                model[meshCount].weights[t.ibuffer[0]].bias = t.fbuffer[0];
                                model[meshCount].weights[t.ibuffer[0]].pos.X = t.fbuffer[1];
                                model[meshCount].weights[t.ibuffer[0]].pos.Y = t.fbuffer[2];
                                model[meshCount].weights[t.ibuffer[0]].pos.Z = t.fbuffer[3];
                            }
                        }
                    }
                    meshCount++;
                }
            }
            file.Close();

            int maxvert = 0;
            for (int k = 0; k < numMesh; k++)
            {
                if (model[k].numVert > maxvert) maxvert = model[k].numVert;
            }
            for (int k = 0; k < numMesh; k++)
            {
                finalVert = new Vector3[maxvert];
                normals = new Vector3[maxvert];
            }

            skeleton = baseSkel;

            updateAnimCount = FramesBetweenAnimUpdate;
            updateNormalsCount = FramesBetweenNormalsUpdate;

            // prepare model for rendering
            PrepareMesh();

            // luo vbo
            if (vbo == null)
            {
                vboData.vbo = new CSat.VBO();
                vbo = vboData.vbo;

                // varataan objektille (kaikille mesheille) tilaa
                vbo.AllocVBO(totalNumOfVerts, totalNumOfVerts, BufferUsageHint.DynamicDraw);
            }
            else
            {
                vboData.vbo = vbo;
            }

            for (int q = 0; q < numMesh; q++)
            {
                CSat.Vertex[] v = (CSat.Vertex[])meshes[q];
                int[] ind = new int[v.Length];
                for (int w = 0; w < ind.Length; w++) ind[w] = w;

                model[q].vi = vbo.LoadVBO(v, ind);
            }
            CSat.Log.WriteDebugLine("Model: " + name);

            CSat.Material material = new CSat.Material();

        }


        MD5Animation curAnim;
        public int FramesBetweenAnimUpdate = 1; // monenko framen jälkeen lasketaan uusi asento
        int updateAnimCount = 0;
        public int FramesBetweenNormalsUpdate = 3; // monenko framen välein päivitetään normaalit
        int updateNormalsCount = 0;

        public void UpdateNormals()
        {
            updateNormalsCount = FramesBetweenNormalsUpdate;
        }
        public void UpdateAnimation()
        {
            updateAnimCount = FramesBetweenAnimUpdate;
        }

        // Prepare mesh for rendering
        void PrepareMesh()
        {
            totalNumOfVerts = 0;
            int i, j, k;
            meshes.Clear();

            // Calculate the final position ingame position of all the model vertexes
            for (k = 0; k < numMesh; k++)
            {
                numOfFaces = model[k].numTris;
                vertices = new CSat.Vertex[numOfFaces * 3];

                for (i = 0; i < model[k].numVert; i++)
                {
                    Vector3 finalVertex = new Vector3(0, 0, 0);
                    for (j = 0; j < model[k].verts[i].countw; j++)
                    {
                        MD5Weight wt = model[k].weights[model[k].verts[i].startw + j];
                        MD5Joint joint = skeleton[wt.joint];

                        Vector3 wv = CSat.MathExt.RotatePoint(ref joint.orient, ref wt.pos);
                        finalVertex.X += (joint.pos.X + wv.X) * wt.bias;
                        finalVertex.Y += (joint.pos.Y + wv.Y) * wt.bias;
                        finalVertex.Z += (joint.pos.Z + wv.Z) * wt.bias;
                    }
                    finalVert[i] = finalVertex;
                }

                // aika laskea normaalit uudelleen?
                if (updateNormalsCount == FramesBetweenNormalsUpdate)
                {
                    CSat.MathExt.CalcNormals(ref finalVert, ref model[k].faces, ref normals, false);
                    updateNormalsCount = 0;
                }
                else updateAnimCount++;

                int count = 0;
                // Organize the final vertexes acording to the meshes triangles
                for (i = 0; i < model[k].numTris; i++)
                {
                    vertices[count] = new CSat.Vertex(finalVert[(int)model[k].faces[i][0]], normals[(int)model[k].faces[i][0]], model[k].verts[(int)model[k].faces[i][0]].uv);
                    vertices[count + 1] = new CSat.Vertex(finalVert[(int)model[k].faces[i][1]], normals[(int)model[k].faces[i][1]], model[k].verts[(int)model[k].faces[i][1]].uv);
                    vertices[count + 2] = new CSat.Vertex(finalVert[(int)model[k].faces[i][2]], normals[(int)model[k].faces[i][2]], model[k].verts[(int)model[k].faces[i][2]].uv);

                    count += 3;
                    totalNumOfVerts += 3;
                }
                meshes.Add(vertices);

                if (vboData.vbo != null) vboData.vbo.Update(vertices, model[k].vi);

            }

        }

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

            int i = 0;
            // tarkista onko objekti näkökentässä
            if (CSat.Frustum.ObjectInFrustum(position.X, position.Y, position.Z, bounds))
            {
                vboData.vbo.BeginRender();

                CSat.Material.SetMaterial("defaultMaterial");
                SetTextureUnits();
                model[i].texture.Bind();

                // lasketaanko uusi asento
                if (updateAnimCount == FramesBetweenAnimUpdate)
                {
                    // Interpolate skeletons between two frames
                    InterpolateSkeletons(ref curAnim.skelFrames, curAnim.curFrame, curAnim.nextFrame,
                              curAnim.numJoints,
                              curAnim.lastTime * curAnim.frameRate);

                    PrepareMesh();
                    updateAnimCount = 0;
                }
                else updateAnimCount++;

                //Render_debug(v);
                vboData.vbo.Render(model[i].vi);
                i++;

                CSat.Settings.NumOfObjects++;

                vboData.vbo.EndRender();
            }


            // renderoidaan myös kaikki childit
            if (objects.Count != 0)
            {
                base.RenderTree();
            }

            GL.PopMatrix();

        }

        void Render_debug(CSat.Vertex[] v)
        {
            GL.Begin(BeginMode.Triangles);
            for (int q = 0; q < v.Length; q++)
            {
                GL.TexCoord2(v[q].uv1);
                GL.Vertex3(v[q].vertex);
            }
            GL.End();
        }

        // animation code
        MD5Joint[] skeleton = null;

        /**
         * Build skeleton for a given frame data.
         */
        void BuildFrameSkeleton(ref MD5JointInfo[] jointInfos,
                    ref MD5BaseFrameJoint[] baseFrame,
                    ref float[] animFrameData,
                    int frameIndex,
                    int num_joints, ref MD5Animation md5anim)
        {
            int i;

            for (i = 0; i < num_joints; ++i)
            {
                MD5BaseFrameJoint baseJoint = baseFrame[i];
                Vector3 animatedPos;
                Quaternion animatedOrient;
                int j = 0;

                animatedPos = baseJoint.pos;
                animatedOrient = baseJoint.orient;

                if ((jointInfos[i].flags & 1) > 0) /* Tx */
                {
                    animatedPos.X = animFrameData[jointInfos[i].startIndex + j];
                    ++j;
                }

                if ((jointInfos[i].flags & 2) > 0) /* Ty */
                {
                    animatedPos.Y = animFrameData[jointInfos[i].startIndex + j];
                    ++j;
                }

                if ((jointInfos[i].flags & 4) > 0) /* Tz */
                {
                    animatedPos.Z = animFrameData[jointInfos[i].startIndex + j];
                    ++j;
                }

                if ((jointInfos[i].flags & 8) > 0) /* Qx */
                {
                    animatedOrient.XYZ.X = animFrameData[jointInfos[i].startIndex + j];
                    ++j;
                }

                if ((jointInfos[i].flags & 16) > 0) /* Qy */
                {
                    animatedOrient.XYZ.Y = animFrameData[jointInfos[i].startIndex + j];
                    ++j;
                }

                if ((jointInfos[i].flags & 32) > 0) /* Qz */
                {
                    animatedOrient.XYZ.Z = animFrameData[jointInfos[i].startIndex + j];
                    ++j;
                }

                /* Compute orient quaternion's w value */
                CSat.MathExt.ComputeW(ref animatedOrient);

                int parent = jointInfos[i].parent;
                md5anim.skelFrames[frameIndex, i].parent = parent;
                md5anim.skelFrames[frameIndex, i].name = jointInfos[i].name;

                /* Has parent? */
                if (md5anim.skelFrames[frameIndex, i].parent < 0)
                {
                    md5anim.skelFrames[frameIndex, i].pos = animatedPos;
                    md5anim.skelFrames[frameIndex, i].orient = animatedOrient;
                }
                else
                {
                    MD5Joint parentJoint = md5anim.skelFrames[frameIndex, parent];
                    Vector3 rpos; /* Rotated position */

                    /* Add positions */
                    rpos = CSat.MathExt.RotatePoint(ref parentJoint.orient, ref animatedPos);

                    md5anim.skelFrames[frameIndex, i].pos.X = rpos.X + parentJoint.pos.X;
                    md5anim.skelFrames[frameIndex, i].pos.Y = rpos.Y + parentJoint.pos.Y;
                    md5anim.skelFrames[frameIndex, i].pos.Z = rpos.Z + parentJoint.pos.Z;

                    /* Concatenate rotations */
                    md5anim.skelFrames[frameIndex, i].orient = CSat.MathExt.Mult(ref parentJoint.orient, ref animatedOrient);
                    md5anim.skelFrames[frameIndex, i].orient = CSat.MathExt.Normalize(ref md5anim.skelFrames[frameIndex, i].orient);
                }

            }
        }



        /**
         * Smoothly interpolate two skeletons
         */
        void InterpolateSkeletons(ref MD5Joint[,] skel, int curFrame, int nextFrame,
                      int num_joints, float interp)
        {
            int i;

            for (i = 0; i < num_joints; ++i)
            {
                /* Copy parent index */
                skeleton[i].parent = skel[curFrame, i].parent;

                /* Linear interpolation for position */
                skeleton[i].pos.X = skel[curFrame, i].pos.X + interp * (skel[nextFrame, i].pos.X - skel[curFrame, i].pos.X);
                skeleton[i].pos.Y = skel[curFrame, i].pos.Y + interp * (skel[nextFrame, i].pos.Y - skel[curFrame, i].pos.Y);
                skeleton[i].pos.Z = skel[curFrame, i].pos.Z + interp * (skel[nextFrame, i].pos.Z - skel[curFrame, i].pos.Z);

                /* Spherical linear interpolation for orientation */
                skeleton[i].orient = CSat.MathExt.Slerp(ref skel[curFrame, i].orient, ref skel[nextFrame, i].orient, interp);
            }
        }

        /**
         * Perform animation related computations.  Calculate the current and
         * next frames, given a delta time.
         */
        void Animate(ref MD5Animation anim, float dt)
        {
            int maxFrames = anim.numFrames - 1;

            anim.lastTime += dt;

            /* move to next frame */
            if (anim.lastTime >= anim.maxTime)
            {
                anim.curFrame++;
                anim.nextFrame++;
                anim.lastTime = 0.0f;

                if (anim.curFrame > maxFrames)
                    anim.curFrame = 0;

                if (anim.nextFrame > maxFrames)
                    anim.nextFrame = 0;

            }
        }


        public void Update(float time, ref MD5Animation md5anim)
        {
            if (animated)
            {
                curAnim = md5anim;

                /* Calculate current and next frames */
                Animate(ref md5anim, time);
            }
            else
            {
                /* No animation, use bind-pose skeleton */
                skeleton = baseSkel;
            }

        }

        /**
         * Load an MD5 animation from file.
         */
        public void LoadAnim(string fileName, ref MD5Animation anim)
        {
            if (fileName == null || fileName == "") return;

            Buffer t = new Buffer();
            MD5JointInfo[] jointInfos = null;
            MD5BaseFrameJoint[] baseFrame = null;
            float[] animFrameData = null;
            int numAnimatedComponents = 0;
            int frame_index;
            int i;

            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(CSat.Settings.DataDir + fileName);

            while ((line = file.ReadLine()) != null)
            {
                if (line == "") continue;

                // Read number of joints
                if (ParseLine(ref t, line, "numFrames %d"))
                {
                    /* Allocate memory for skeleton frames and bounding boxes */
                    anim.numFrames = t.ibuffer[0];
                    if (anim.numFrames > 0)
                    {
                        anim.bboxes = new MD5BoundingBox[anim.numFrames];
                    }
                }

                if (ParseLine(ref t, line, "numJoints %d"))
                {
                    /* Allocate memory for joints of each frame */
                    anim.numJoints = t.ibuffer[0];
                    if (anim.numJoints > 0)
                    {
                        /* Allocate temporary memory for building skeleton frames */
                        jointInfos = new MD5JointInfo[anim.numJoints];
                        baseFrame = new MD5BaseFrameJoint[anim.numJoints];
                    }
                    anim.skelFrames = new MD5Joint[anim.numFrames, anim.numJoints];
                }

                if (ParseLine(ref t, line, "frameRate %d"))
                {
                    anim.frameRate = t.ibuffer[0];
                }

                if (ParseLine(ref t, line, "numAnimatedComponents %d"))
                {
                    numAnimatedComponents = t.ibuffer[0];
                    if (numAnimatedComponents > 0)
                    {
                        /* Allocate memory for animation frame data */
                        animFrameData = new float[numAnimatedComponents];
                    }

                }

                if (line.Equals("hierarchy {"))
                {
                    for (i = 0; i < anim.numJoints; ++i)
                    {
                        /* Read whole line */
                        line = file.ReadLine();
                        Cleanstring(ref line);

                        /* Read joint info */
                        ParseLine(ref t, line, "%s %d %d %d");
                        jointInfos[i].name = t.sbuffer;
                        jointInfos[i].parent = t.ibuffer[0];
                        jointInfos[i].flags = t.ibuffer[1];
                        jointInfos[i].startIndex = t.ibuffer[2];
                    }
                }

                if (line.Equals("bounds {"))
                {
                    for (i = 0; i < anim.numFrames; ++i)
                    {
                        /* Read whole line */
                        line = file.ReadLine();
                        Cleanstring(ref line);

                        /* Read bounding box */
                        ParseLine(ref t, line, "( %f %f %f ) ( %f %f %f )");
                        anim.bboxes[i].min.X = t.fbuffer[0];
                        anim.bboxes[i].min.Y = t.fbuffer[1];
                        anim.bboxes[i].min.Z = t.fbuffer[2];
                        anim.bboxes[i].max.X = t.fbuffer[3];
                        anim.bboxes[i].max.Y = t.fbuffer[4];
                        anim.bboxes[i].max.Z = t.fbuffer[5];
                    }
                }

                if (line.Equals("baseframe {"))
                {
                    for (i = 0; i < anim.numJoints; ++i)
                    {
                        /* Read whole line */
                        line = file.ReadLine();
                        Cleanstring(ref line);

                        /* Read base frame joint */
                        ParseLine(ref t, line, "( %f %f %f ) ( %f %f %f )");

                        if (t.fbuffer.Length == 6)
                        {
                            baseFrame[i].pos.X = t.fbuffer[0];
                            baseFrame[i].pos.Y = t.fbuffer[1];
                            baseFrame[i].pos.Z = t.fbuffer[2];
                            baseFrame[i].orient.XYZ.X = t.fbuffer[3];
                            baseFrame[i].orient.XYZ.Y = t.fbuffer[4];
                            baseFrame[i].orient.XYZ.Z = t.fbuffer[5];

                            /* Compute the w component */
                            CSat.MathExt.ComputeW(ref baseFrame[i].orient);

                        }
                    }
                }

                if (ParseLine(ref t, line, "frame %d"))
                {
                    frame_index = t.ibuffer[0];

                    /* Read frame data */
                    for (i = 0; i < numAnimatedComponents; )
                    {
                        line = file.ReadLine();
                        if (line[0] == '}') break;
                        Cleanstring(ref line);
                        string[] splt = line.Split(' ');

                        for (int ww = 0; ww < splt.Length; ww++)
                        {
                            animFrameData[i++] = float.Parse(splt[ww]);
                        }
                    }

                    /* Build frame skeleton from the collected data */
                    BuildFrameSkeleton(ref jointInfos, ref baseFrame, ref animFrameData, frame_index, anim.numJoints, ref anim);
                }
            }

            file.Close();

            anim.curFrame = 0;
            anim.nextFrame = 1;

            anim.lastTime = 0;
            anim.maxTime = 1.0f / anim.frameRate;

            /* Allocate memory for animated skeleton */
            skeleton = new MD5Joint[anim.numJoints];
            animated = true;

            Vector3 min = new Vector3(9999, 9999, 9999);
            Vector3 max = new Vector3(-9999, -9999, -9999);
            // laske max bboxit
            for (int q = 0; q < anim.numFrames; q++)
            {
                if (anim.bboxes[q].min.X < min.X) min.X = anim.bboxes[q].min.X;
                if (anim.bboxes[q].min.Y < min.Y) min.Y = anim.bboxes[q].min.Y;
                if (anim.bboxes[q].min.Z < min.Z) min.Z = anim.bboxes[q].min.Z;

                if (anim.bboxes[q].max.X > max.X) max.X = anim.bboxes[q].max.X;
                if (anim.bboxes[q].max.Y > max.Y) max.Y = anim.bboxes[q].max.Y;
                if (anim.bboxes[q].max.Z > max.Z) max.Z = anim.bboxes[q].max.Z;
            }
            bounds.Mode = CSat.BoundingVolume.Sphere;
            bounds.CreateBoundingBox(min, max);

            CSat.Log.WriteDebugLine("Animation: " + fileName);
        }

        public struct Buffer
        {
            public string sbuffer;
            public int[] ibuffer;
            public float[] fbuffer;
        }

        public static void Cleanstring(ref string str)
        {
            str = str.Replace(".", ",");
            str = str.Replace("\t", " ");
            str = str.Replace("\"", " ");
            str = str.Trim();
            str = str.Replace("     ", " ");
            str = str.Replace("    ", " ");
            str = str.Replace("   ", " ");
            str = str.Replace("  ", " ");
        }

        public static bool ParseLine(ref Buffer t, string ln, string str)
        {
            int typef = 0, typed = 0, i;
            Cleanstring(ref ln);
            ln = ln.ToLower();
            str = str.ToLower();
            string[] splitln = ln.Split(new Char[] { ' ' });
            string[] splitstr = str.Split(new Char[] { ' ' });
            for (i = 0; i < str.Length - 1; i++)
            {
                if (str[i] == '%' && str[i + 1] == 'd')
                    typed++;
                if (str[i] == '%' && str[i + 1] == 'f')
                    typef++;
            }
            t.ibuffer = new int[typed];
            t.fbuffer = new float[typef];
            typef = 0;
            typed = 0;
            for (i = 0; i < splitstr.Length; i++)
            {
                if (string.Equals(splitstr[i], "%d")) //integer
                {
                    t.ibuffer[typed] = int.Parse(splitln[i]);
                    typed++;
                }
                else if (string.Equals(splitstr[i], "%f")) //double
                {
                    t.fbuffer[typef] = float.Parse(splitln[i]);
                    typef++;
                }
                else if (string.Equals(splitstr[i], "%s")) //string
                    t.sbuffer = splitln[i];
                else if (!string.Equals(splitstr[i], splitln[i]))
                    return false;
            }
            return true;
        }


    }
}
