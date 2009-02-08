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

// Normaalien laskut ym ja lisää quaternion metodeita.

using System;
using OpenTK.Math;

namespace CSat
{
    public class MathExt
    {
        public static readonly float PiOver180 = (float)(Math.PI / 180);


        #region --- Calc plane, normals.. ---
        public static Vector3 VectorMatrixMult(ref Vector3 vec, ref Matrix4 mat)
        {
            Vector3 outv;
            outv.X = vec.X * mat.Row0.X + vec.Y * mat.Row0.Y + vec.Z * mat.Row0.Z;
            outv.Y = vec.X * mat.Row1.X + vec.Y * mat.Row1.Y + vec.Z * mat.Row1.Z;
            outv.Z = vec.X * mat.Row2.X + vec.Y * mat.Row2.Y + vec.Z * mat.Row2.Z;
            return outv;
        }

        public static void CalcPlane(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3, out Vector4 outv)
        {
            outv.X = (v1.Y * (v2.Z - v3.Z)) + (v2.Y * (v3.Z - v1.Z)) + (v3.Y * (v1.Z - v2.Z));
            outv.Y = (v1.Z * (v2.X - v3.X)) + (v2.Z * (v3.X - v1.X)) + (v3.Z * (v1.X - v2.X));
            outv.Z = (v1.X * (v2.Y - v3.Y)) + (v2.X * (v3.Y - v1.Y)) + (v3.X * (v1.Y - v2.Y));
            outv.W = -((v1.X * ((v2.Y * v3.Z) - (v3.Y * v2.Z))) + (v2.X * ((v3.Y * v1.Z) - (v1.Y * v3.Z))) + (v3.X * ((v1.Y * v2.Z) - (v2.Y * v1.Z))));
        }

        /// <summary>
        /// laske tasolle normaali
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <param name="outv"></param>
        public static void CalcNormal(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3, out Vector3 outv)
        {
            float RelX1 = v2.X - v1.X;
            float RelY1 = v2.Y - v1.Y;
            float RelZ1 = v2.Z - v1.Z;
            float RelX2 = v3.X - v1.X;
            float RelY2 = v3.Y - v1.Y;
            float RelZ2 = v3.Z - v1.Z;
            outv.X = (RelY1 * RelZ2) - (RelZ1 * RelY2);
            outv.Y = (RelZ1 * RelX2) - (RelX1 * RelZ2);
            outv.Z = (RelX1 * RelY2) - (RelY1 * RelX2);
        }

        /// <summary>
        /// laske vertexnormaalit
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="faces"></param>
        /// <param name="normals"></param>
        /// <param name="flipNormals"></param>
        public static void CalcNormals(ref Vector3[] pos, ref int[][] faces, ref Vector3[] normals, bool flipNormals)
        {
            int q, w, count = 0;
            Vector3 outv = new Vector3();
            Vector3 c = new Vector3();
            float len;

            for (q = 0; q < pos.Length; q++)
            {
                c.X = c.Y = c.Z = 0;

                for (w = 0; w < faces.Length; w++)
                {
                    // jos vertex on tässä kolmiossa
                    if ((faces[w][0] == q) || (faces[w][1] == q) || (faces[w][2] == q))
                    {
                        CalcNormal(ref pos[faces[w][0]], ref pos[faces[w][1]], ref pos[faces[w][2]], out outv);

                        len = outv.LengthSquared; // outv.Length;

                        if (len == 0)
                        {
                            len = -1.0f;
                        }

                        c += (outv / len);
                        count++;
                    }

                    if (count > 0)
                    {
                        // laske vektorin pituus
                        len = c.LengthSquared; // c.Length;

                        if (flipNormals == true)
                        {
                            len = -len;
                        }

                        if (len == 0)
                        {
                            len = -1.0f;
                        }

                        normals[q] = new Vector3();
                        normals[q] = (c / len);

                        if (len != -1)
                        {
                            normals[q].Normalize();
                        }
                    }
                }
            }
        }

        #endregion

        // quaternion code:
        #region --- Quaternion 2 ---
        public static void ComputeW(ref Quaternion q)
        {
            float t = 1.0f - (q.XYZ.X * q.XYZ.X) - (q.XYZ.Y * q.XYZ.Y) - (q.XYZ.Z * q.XYZ.Z);
            if (t < 0.0f) q.W = 0.0f;
            else q.W = (float)-Math.Sqrt(t);
        }

        public static Vector3 RotatePoint(ref Quaternion q, ref Vector3 v)
        {
            Vector3 outv;
            Quaternion inv;

            // invert
            inv.XYZ.X = -q.XYZ.X;
            inv.XYZ.Y = -q.XYZ.Y;
            inv.XYZ.Z = -q.XYZ.Z;
            inv.W = q.W;

            Quaternion norminv = Quaternion.Normalize(inv);
            Quaternion m = MultVec(ref q, ref v);
            Quaternion qm = MathExt.Mult(ref m, ref norminv);

            outv.X = qm.XYZ.X;
            outv.Y = qm.XYZ.Y;
            outv.Z = qm.XYZ.Z;

            return outv;
        }

        public static Quaternion MultVec(ref Quaternion q, ref Vector3 v)
        {
            Quaternion outq;

            outq.W = -(q.XYZ.X * v.X) - (q.XYZ.Y * v.Y) - (q.XYZ.Z * v.Z);
            outq.XYZ.X = ((q.W * v.X) + (q.XYZ.Y * v.Z)) - (q.XYZ.Z * v.Y);
            outq.XYZ.Y = ((q.W * v.Y) + (q.XYZ.Z * v.X)) - (q.XYZ.X * v.Z);
            outq.XYZ.Z = ((q.W * v.Z) + (q.XYZ.X * v.Y)) - (q.XYZ.Y * v.X);

            return outq;
        }

        public static float DotProduct(ref Quaternion qa, ref Quaternion qb)
        {
            return ((qa.XYZ.X * qb.XYZ.X) + (qa.XYZ.Y * qb.XYZ.Y) + (qa.XYZ.Z * qb.XYZ.Z) + (qa.W * qb.W));
        }

        public static Quaternion Slerp(ref Quaternion qa, ref Quaternion qb, float t)
        {
            Quaternion outr = new Quaternion();

            // check for out-of range parameter and return edge points if so
            if (t <= 0.0)
            {
                return qa;
            }

            if (t >= 1.0)
            {
                return qb;
            }

            // compute "cosine of angle between quaternions" using dot product
            float cosOmega = DotProduct(ref qa, ref qb);

            // if negative dot, use -q1. two quaternions q and -q
            // represent the same Rotation, but may produce
            // different slerp. we chose q or -q to rotate using
            // the acute angle.
            float q1w = qb.W;
            float q1x = qb.XYZ.X;
            float q1y = qb.XYZ.Y;
            float q1z = qb.XYZ.Z;

            if (cosOmega < 0.0f)
            {
                q1w = -q1w;
                q1x = -q1x;
                q1y = -q1y;
                q1z = -q1z;
                cosOmega = -cosOmega;
            }

            // we should have two unit quaternions, so dot should be <= 1.0
            // assert( cosOmega < 1.1f );
            if (cosOmega >= 1.1f)
            {
                Log.WriteDebugLine("Quaternion error: Slerp");
            }

            // compute interpolation fraction, checking for quaternions
            // almost exactly the same
            float k0;

            // compute interpolation fraction, checking for quaternions
            // almost exactly the same
            float k1;

            if (cosOmega > 0.9999f)
            {
                // very close - just use linear interpolation,
                // which will protect againt a divide by zero
                k0 = 1.0f - t;
                k1 = t;
            }
            else
            {
                // compute the sin of the angle using the
                // trig identity sin^2(omega) + cos^2(omega) = 1
                float sinOmega = (float)Math.Sqrt(1.0f - (cosOmega * cosOmega));

                // compute the angle from its sin and cosine
                float omega = (float)Math.Atan2(sinOmega, cosOmega);

                // compute inverse of denominator, so we only have to divide
                // once
                float oneOverSinOmega = 1.0f / sinOmega;

                // Compute interpolation parameters
                k0 = (float)Math.Sin((1.0f - t) * omega) * oneOverSinOmega;
                k1 = (float)Math.Sin(t * omega) * oneOverSinOmega;
            }

            // interpolate and return new quaternion
            outr.W = (k0 * qa.W) + (k1 * q1w);
            outr.XYZ.X = (k0 * qa.XYZ.X) + (k1 * q1x);
            outr.XYZ.Y = (k0 * qa.XYZ.Y) + (k1 * q1y);
            outr.XYZ.Z = (k0 * qa.XYZ.Z) + (k1 * q1z);

            return outr;
        }


        public static Quaternion Mult(ref Quaternion qb, ref Quaternion qa)
        {
            Quaternion outq = new Quaternion();

            outq.W = (qb.W * qa.W) - (qb.XYZ.X * qa.XYZ.X) - (qb.XYZ.Y * qa.XYZ.Y) - (qb.XYZ.Z * qa.XYZ.Z);
            outq.XYZ.X = ((qb.W * qa.XYZ.X) + (qb.XYZ.X * qa.W) + (qb.XYZ.Y * qa.XYZ.Z)) - (qb.XYZ.Z * qa.XYZ.Y);
            outq.XYZ.Y = ((qb.W * qa.XYZ.Y) + (qb.XYZ.Y * qa.W) + (qb.XYZ.Z * qa.XYZ.X)) - (qb.XYZ.X * qa.XYZ.Z);
            outq.XYZ.Z = ((qb.W * qa.XYZ.Z) + (qb.XYZ.Z * qa.W) + (qb.XYZ.X * qa.XYZ.Y)) - (qb.XYZ.Y * qa.XYZ.X);

            return outq;
        }

        public static Quaternion Normalize(ref Quaternion q)
        {
            /* compute magnitude of the quaternion */
            float mag = (float)Math.Sqrt((q.XYZ.X * q.XYZ.X) + (q.XYZ.Y * q.XYZ.Y) + (q.XYZ.Z * q.XYZ.Z) + (q.W * q.W));

            /* check for bogus length, to protect against divide by zero */
            if (mag > 0.0f)
            {
                /* normalize it */
                float oneOverMag = 1.0f / mag;

                q.XYZ.X *= oneOverMag;
                q.XYZ.Y *= oneOverMag;
                q.XYZ.Z *= oneOverMag;
                q.W *= oneOverMag;
            }
            return q;
        }
        #endregion

    }
}
