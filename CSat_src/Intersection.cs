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
        static float EPSILON = 0.00001f;
        public static Vector3 intersection;
        public static float u, v, t;

        /// <summary>
        /// tarkista osuuko start->end vektori johonkin polyyn obj -objektissa. palauttaa true jos osuu, muuten false.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool CheckIntersection(ref Vector3 start, ref Vector3 end, ref Object3D obj)
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
            for (int q = 0; q < obj.Objects.Count; q++)
            {
                Object3D child = (Object3D)obj.Objects[q];
                if (CheckIntersection(ref start, ref end, ref child, ref obj.Position, ref rot, ref outm) == true) return true;
            }
            return false;
        }

        /// <summary>
        /// tarkista osuuko start->end vektori johonkin polyyn. palauttaa true jos osuu, muuten false.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="obj"></param>
        /// <param name="Position"></param>
        /// <param name="Rotation"></param>
        /// <param name="Matrix"></param>
        /// <returns></returns>
        public static bool CheckIntersection(ref Vector3 start, ref Vector3 end, ref Object3D obj, ref Vector3 position, ref Vector3 rotation, ref Matrix4 matrix)
        {
            Vector3 dir = new Vector3();
            dir = end - start;

            // vektorin pituus
            float len = dir.Length;
            dir.Normalize();
            //len *= 2;

            for (int e = 0; e < obj.VertexInd.Count / 3; e++)
            {
                int i = e * 3;
                // tarkista kolmio
                Vector3 v1 = obj.Vertex[(int)obj.VertexInd[i + 0]];
                Vector3 v2 = obj.Vertex[(int)obj.VertexInd[i + 1]];
                Vector3 v3 = obj.Vertex[(int)obj.VertexInd[i + 2]];

                Vector3 vout;
                if (Math.Abs(rotation.X + rotation.Y + rotation.Z) > 0.001f)
                {
                    vout = MathExt.VectorMatrixMult(ref v1, ref matrix);
                    v1 = vout;

                    vout = MathExt.VectorMatrixMult(ref v2, ref matrix);
                    v2 = vout;

                    vout = MathExt.VectorMatrixMult(ref v3, ref matrix);
                    v3 = vout;
                }
                v1 = v1 + position;
                v2 = v2 + position;
                v3 = v3 + position;

                if (IntersectTriangle(ref start, ref dir, ref v1, ref v2, ref v3) == true)
                {
                    if (Math.Abs(t) > len) continue;
                    return true;
                }
            }
            return false;
        }

        static Vector3 edge1 = new Vector3();
        static Vector3 edge2 = new Vector3();
        static Vector3 tvec = new Vector3();
        static Vector3 pvec = new Vector3();
        static Vector3 qvec = new Vector3();

        public static bool IntersectTriangle(ref Vector3 orig, ref Vector3 dir, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            float det, inv_det;

            // find vectors for two edges sharing vert0
            edge1 = v1 - v0;
            edge2 = v2 - v0;

            // begin calculating determinant - also used to calculate U parameter
            pvec = Vector3.Cross(dir, edge2);

            // if determinant is near zero, ray lies in plane of triangle
            det = Vector3.Dot(edge1, pvec);

            if (det < EPSILON)
            {
                return false;
            }

            // calculate distance from vert0 to ray origin
            tvec = orig - v0;

            // calculate U parameter and test bounds
            u = Vector3.Dot(tvec, pvec);
            if (u < 0.0 || u > det)
            {
                return false;
            }

            // prepare to test V parameter
            qvec = Vector3.Cross(tvec, edge1);

            // calculate V parameter and test bounds
            v = Vector3.Dot(dir, qvec);
            if (v < 0.0 || u + v > det)
            {
                return false;
            }

            // calculate t, scale parameters, ray intersects triangle
            t = Vector3.Dot(edge2, qvec);
            inv_det = 1.0f / det;

            u *= inv_det;
            v *= inv_det;
            t *= inv_det;

            intersection = v0 + (edge1 * u) + (edge2 * v);

            return true;
        }

        /// <summary>
        /// palauttaa true jos vektori oldpos->newpos välissä poly worldissa. sopii esim kameralle
        /// doNotTestThis - objekti jota ei testata (eli liikuteltava objekti tai null jos esim kamera)
        /// </summary>
        /// <param name="world"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="doNotTestThis"></param>
        /// <returns></returns>
        public static bool CheckCollision(ref Group world, Vector3 start, Vector3 end, ref Object3D doNotTestThis)
        {
            Vector3 len = start - end;
            if (len.X == 0 && len.Y == 0 && len.Z == 0) return false;

            for (int q = 0; q < world.Objects.Count; q++)
            {
                if (world.Objects[q] is Object3D)
                {
                    if (world.Objects[q] != doNotTestThis)
                    {
                        Object3D ob = (Object3D)world.Objects[q];
                        if (Intersection.CheckIntersection(ref start, ref end, ref ob) == true) return true;
                    }
                }

            }
            return false;
        }

        /// <summary>
        /// palauttaa true jos objektin boundingboxin joku vertexi osuu johonkin polyyn worldissa
        /// </summary>
        /// <param name="world"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool CheckCollisionBB(ref Group world, Vector3 start, Vector3 end, ref Object3D obj)
        {
            Vector3 len = start - end;
            if (len.X == 0 && len.Y == 0 && len.Z == 0) return false;

            for (int q = 0; q < world.Objects.Count; q++)
            {
                if (world.Objects[q] is Object3D) // todo  animatedmodelia ei tsekata, vielä
                {
                    if (world.Objects[q] != obj)
                    {
                        // jos objekti käännetty (Rotation ja/tai FixRotation != 0), pitää ottaa huomioon ja kääntää bounding boxia.
                        Matrix4 outm = new Matrix4();
                        Vector3 rot = -(obj.Rotation + obj.FixRotation);
                        if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
                        {
                            rot = rot * MathExt.PiOver180;
                            Matrix4 mx = Matrix4.RotateX(rot.X);
                            Matrix4 my = Matrix4.RotateY(rot.Y);
                            Matrix4 mz = Matrix4.RotateZ(rot.Z);
                            Matrix4 outm0;
                            Matrix4.Mult(ref mx, ref my, out outm0);
                            Matrix4.Mult(ref outm0, ref mz, out outm);
                        }
                        // tarkistetaan bounding boxin kulmat, yrittääkö läpäistä jonkun polyn
                        for (int c = 0; c < 8; c++)
                        {
                            //Vector3 v = obj.MeshBoundingVolume.Corner[c];
                            Vector3 v = obj.ObjBoundingVolume.Corner[c];

                            Vector3 vout;
                            if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
                            {
                                vout = MathExt.VectorMatrixMult(ref v, ref outm);
                            }
                            else vout = v;

                            vout = vout + obj.Position;
                            Vector3 endv = vout + len;

                            Object3D ob = (Object3D)world.Objects[q];
                            if (Intersection.CheckIntersection(ref vout, ref endv, ref ob) == true)
                            {
                                return true;
                            }
                        }

                    }
                }
            }
            return false;
        }

    }
}
