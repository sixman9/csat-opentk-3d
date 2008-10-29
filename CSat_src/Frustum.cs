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
 * tutoriaali:
 * http://www.crownandcutlass.com/features/technicaldetails/frustum.html
 *
 */

using System;
using OpenTK.Graphics;

namespace CSat
{

    public class Frustum
    {
        static float[,] frustum = new float[6, 4];
        const int RIGHT = 0, LEFT = 1, BOTTOM = 2, TOP = 3, BACK = 4, FRONT = 5;

        static float[] projMatrix = new float[16];
        static float[] modelMatrix = new float[16];
        static float[] clipMatrix = new float[16];

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
            // ota projection ja modelview matriisit
            GL.GetFloat(GetPName.ProjectionMatrix, projMatrix);
            GL.GetFloat(GetPName.ModelviewMatrix, modelMatrix);

            clipMatrix[0] = (modelMatrix[0] * projMatrix[0]) + (modelMatrix[1] * projMatrix[4]) + (modelMatrix[2] * projMatrix[8]) + (modelMatrix[3] * projMatrix[12]);
            clipMatrix[1] = (modelMatrix[0] * projMatrix[1]) + (modelMatrix[1] * projMatrix[5]) + (modelMatrix[2] * projMatrix[9]) + (modelMatrix[3] * projMatrix[13]);
            clipMatrix[2] = (modelMatrix[0] * projMatrix[2]) + (modelMatrix[1] * projMatrix[6]) + (modelMatrix[2] * projMatrix[10]) + (modelMatrix[3] * projMatrix[14]);
            clipMatrix[3] = (modelMatrix[0] * projMatrix[3]) + (modelMatrix[1] * projMatrix[7]) + (modelMatrix[2] * projMatrix[11]) + (modelMatrix[3] * projMatrix[15]);

            clipMatrix[4] = (modelMatrix[4] * projMatrix[0]) + (modelMatrix[5] * projMatrix[4]) + (modelMatrix[6] * projMatrix[8]) + (modelMatrix[7] * projMatrix[12]);
            clipMatrix[5] = (modelMatrix[4] * projMatrix[1]) + (modelMatrix[5] * projMatrix[5]) + (modelMatrix[6] * projMatrix[9]) + (modelMatrix[7] * projMatrix[13]);
            clipMatrix[6] = (modelMatrix[4] * projMatrix[2]) + (modelMatrix[5] * projMatrix[6]) + (modelMatrix[6] * projMatrix[10]) + (modelMatrix[7] * projMatrix[14]);
            clipMatrix[7] = (modelMatrix[4] * projMatrix[3]) + (modelMatrix[5] * projMatrix[7]) + (modelMatrix[6] * projMatrix[11]) + (modelMatrix[7] * projMatrix[15]);

            clipMatrix[8] = (modelMatrix[8] * projMatrix[0]) + (modelMatrix[9] * projMatrix[4]) + (modelMatrix[10] * projMatrix[8]) + (modelMatrix[11] * projMatrix[12]);
            clipMatrix[9] = (modelMatrix[8] * projMatrix[1]) + (modelMatrix[9] * projMatrix[5]) + (modelMatrix[10] * projMatrix[9]) + (modelMatrix[11] * projMatrix[13]);
            clipMatrix[10] = (modelMatrix[8] * projMatrix[2]) + (modelMatrix[9] * projMatrix[6]) + (modelMatrix[10] * projMatrix[10]) + (modelMatrix[11] * projMatrix[14]);
            clipMatrix[11] = (modelMatrix[8] * projMatrix[3]) + (modelMatrix[9] * projMatrix[7]) + (modelMatrix[10] * projMatrix[11]) + (modelMatrix[11] * projMatrix[15]);

            clipMatrix[12] = (modelMatrix[12] * projMatrix[0]) + (modelMatrix[13] * projMatrix[4]) + (modelMatrix[14] * projMatrix[8]) + (modelMatrix[15] * projMatrix[12]);
            clipMatrix[13] = (modelMatrix[12] * projMatrix[1]) + (modelMatrix[13] * projMatrix[5]) + (modelMatrix[14] * projMatrix[9]) + (modelMatrix[15] * projMatrix[13]);
            clipMatrix[14] = (modelMatrix[12] * projMatrix[2]) + (modelMatrix[13] * projMatrix[6]) + (modelMatrix[14] * projMatrix[10]) + (modelMatrix[15] * projMatrix[14]);
            clipMatrix[15] = (modelMatrix[12] * projMatrix[3]) + (modelMatrix[13] * projMatrix[7]) + (modelMatrix[14] * projMatrix[11]) + (modelMatrix[15] * projMatrix[15]);

            // laske frustumin tasot ja normalisoi ne
            frustum[RIGHT, 0] = clipMatrix[3] - clipMatrix[0];
            frustum[RIGHT, 1] = clipMatrix[7] - clipMatrix[4];
            frustum[RIGHT, 2] = clipMatrix[11] - clipMatrix[8];
            frustum[RIGHT, 3] = clipMatrix[15] - clipMatrix[12];
            NormalizePlane(frustum, RIGHT);

            frustum[LEFT, 0] = clipMatrix[3] + clipMatrix[0];
            frustum[LEFT, 1] = clipMatrix[7] + clipMatrix[4];
            frustum[LEFT, 2] = clipMatrix[11] + clipMatrix[8];
            frustum[LEFT, 3] = clipMatrix[15] + clipMatrix[12];
            NormalizePlane(frustum, LEFT);

            frustum[BOTTOM, 0] = clipMatrix[3] + clipMatrix[1];
            frustum[BOTTOM, 1] = clipMatrix[7] + clipMatrix[5];
            frustum[BOTTOM, 2] = clipMatrix[11] + clipMatrix[9];
            frustum[BOTTOM, 3] = clipMatrix[15] + clipMatrix[13];
            NormalizePlane(frustum, BOTTOM);

            frustum[TOP, 0] = clipMatrix[3] - clipMatrix[1];
            frustum[TOP, 1] = clipMatrix[7] - clipMatrix[5];
            frustum[TOP, 2] = clipMatrix[11] - clipMatrix[9];
            frustum[TOP, 3] = clipMatrix[15] - clipMatrix[13];
            NormalizePlane(frustum, TOP);

            frustum[BACK, 0] = clipMatrix[3] - clipMatrix[2];
            frustum[BACK, 1] = clipMatrix[7] - clipMatrix[6];
            frustum[BACK, 2] = clipMatrix[11] - clipMatrix[10];
            frustum[BACK, 3] = clipMatrix[15] - clipMatrix[14];
            NormalizePlane(frustum, BACK);

            frustum[FRONT, 0] = clipMatrix[3] + clipMatrix[2];
            frustum[FRONT, 1] = clipMatrix[7] + clipMatrix[6];
            frustum[FRONT, 2] = clipMatrix[11] + clipMatrix[10];
            frustum[FRONT, 3] = clipMatrix[15] + clipMatrix[14];
            NormalizePlane(frustum, FRONT);
        }

        // tasojen normaalit osoittaa sis‰‰np‰in joten jos testattava vertex on
        // kaikkien tasojen "edess‰", se on ruudulla ja rendataan
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

        public static bool ObjectInFrustum(float x, float y, float z, BoundingVolume ba)
        {
            // mode: mik‰ testaus tehd‰‰n
            switch (ba.Mode)
            {
                case 1: // box
                    if (BoxInFrustum(x, y, z, ba) == false) return false;
                    break;

                case 2: // sphere
                    if (SphereInFrustum(x, y, z, ba.R) == 0) return false;
                    break;
            }

            return true;
        }


    }
}
