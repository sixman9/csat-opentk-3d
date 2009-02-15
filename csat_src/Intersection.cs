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
 * tutti:
 * http://nehe.gamedev.net/data/lessons/lesson.asp?lesson=30
 * http://www.gamedev.net/reference/articles/article1026.asp
 * http://jgt.akpeters.com/papers/MollerTrumbore97/
 *
 */
using System;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public class Intersection
    {
        static float Epsilon = 0.00001f;
        public static float U, V, T;

        /// <summary>
        /// leikkauskohta 3d-maailmassa
        /// </summary>
        public static Vector3 IntersectionPoint;

        /// <summary>
        /// kuinka lähelle objektia päästään
        /// </summary>
        public static float DistAdder = 1.0f;

        /// <summary>
        /// tarkista osuuko start->end vektori johonkin polyyn mesh -objektissa. palauttaa true jos osuu, muuten false.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static bool CheckIntersection(ref Vector3 start, ref Vector3 end, ref Mesh obj)
        {
            // jos objektia käännetty
            Matrix4 outm = new Matrix4();
            Vector3 rot = -(obj.Rotation + obj.FixRotation); 
            if (rot.X != 0 || rot.Y != 0 || rot.Z != 0)
            {
                rot = rot * MathExt.PiOver180;
                Matrix4 mx = Matrix4.RotateX(rot.X);
                Matrix4 my = Matrix4.RotateY(rot.Y);
                Matrix4 mz = Matrix4.RotateZ(rot.Z);
                Matrix4 outm0;
                Matrix4.Mult(ref mx, ref my, out outm0);
                Matrix4.Mult(ref outm0, ref mz, out outm);
            }
            if (CheckIntersection(ref start, ref end, ref obj, ref rot, ref outm) == true) return true;

            return false;
        }
        /// <summary>
        /// tarkista osuuko start->end vektori johonkin polyyn. palauttaa true jos osuu, muuten false.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static bool CheckIntersection(ref Vector3 start, ref Vector3 end, ref Mesh obj, ref Vector3 rotation, ref Matrix4 matrix)
        {
            Vector3 position = obj.Position;
            Vector3 dir = new Vector3();
            dir = end - start;

            // vektorin pituus
            float len = dir.Length + DistAdder;
            dir.Normalize();
            Vector3[] v = new Vector3[3];

            for (int e = 0; e < obj.Vertices.Length; e += 3)
            {
                v[0] = obj.Vertices[e].vertex;
                v[1] = obj.Vertices[e + 1].vertex;
                v[2] = obj.Vertices[e + 2].vertex;

                Vector3 vout;
                if (Math.Abs(rotation.X + rotation.Y + rotation.Z) > 0.001f) 
                {
                    vout = MathExt.VectorMatrixMult(ref v[0], ref matrix);
                    v[0] = vout;

                    vout = MathExt.VectorMatrixMult(ref v[1], ref matrix);
                    v[1] = vout;

                    vout = MathExt.VectorMatrixMult(ref v[2], ref matrix);
                    v[2] = vout;
                }
                // TODO: huono ja hidas tapa, käännetään joka polygoni. FIX

                v[0] += position;
                v[1] += position;
                v[2] += position;

                if (IntersectTriangle(ref start, ref dir, ref v[0], ref v[1], ref v[2]) == true)
                {
                    if (Math.Abs(T) > len) continue;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// palauttaa true jos objektin boundingbox osuu toisen objektin boundingboxiin
        /// </summary>
        /// <param name="group"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="mesh"></param>
        /// <returns></returns>
        /*public static bool CheckCollisionBB_BB(ref Node group, Vector3 start, Vector3 end, ref Mesh obj)
        {
            // TODO: collision bb_bb
            return false;
        }*/

        /// <summary>
        /// palauttaa true jos objektin boundingboxin joku kulma osuu johonkin polyyn groupissa
        /// </summary>
        /// <param name="group"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static bool CheckCollisionBB_Poly(ref Node group, Vector3 start, Vector3 end, ref Mesh obj)
        {
            Vector3 len = start - end;
            if (Math.Abs(len.X + len.Y + len.Z) < Epsilon) return false;

            // tarkista objektin bbox
            if (CheckBB_Poly(ref group, len, ref obj, ref obj) == true) return true;

            // ei osunut joten tsekataan joka meshin bbox erikseen.
            //TODO: --liian hidas
            /*for (int q = 0; q < obj.Meshes().Count; q++)
            {
                Mesh m = obj.Meshes()[q];
                if (CheckBB_Poly_Rec(ref group, start, end, len, ref m, ref obj ) == true) return true;
            }*/

            return false;
        }

        /// <summary>
        /// tarkistaa meshin bboxin törmäyksen groupissa oleviin objekteihin. meshillä ei välttämättä ole Positionnia, joten obj -objektia
        /// käytetään antamaan paikka ja tarkistukseen ettei tarkisteta törmäystä itteensä.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="len"></param>
        /// <param name="mesh"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool CheckBB_Poly(ref Node group, Vector3 len, ref Mesh mesh, ref Mesh obj)
        {
            for (int q = 0; q < group.Objects.Length; q++)
            {
                if (group.Objects[q] is Mesh) // vain meshit tarkistetaan
                {
                    if (group.Objects[q] != mesh && group.Objects[q] != obj)
                    {
                        // tarkistetaan bounding boxin kulmat, yrittääkö läpäistä jonkun polyn
                        for (int c = 0; c < 8; c++)
                        {
                            Vector3 v = mesh.Boundings.Corner[c];
                            v += obj.Position; // huom. objektin position, koska meshillä ei välttämättä ole paikkaa.
                            Vector3 endv = v + len;
                            Mesh msh = (Mesh)group.Objects[q];
                            if (CheckIntersection(ref v, ref endv, ref msh) == true)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool IntersectTriangle(ref Vector3 orig, ref Vector3 dir, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            float det, inv_det;

            // find vectors for two edges sharing vert0
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;

            // begin calculating determinant - also used to calculate U parameter
            Vector3 pvec = Vector3.Cross(dir, edge2);

            // if determinant is near zero, ray lies in plane of triangle
            det = Vector3.Dot(edge1, pvec);

            if (det < Epsilon)
            {
                return false;
            }

            // calculate distance from vert0 to ray origin
            Vector3 tvec = orig - v0;

            // calculate U parameter and test bounds
            U = Vector3.Dot(tvec, pvec);
            if (U < 0.0 || U > det)
            {
                return false;
            }

            // prepare to test V parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);

            // calculate V parameter and test bounds
            V = Vector3.Dot(dir, qvec);
            if (V < 0.0 || U + V > det)
            {
                return false;
            }

            // calculate T, scale parameters, ray intersects triangle
            T = Vector3.Dot(edge2, qvec);
            inv_det = 1.0f / det;

            U *= inv_det;
            V *= inv_det;
            T *= inv_det;

            IntersectionPoint = v0 + (edge1 * U) + (edge2 * V);

            return true;
        }

    }
}
