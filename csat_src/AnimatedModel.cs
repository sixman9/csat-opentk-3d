#region --- License ---
/* 
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
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
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSat
{
    public class AnimatedModel : Mesh
    {
        private bool animated = false;
        private int numJoints;
        private int numMesh;
        private int meshCount;
        private int numOfFaces;
        private Vector3[] finalVert;
        private Vector3[] normals;
        private List<Vertex[]> meshes;

        public AnimatedModel(string name, string fileName)
        {
            Name = name;
            Load(fileName);
        }

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
            public Texture texture;
            public int numVert;
            public int numTris;
            public int numWeights;
            public MD5Vertex[] verts;
            public int[][] faces;
            public MD5Weight[] weights;

            public VBO vbo;
        }
        private MD5Mesh[] model;

        private MD5Joint[] baseSkel;

        /// <summary>
        /// Joint info
        /// </summary>
        struct MD5JointInfo
        {
            public string name;
            public int parent;
            public int flags;
            public int startIndex;
        };

        /// <summary>
        /// Base frame joint
        /// </summary>
        struct MD5BaseFrameJoint
        {
            public Vector3 pos;
            public Quaternion orient;
        };

        public override void Dispose()
        {
            for (int q = 0; q < model.Length; q++)
            {
                model[q].texture.Dispose();
                model[q].vbo.Dispose();
            }
        }

        private string ext = "";
        /// <summary>
        /// jos shaderin päätettä ei ole md5 tiedostoissa, voi pistää tässä, eli esim .jpg, .tga, .dds ..
        /// </summary>
        /// <param name="ext"></param>
        public void UseExt(string ext)
        {
            this.ext = ext;
        }

        /// <summary>
        /// lataa md5 model
        /// </summary>
        /// <param name="fileName"></param>
        public void Load(string fileName)
        {
            // Initialize everything
            string line, textureName = "";
            int i;
            Buffer t = new Buffer();
            meshCount = 0;
            using (System.IO.StreamReader file = new System.IO.StreamReader(Settings.DataDir + fileName))
            {
                {
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

                            meshes = new List<Vertex[]>(numMesh);
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
                                baseSkel[i].orient = new Quaternion(t.fbuffer[3], t.fbuffer[4], t.fbuffer[5], 1);
                                /*
jotain tosi outoa..
    toi orient homma ei skulaa..
        vaik varaa tilaa new:llä, ei toimi..
            eikä  orient.X = 1;  eikä mikään. miks??
                */
                                MathExt.ComputeW(ref baseSkel[i].orient);
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

                                    textureName = Settings.DataDir + dir + textureName;
                                    textureName = textureName.Replace(",", ".");
                                }

                                // Read mesh data
                                if (ParseLine(ref t, line, "numverts %d"))
                                {
                                    model[meshCount].numVert = t.ibuffer[0];
                                    model[meshCount].verts = new MD5Vertex[model[meshCount].numVert];

                                    model[meshCount].texture = Texture.Load(textureName, false);

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
                }
            }

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

            for (int q = 0; q < numMesh; q++)
            {
                Vertex[] v = meshes[q];
                int[] ind = new int[v.Length];
                for (int w = 0; w < ind.Length; w++) ind[w] = w;

                // luo vbo ja datat sinne
                model[q].vbo = new VBO(BufferUsageHint.DynamicDraw);
                model[q].vbo.DataToVBO(v, ind);
            }
            material = new Material("default");
            MaterialName = "default";

            Log.WriteDebugLine("Model: " + Name);
        }

        public void UpdateNormals()
        {
            updateNormalsCount = FramesBetweenNormalsUpdate;
        }
        public void UpdateAnimation()
        {
            updateAnimCount = FramesBetweenAnimUpdate;
        }

        /// <summary>
        /// Prepare mesh for rendering
        /// </summary>
        void PrepareMesh()
        {
            int i, j, k;
            meshes.Clear();

            // Calculate the final Position ingame Position of all the model vertexes
            for (k = 0; k < numMesh; k++)
            {
                numOfFaces = model[k].numTris;
                vertices = new Vertex[numOfFaces * 3];

                for (i = 0; i < model[k].numVert; i++)
                {
                    Vector3 finalVertex = new Vector3(0, 0, 0);
                    for (j = 0; j < model[k].verts[i].countw; j++)
                    {
                        MD5Weight wt = model[k].weights[model[k].verts[i].startw + j];
                        MD5Joint joint = skeleton[wt.joint];

                        Vector3 wv = MathExt.RotatePoint(ref joint.orient, ref wt.pos);
                        finalVertex.X += (joint.pos.X + wv.X) * wt.bias;
                        finalVertex.Y += (joint.pos.Y + wv.Y) * wt.bias;
                        finalVertex.Z += (joint.pos.Z + wv.Z) * wt.bias;
                    }
                    finalVert[i] = finalVertex;
                }

                // aika laskea normaalit uudelleen?
                if (updateNormalsCount == FramesBetweenNormalsUpdate)
                {
                    MathExt.CalcNormals(ref finalVert, ref model[k].faces, ref normals, false);
                    updateNormalsCount = 0;
                }
                else updateAnimCount++;

                int count = 0;
                // Organize the final vertexes acording to the meshes triangles
                for (i = 0; i < model[k].numTris; i++)
                {
                    vertices[count] = new Vertex(finalVert[(int)model[k].faces[i][0]], normals[(int)model[k].faces[i][0]], model[k].verts[(int)model[k].faces[i][0]].uv);
                    vertices[count + 1] = new Vertex(finalVert[(int)model[k].faces[i][1]], normals[(int)model[k].faces[i][1]], model[k].verts[(int)model[k].faces[i][1]].uv);
                    vertices[count + 2] = new Vertex(finalVert[(int)model[k].faces[i][2]], normals[(int)model[k].faces[i][2]], model[k].verts[(int)model[k].faces[i][2]].uv);

                    count += 3;
                }
                meshes.Add(vertices);

                if (model[k].vbo != null) model[k].vbo.Update(vertices);
            }
        }

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
            material.SetMaterial(MaterialName);
            if (Shader != null) Shader.UseProgram();
            if (DoubleSided) GL.Disable(EnableCap.CullFace);

            for (int i = 0; i < model.Length; i++)
            {
                if (model[i].vbo == null) continue;

                model[i].texture.Bind();

                // lasketaanko uusi asento
                if (updateAnimCount == FramesBetweenAnimUpdate)
                {
                    // Interpolate skeletons between two frames
                    InterpolateSkeletons(ref curAnim.skelFrames, curAnim.curFrame, curAnim.nextFrame, curAnim.numJoints, curAnim.lastTime * curAnim.frameRate);
                    PrepareMesh();
                    updateAnimCount = 0;
                }
                else updateAnimCount++;

                model[i].vbo.BeginRender();
                model[i].vbo.Render();
                model[i].vbo.EndRender();
                Settings.NumOfObjects++;
            }

            if (DoubleSided) GL.Enable(EnableCap.CullFace);
            if (Shader != null) Shader.RemoveProgram();
        }



        /******************************************************************************************
        // animation code -------------------------------------------------------------------------
        ******************************************************************************************/
        List<Animation> animations = new List<Animation>();

        Animation curAnim;

        MD5Joint[] skeleton = null;

        /// <summary>
        /// monenko framen jälkeen lasketaan uusi asento
        /// </summary>
        public int FramesBetweenAnimUpdate = 1;
        int updateAnimCount = 0;
        /// <summary>
        /// monenko framen välein päivitetään normaalit
        /// </summary>
        public int FramesBetweenNormalsUpdate = 3;
        int updateNormalsCount = 0;

        /// <summary>
        /// aseta haluttu animaatio
        /// </summary>
        /// <param name="animName"></param>
        public override void UseAnimation(string animName)
        {
            if (curAnim.animName != animName)
            {
                for (int q = 0; q < animations.Count; q++)
                {
                    if (animations[q].animName == animName)
                    {
                        curAnim = animations[q];
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// luo luuranko.
        /// </summary>
        /// <param name="jointInfos"></param>
        /// <param name="baseFrame"></param>
        /// <param name="animFrameData"></param>
        /// <param name="frameIndex"></param>
        /// <param name="num_joints"></param>
        /// <param name="md5anim"></param>
        void BuildFrameSkeleton(ref MD5JointInfo[] jointInfos,
                                ref MD5BaseFrameJoint[] baseFrame,
                                ref float[] animFrameData,
                                int frameIndex,
                                int num_joints, ref Animation md5anim)
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
                    animatedOrient.X = animFrameData[jointInfos[i].startIndex + j];
                    ++j;
                }

                if ((jointInfos[i].flags & 16) > 0) /* Qy */
                {
                    animatedOrient.Y = animFrameData[jointInfos[i].startIndex + j];
                    ++j;
                }

                if ((jointInfos[i].flags & 32) > 0) /* Qz */
                {
                    animatedOrient.Z = animFrameData[jointInfos[i].startIndex + j];
                    ++j;
                }

                /* Compute orient quaternion's w value */
                MathExt.ComputeW(ref animatedOrient);

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
                    Vector3 rpos; /* Rotated Position */

                    /* Add positions */
                    rpos = MathExt.RotatePoint(ref parentJoint.orient, ref animatedPos);

                    md5anim.skelFrames[frameIndex, i].pos.X = rpos.X + parentJoint.pos.X;
                    md5anim.skelFrames[frameIndex, i].pos.Y = rpos.Y + parentJoint.pos.Y;
                    md5anim.skelFrames[frameIndex, i].pos.Z = rpos.Z + parentJoint.pos.Z;

                    /* Concatenate rotations */
                    md5anim.skelFrames[frameIndex, i].orient = MathExt.Mult(ref parentJoint.orient, ref animatedOrient);
                    md5anim.skelFrames[frameIndex, i].orient = MathExt.Normalize(ref md5anim.skelFrames[frameIndex, i].orient);
                }

            }
        }

        /// <summary>
        /// laske luurangolle asento.
        /// </summary>
        /// <param name="skel"></param>
        /// <param name="curFrame"></param>
        /// <param name="nextFrame"></param>
        /// <param name="num_joints"></param>
        /// <param name="interp"></param>
        void InterpolateSkeletons(ref MD5Joint[,] skel, int curFrame, int nextFrame,
                                  int num_joints, float interp)
        {
            int i;

            for (i = 0; i < num_joints; ++i)
            {
                /* Copy parent index */
                skeleton[i].parent = skel[curFrame, i].parent;

                /* Linear interpolation for Position */
                skeleton[i].pos.X = skel[curFrame, i].pos.X + interp * (skel[nextFrame, i].pos.X - skel[curFrame, i].pos.X);
                skeleton[i].pos.Y = skel[curFrame, i].pos.Y + interp * (skel[nextFrame, i].pos.Y - skel[curFrame, i].pos.Y);
                skeleton[i].pos.Z = skel[curFrame, i].pos.Z + interp * (skel[nextFrame, i].pos.Z - skel[curFrame, i].pos.Z);

                /* Spherical linear interpolation for orientation */
                skeleton[i].orient = MathExt.Slerp(ref skel[curFrame, i].orient, ref skel[nextFrame, i].orient, interp);
            }
        }

        /// <summary>
        /// lasketaan frame
        /// </summary>
        /// <param name="anim"></param>
        /// <param name="dt"></param>
        void Animate(ref Animation anim, float dt)
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


        public override void Update(float time)
        {
            if (animated)
            {
                /* Calculate current and next frames */
                Animate(ref curAnim, time);
            }
            else
            {
                /* No animation, use bind-pose skeleton */
                skeleton = baseSkel;
            }

        }

        /// <summary>
        /// lataa md5-animaatio.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="anim"></param>
        public override void LoadAnim(string animName, string fileName)
        {
            if (fileName == null || fileName == "") return;

            Animation anim = new Animation();
            anim.animName = animName;

            Buffer t = new Buffer();
            MD5JointInfo[] jointInfos = null;
            MD5BaseFrameJoint[] baseFrame = null;
            float[] animFrameData = null;
            int numAnimatedComponents = 0;
            int frame_index;
            int i;

            using (System.IO.StreamReader file = new System.IO.StreamReader(Settings.DataDir + fileName))
            {
                string line;
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
                                baseFrame[i].orient.X = t.fbuffer[3];
                                baseFrame[i].orient.Y = t.fbuffer[4];
                                baseFrame[i].orient.Z = t.fbuffer[5];

                                /* Compute the w component */
                                MathExt.ComputeW(ref baseFrame[i].orient);

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

                anim.curFrame = 0;
                anim.nextFrame = 1;

                anim.lastTime = 0;
                anim.maxTime = 1.0f / anim.frameRate;

                /* Allocate memory for animated skeleton */
                skeleton = new MD5Joint[anim.numJoints];
                animated = true;

                Vector3 min = new Vector3(9999, 9999, 9999);
                Vector3 max = new Vector3(-9999, -9999, -9999);

                // laske bboxit
                for (int q = 0; q < anim.numFrames; q++)
                {
                    if (anim.bboxes[q].min.X < min.X) min.X = anim.bboxes[q].min.X;
                    if (anim.bboxes[q].min.Y < min.Y) min.Y = anim.bboxes[q].min.Y;
                    if (anim.bboxes[q].min.Z < min.Z) min.Z = anim.bboxes[q].min.Z;

                    if (anim.bboxes[q].max.X > max.X) max.X = anim.bboxes[q].max.X;
                    if (anim.bboxes[q].max.Y > max.Y) max.Y = anim.bboxes[q].max.Y;
                    if (anim.bboxes[q].max.Z > max.Z) max.Z = anim.bboxes[q].max.Z;
                }

                Boundings = new BoundingVolume();
                Boundings.CreateBoundingVolume(this, min, max);

                Update(0);
                animations.Add(anim);

                Log.WriteDebugLine("Animation: " + fileName);

                UseAnimation(animName);
            }
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


    /// <summary>
    /// Animation data
    /// </summary>
    public struct Animation
    {
        public string animName;
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
    public struct MD5Joint
    {
        public string name;
        public int parent;
        public Vector3 pos;
        public Quaternion orient;
    }
    /// <summary>
    /// Bounding box
    /// </summary>
    public struct MD5BoundingBox
    {
        public Vector3 min;
        public Vector3 max;
    };


}
