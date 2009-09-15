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
 * tutoriaali:
 * http://www.crownandcutlass.com/features/technicaldetails/frustum.html
 *
 */

using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSat
{
    public class Frustum
    {
        static float[,] frustum = new float[6, 4];
        const int RIGHT = 0, LEFT = 1, BOTTOM = 2, TOP = 3, BACK = 4, FRONT = 5;

        static void NormalizePlane(float[,] frustum, int side)
        {
            float magnitude = (float)Math.Sqrt((frustum[side, 0] * frustum[side, 0]) + (frustum[side, 1] * frustum[side, 1])
                                                + (frustum[side, 2] * frustum[side, 2]));

            frustum[side, 0] /= magnitude;
            frustum[side, 1] /= magnitude;
            frustum[side, 2] /= magnitude;
            frustum[side, 3] /= magnitude;
        }

        public static void CalculateFrustum()
        {
            // ota projection ja Modelview matriisit
            GL.GetFloat(GetPName.ProjectionMatrix, Util.ProjMatrix);
            GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);

            Util.ClipMatrix[0] = (Util.ModelMatrix[0] * Util.ProjMatrix[0]) + (Util.ModelMatrix[1] * Util.ProjMatrix[4]) + (Util.ModelMatrix[2] * Util.ProjMatrix[8]) + (Util.ModelMatrix[3] * Util.ProjMatrix[12]);
            Util.ClipMatrix[1] = (Util.ModelMatrix[0] * Util.ProjMatrix[1]) + (Util.ModelMatrix[1] * Util.ProjMatrix[5]) + (Util.ModelMatrix[2] * Util.ProjMatrix[9]) + (Util.ModelMatrix[3] * Util.ProjMatrix[13]);
            Util.ClipMatrix[2] = (Util.ModelMatrix[0] * Util.ProjMatrix[2]) + (Util.ModelMatrix[1] * Util.ProjMatrix[6]) + (Util.ModelMatrix[2] * Util.ProjMatrix[10]) + (Util.ModelMatrix[3] * Util.ProjMatrix[14]);
            Util.ClipMatrix[3] = (Util.ModelMatrix[0] * Util.ProjMatrix[3]) + (Util.ModelMatrix[1] * Util.ProjMatrix[7]) + (Util.ModelMatrix[2] * Util.ProjMatrix[11]) + (Util.ModelMatrix[3] * Util.ProjMatrix[15]);

            Util.ClipMatrix[4] = (Util.ModelMatrix[4] * Util.ProjMatrix[0]) + (Util.ModelMatrix[5] * Util.ProjMatrix[4]) + (Util.ModelMatrix[6] * Util.ProjMatrix[8]) + (Util.ModelMatrix[7] * Util.ProjMatrix[12]);
            Util.ClipMatrix[5] = (Util.ModelMatrix[4] * Util.ProjMatrix[1]) + (Util.ModelMatrix[5] * Util.ProjMatrix[5]) + (Util.ModelMatrix[6] * Util.ProjMatrix[9]) + (Util.ModelMatrix[7] * Util.ProjMatrix[13]);
            Util.ClipMatrix[6] = (Util.ModelMatrix[4] * Util.ProjMatrix[2]) + (Util.ModelMatrix[5] * Util.ProjMatrix[6]) + (Util.ModelMatrix[6] * Util.ProjMatrix[10]) + (Util.ModelMatrix[7] * Util.ProjMatrix[14]);
            Util.ClipMatrix[7] = (Util.ModelMatrix[4] * Util.ProjMatrix[3]) + (Util.ModelMatrix[5] * Util.ProjMatrix[7]) + (Util.ModelMatrix[6] * Util.ProjMatrix[11]) + (Util.ModelMatrix[7] * Util.ProjMatrix[15]);

            Util.ClipMatrix[8] = (Util.ModelMatrix[8] * Util.ProjMatrix[0]) + (Util.ModelMatrix[9] * Util.ProjMatrix[4]) + (Util.ModelMatrix[10] * Util.ProjMatrix[8]) + (Util.ModelMatrix[11] * Util.ProjMatrix[12]);
            Util.ClipMatrix[9] = (Util.ModelMatrix[8] * Util.ProjMatrix[1]) + (Util.ModelMatrix[9] * Util.ProjMatrix[5]) + (Util.ModelMatrix[10] * Util.ProjMatrix[9]) + (Util.ModelMatrix[11] * Util.ProjMatrix[13]);
            Util.ClipMatrix[10] = (Util.ModelMatrix[8] * Util.ProjMatrix[2]) + (Util.ModelMatrix[9] * Util.ProjMatrix[6]) + (Util.ModelMatrix[10] * Util.ProjMatrix[10]) + (Util.ModelMatrix[11] * Util.ProjMatrix[14]);
            Util.ClipMatrix[11] = (Util.ModelMatrix[8] * Util.ProjMatrix[3]) + (Util.ModelMatrix[9] * Util.ProjMatrix[7]) + (Util.ModelMatrix[10] * Util.ProjMatrix[11]) + (Util.ModelMatrix[11] * Util.ProjMatrix[15]);

            Util.ClipMatrix[12] = (Util.ModelMatrix[12] * Util.ProjMatrix[0]) + (Util.ModelMatrix[13] * Util.ProjMatrix[4]) + (Util.ModelMatrix[14] * Util.ProjMatrix[8]) + (Util.ModelMatrix[15] * Util.ProjMatrix[12]);
            Util.ClipMatrix[13] = (Util.ModelMatrix[12] * Util.ProjMatrix[1]) + (Util.ModelMatrix[13] * Util.ProjMatrix[5]) + (Util.ModelMatrix[14] * Util.ProjMatrix[9]) + (Util.ModelMatrix[15] * Util.ProjMatrix[13]);
            Util.ClipMatrix[14] = (Util.ModelMatrix[12] * Util.ProjMatrix[2]) + (Util.ModelMatrix[13] * Util.ProjMatrix[6]) + (Util.ModelMatrix[14] * Util.ProjMatrix[10]) + (Util.ModelMatrix[15] * Util.ProjMatrix[14]);
            Util.ClipMatrix[15] = (Util.ModelMatrix[12] * Util.ProjMatrix[3]) + (Util.ModelMatrix[13] * Util.ProjMatrix[7]) + (Util.ModelMatrix[14] * Util.ProjMatrix[11]) + (Util.ModelMatrix[15] * Util.ProjMatrix[15]);

            // laske frustumin tasot ja normalisoi ne
            frustum[RIGHT, 0] = Util.ClipMatrix[3] - Util.ClipMatrix[0];
            frustum[RIGHT, 1] = Util.ClipMatrix[7] - Util.ClipMatrix[4];
            frustum[RIGHT, 2] = Util.ClipMatrix[11] - Util.ClipMatrix[8];
            frustum[RIGHT, 3] = Util.ClipMatrix[15] - Util.ClipMatrix[12];
            NormalizePlane(frustum, RIGHT);

            frustum[LEFT, 0] = Util.ClipMatrix[3] + Util.ClipMatrix[0];
            frustum[LEFT, 1] = Util.ClipMatrix[7] + Util.ClipMatrix[4];
            frustum[LEFT, 2] = Util.ClipMatrix[11] + Util.ClipMatrix[8];
            frustum[LEFT, 3] = Util.ClipMatrix[15] + Util.ClipMatrix[12];
            NormalizePlane(frustum, LEFT);

            frustum[BOTTOM, 0] = Util.ClipMatrix[3] + Util.ClipMatrix[1];
            frustum[BOTTOM, 1] = Util.ClipMatrix[7] + Util.ClipMatrix[5];
            frustum[BOTTOM, 2] = Util.ClipMatrix[11] + Util.ClipMatrix[9];
            frustum[BOTTOM, 3] = Util.ClipMatrix[15] + Util.ClipMatrix[13];
            NormalizePlane(frustum, BOTTOM);

            frustum[TOP, 0] = Util.ClipMatrix[3] - Util.ClipMatrix[1];
            frustum[TOP, 1] = Util.ClipMatrix[7] - Util.ClipMatrix[5];
            frustum[TOP, 2] = Util.ClipMatrix[11] - Util.ClipMatrix[9];
            frustum[TOP, 3] = Util.ClipMatrix[15] - Util.ClipMatrix[13];
            NormalizePlane(frustum, TOP);

            frustum[BACK, 0] = Util.ClipMatrix[3] - Util.ClipMatrix[2];
            frustum[BACK, 1] = Util.ClipMatrix[7] - Util.ClipMatrix[6];
            frustum[BACK, 2] = Util.ClipMatrix[11] - Util.ClipMatrix[10];
            frustum[BACK, 3] = Util.ClipMatrix[15] - Util.ClipMatrix[14];
            NormalizePlane(frustum, BACK);

            frustum[FRONT, 0] = Util.ClipMatrix[3] + Util.ClipMatrix[2];
            frustum[FRONT, 1] = Util.ClipMatrix[7] + Util.ClipMatrix[6];
            frustum[FRONT, 2] = Util.ClipMatrix[11] + Util.ClipMatrix[10];
            frustum[FRONT, 3] = Util.ClipMatrix[15] + Util.ClipMatrix[14];
            NormalizePlane(frustum, FRONT);
        }

        /// <summary>
        /// tasojen normaalit osoittaa sis‰‰np‰in joten jos testattava vertex on
        /// kaikkien tasojen "edess‰", se on ruudulla ja rendataan
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static bool PointInFrustum(float x, float y, float z)
        {
            // tasoyht‰lˆ: A*x + B*y + C*z + D = 0
            // ABC on normaalin X, Y ja Z
            // D on tason et‰isyys origosta
            // =0 vertex on tasolla
            // <0 tason takana
            // >0 tason edess‰
            for (int a = 0; a < 6; a++)
            {
                // jos vertex jonkun tason takana, niin palauta false (ei rendata)
                if (((frustum[a, 0] * x) + (frustum[a, 1] * y) + (frustum[a, 2] * z) + frustum[a, 3]) <= 0)
                {
                    return false;
                }
            }

            // ruudulla
            return true;
        }

        /// <summary>
        /// palauttaa et‰isyyden kameraan jos pallo frustumissa, muuten 0.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static float SphereInFrustum(float x, float y, float z, float radius)
        {
            float d = 0;
            for (int p = 0; p < 6; p++)
            {
                // jos pallo ei ole ruudulla
                d = frustum[p, 0] * x + frustum[p, 1] * y + frustum[p, 2] * z + frustum[p, 3];
                if (d <= -radius)
                {
                    return 0;
                }
            }
            // kaikkien tasojen edess‰ eli n‰kyviss‰
            // palauta matka kameraan
            return d + radius;
        }

        /// <summary>
        /// box testaus. onko laatikko edes osittain ruudulla. jos on, palauta true.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool BoxInFrustum(float x, float y, float z, BoundingVolume bb)
        {
            int q;
            for (q = 0; q < 6; q++)
            {
                if ((frustum[q, 0] * (x + bb.Min.X)) + (frustum[q, 1] * (y + bb.Min.Y)) + (frustum[q, 2] * (z + bb.Min.Z)) + frustum[q, 3] > 0)
                {
                    continue;
                }

                if ((frustum[q, 0] * (x + bb.Max.X)) + (frustum[q, 1] * (y + bb.Min.Y)) + (frustum[q, 2] * (z + bb.Min.Z)) + frustum[q, 3] > 0)
                {
                    continue;
                }

                if ((frustum[q, 0] * (x + bb.Max.X)) + (frustum[q, 1] * (y + bb.Min.Y)) + (frustum[q, 2] * (z + bb.Max.Z)) + frustum[q, 3] > 0)
                {
                    continue;
                }

                if ((frustum[q, 0] * (x + bb.Min.X)) + (frustum[q, 1] * (y + bb.Min.Y)) + (frustum[q, 2] * (z + bb.Max.Z)) + frustum[q, 3] > 0)
                {
                    continue;
                }

                if ((frustum[q, 0] * (x + bb.Min.X)) + (frustum[q, 1] * (y + bb.Max.Y)) + (frustum[q, 2] * (z + bb.Min.Z)) + frustum[q, 3] > 0)
                {
                    continue;
                }

                if ((frustum[q, 0] * (x + bb.Max.X)) + (frustum[q, 1] * (y + bb.Max.Y)) + (frustum[q, 2] * (z + bb.Min.Z)) + frustum[q, 3] > 0)
                {
                    continue;
                }

                if ((frustum[q, 0] * (x + bb.Max.X)) + (frustum[q, 1] * (y + bb.Max.Y)) + (frustum[q, 2] * (z + bb.Max.Z)) + frustum[q, 3] > 0)
                {
                    continue;
                }

                if ((frustum[q, 0] * (x + bb.Min.X)) + (frustum[q, 1] * (y + bb.Max.Y)) + (frustum[q, 2] * (z + bb.Max.Z)) + frustum[q, 3] > 0)
                {
                    continue;
                }

                // jos p‰‰st‰‰n t‰nne, objekti ei ole frustumissa
                return false;
            }

            // vertex ruudulla, objekti on n‰kyv‰
            return true;
        }

        public static bool ObjectInFrustum(Vector3 position, BoundingVolume bound)
        {
            return ObjectInFrustum(position.X, position.Y, position.Z, bound);
        }

        public static bool ObjectInFrustum(float x, float y, float z, BoundingVolume bound)
        {
            if (bound == null) return true;

            switch (bound.Mode)
            {
                case BoundingVolume.TestMode.Box: // box
                    if (BoxInFrustum(x, y, z, bound) == false) return false;
                    break;

                case BoundingVolume.TestMode.Sphere: // sphere
                    if (SphereInFrustum(x, y, z, bound.R) == 0) return false;
                    break;
            }
            return true;
        }


    }
}
