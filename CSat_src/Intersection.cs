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
        /// tarkista osuuko start->end vektori johonkin polyyn obj -objektissa (Object3D/AnimatedModel). palauttaa true jos osuu, muuten false.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool CheckIntersection(ref Vector3 start, ref Vector3 end, ref Object obj)
        {
            ObjectInfo ob = (ObjectInfo)obj;

            // jos objektia k‰‰nnetty
            Matrix4 outm = new Matrix4();
            Vector3 rot = -(ob.Rotation + ob.FixRotation);
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
            for (int q = 0; q < ob.Objects.Count; q++)
            {
                Object child = ob.Objects[q];
                if (CheckIntersection(ref start, ref end, ref child, ref ob.Position, ref rot, ref outm) == true) return true;
            }
            return false;
        }

        /// <summary>
        /// tarkista osuuko start->end vektori johonkin polyyn. palauttaa true jos osuu, muuten false.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="obj"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static bool CheckIntersection(ref Vector3 start, ref Vector3 end, ref Object obj, ref Vector3 position, ref Vector3 rotation, ref Matrix4 matrix)
        {
            Vector3 dir = new Vector3();
            dir = end - start;

            // vektorin pituus
            float len = dir.Length;
            dir.Normalize();
            //len *= 2;

            Vector3[] v = new Vector3[3];
            ObjectInfo ob = (ObjectInfo)obj;
            int faces = 0;
            if (obj is Object3D) faces = ((Object3D)obj).GetNumOfTriangles();
            else faces = ((AnimatedModel)obj).GetNumOfTriangles();

            for (int e = 0; e < faces; e++)
            {
                if (obj is Object3D) ((Object3D)obj).GetTriangle(e, ref v);
                else ((AnimatedModel)obj).GetTriangle(e, ref v);

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
                v[0] += position;
                v[1] += position;
                v[2] += position;

                if (IntersectTriangle(ref start, ref dir, ref v[0], ref v[1], ref v[2]) == true)
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
        /// palauttaa true jos objektin boundingbox (Object3D/AnimatedModel) osuu toisen objektin boundingboxiin (Object3D/AnimatedModel) 
        /// </summary>
        /// <param name="world"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool CheckCollisionBB_BB(ref Group world, Vector3 start, Vector3 end, ref Object obj)
        {
            // todo
            return false;
        }

        /// <summary>
        /// palauttaa true jos objektin (Object3D/AnimatedModel) boundingboxin joku vertexi osuu johonkin polyyn worldissa
        /// </summary>
        /// <param name="world"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool CheckCollisionBB_Poly(ref Group world, Vector3 start, Vector3 end, ref Object obj)
        {
            Vector3 len = start - end;
            if (len.X == 0 && len.Y == 0 && len.Z == 0) return false;

            ObjectInfo ob = (ObjectInfo)obj;
            BoundingVolume bvol;
            if (obj is Object3D) bvol = ((Object3D)obj).GetBoundingVolume();
            else bvol = ((AnimatedModel)obj).GetBoundingVolume();

            for (int q = 0; q < world.Objects.Count; q++)
            {
                if (world.Objects[q] != obj)
                {
                    // tarkistetaan bounding boxin kulmat, yritt‰‰kˆ l‰p‰ist‰ jonkun polyn
                    for (int c = 0; c < 8; c++)
                    {
                        Vector3 v = bvol.Corner[c];
                        v += ob.Position;
                        Vector3 endv = v + len;

                        Object child = (Object)world.Objects[q];
                        if (CheckIntersection(ref v, ref endv, ref child) == true)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool CheckCollisionBB_Poly(ref Group world, ref Vector3 start, ref Vector3 end, ref Object3D obj)
        {
            Object ob = (Object)obj;
            return CheckCollisionBB_Poly(ref world, start, end, ref ob);
        }
        public static bool CheckCollisionBB_Poly(ref Group world, ref Vector3 start, ref Vector3 end, ref AnimatedModel obj)
        {
            Object ob = (Object)obj;
            return CheckCollisionBB_Poly(ref world, start, end, ref ob);
        }
        public static bool CheckIntersection(ref Vector3 start, ref Vector3 end, ref Object3D obj)
        {
            Object ob = (Object)obj;
            return CheckIntersection(ref start, ref end, ref ob);
        }
        public static bool CheckIntersection(ref Vector3 start, ref Vector3 end, ref AnimatedModel obj)
        {
            Object ob = (Object)obj;
            return CheckIntersection(ref start, ref end, ref ob);
        }

    }
}
