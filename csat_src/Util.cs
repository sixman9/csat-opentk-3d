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

using System;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public static class Util
    {
        public static float[] ProjMatrix = new float[16];
        public static float[] ModelMatrix = new float[16];
        public static float[] ClipMatrix = new float[16];

        public static void CopyArray(ref float[] arrayIn, ref float[] arrayOut)
        {
            if (arrayOut == null) arrayOut = new float[arrayIn.Length];
            for (int q = 0; q < arrayIn.Length; q++)
            {
                arrayOut[q] = arrayIn[q];
            }
        }

        public static void ArrayToMatrix(float[] array, out Matrix4 matrix)
        {
            matrix.Row0.X = array[0];
            matrix.Row0.Y = array[1];
            matrix.Row0.Z = array[2];
            matrix.Row0.W = array[3];

            matrix.Row1.X = array[4];
            matrix.Row1.Y = array[5];
            matrix.Row1.Z = array[6];
            matrix.Row1.W = array[7];

            matrix.Row2.X = array[8];
            matrix.Row2.Y = array[9];
            matrix.Row2.Z = array[10];
            matrix.Row2.W = array[11];

            matrix.Row3.X = array[12];
            matrix.Row3.Y = array[13];
            matrix.Row3.Z = array[14];
            matrix.Row3.W = array[15];
        }

        public static void ClearArrays()
        {
            Texture.DisposeAll();
            Light.DisposeAll();
            Node.DisposeAll();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// ristikkoviritelmä
        /// </summary>
        public static void RenderGrid()
        {
            GL.LineWidth(2);
            GL.Begin(BeginMode.Lines);
            GL.Vertex3(-100, 0, 0);
            GL.Vertex3(100, 0, 0);
            GL.Vertex3(0, 0, -100);
            GL.Vertex3(0, 0, 100);
            GL.Vertex3(0, -100, 0);
            GL.Vertex3(0, 100, 0);
            GL.End();
            GL.LineWidth(1);

            GL.Begin(BeginMode.Lines);
            for (int q = 0; q < 20; q++)
            {
                GL.Vertex3(-100, 0, q * 10 - 100);
                GL.Vertex3(100, 0, q * 10 - 100);

                GL.Vertex3(q * 10 - 100, 0, -100);
                GL.Vertex3(q * 10 - 100, 0, 100);
            }
            GL.End();

        }

        /// <summary>
        /// palauttaa str:stä float luvun. jos pisteen kanssa ei onnistu, kokeillaan pilkun kanssa.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static float GetFloat(string str)
        {
            float n;
            if (float.TryParse(str, out n) == true)
            {
                return n;
            }
            str = str.Replace('.', ','); // pisteet pilkuiksi
            if (float.TryParse(str, out n) == true)
            {
                return n;
            }
            throw new Exception("GetFloat failed: " + str);
        }

        static bool is3DMode = false;
        public static double Near = 0.1, Far = 1000, Fov = 45;
        public static int ScreenWidth = 800, ScreeenHeight = 600;
        public static void Set2DMode(int width, int height)
        {
            is3DMode = false;
            ScreenWidth = width;
            ScreeenHeight = height;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1, 1);
            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
        }
        public static void Set3DMode(int width, int height, double near, double far)
        {
            is3DMode = true;
            ScreenWidth = width;
            ScreeenHeight = height;
            Near = near;
            Far = far;

            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Glu.Perspective(Fov, (double)width / (double)height, near, far);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public static void Resize(int width, int height, double near, double far)
        {
            if (is3DMode) Set3DMode(width, height, near, far);
            else Set2DMode(width, height);
        }
        public static void Resize()
        {
            Resize(ScreenWidth, ScreeenHeight, Near, Far);
        }
        public static void Set2DMode()
        {
            Set2DMode(ScreenWidth, ScreeenHeight);
        }
        public static void Set3DMode()
        {
            Set3DMode(ScreenWidth, ScreeenHeight, Near, Far);
        }

        public static void CheckGLError(string className)
        {
            ErrorCode error = ErrorCode.NoError;

            GL.Finish();

            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new ArgumentException(className + ": " + Glu.ErrorString(error) + " (" + error + ")");
            }
        }

    }

}
