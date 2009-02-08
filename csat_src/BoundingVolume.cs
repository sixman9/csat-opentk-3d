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
using System.Collections.Generic;

namespace CSat
{
    public class BoundingVolume
    {
        public enum TestMode { None, Box, Sphere };
        public Vector3 Min = new Vector3(99999, 99999, 99999);
        public Vector3 Max = new Vector3(-99999, -99999, -99999);
        public TestMode Mode = TestMode.Sphere;
        public float R = 0;
        public Vector3[] Corner = new Vector3[8]; // bboxin kulmat

        public BoundingVolume()
        {
        }
        public BoundingVolume(TestMode mode)
        {
            Mode = mode;
        }

        public void CreateBoundingVolume(Mesh mesh)
        {
            for (int q = 0; q < mesh.Vertices.Length; q++)
            {
                if (mesh.Vertices[q].vertex.X < Min.X) Min.X = mesh.Vertices[q].vertex.X;
                if (mesh.Vertices[q].vertex.Y < Min.Y) Min.Y = mesh.Vertices[q].vertex.Y;
                if (mesh.Vertices[q].vertex.Z < Min.Z) Min.Z = mesh.Vertices[q].vertex.Z;

                if (mesh.Vertices[q].vertex.X > Max.X) Max.X = mesh.Vertices[q].vertex.X;
                if (mesh.Vertices[q].vertex.Y > Max.Y) Max.Y = mesh.Vertices[q].vertex.Y;
                if (mesh.Vertices[q].vertex.Z > Max.Z) Max.Z = mesh.Vertices[q].vertex.Z;
            }

            Vector3 v = Max - Min;
            R = v.Length / 2;

            if (mesh.IsRendObj)
            {
                v.Scale(0.5f, 0.5f, 0.5f); // puoleen väliin
                mesh.ObjCenter = Min + v; // objektin keskikohta
            }

            SetCorners();
        }

        public void CreateBoundingVolume(Mesh mesh, Vector3 min, Vector3 max)
        {
            if (mesh.Vertices.Length == 0) return;

            Min = min;
            Max = max;
            Vector3 v = Max - Min;
            R = v.Length / 2;

            if (mesh.IsRendObj)
            {
                v.Scale(0.5f, 0.5f, 0.5f); // puoleen väliin
                mesh.ObjCenter = Min + v; // objektin keskikohta
            }

            SetCorners();
        }

        void SetCorners()
        {
            // aseta kulmat
            Corner[0] = Min;

            Corner[1] = Min;
            Corner[1].X = Max.X;

            Corner[2] = Min;
            Corner[2].X = Max.X;
            Corner[2].Y = Max.Y;

            Corner[3] = Min;
            Corner[3].Y = Max.Y;

            Corner[4] = Max;

            Corner[5] = Max;
            Corner[5].X = Min.X;

            Corner[6] = Max;
            Corner[6].X = Min.X;
            Corner[6].Y = Min.Y;

            Corner[7] = Max;
            Corner[7].Y = Min.Y;
        }

    }
}
