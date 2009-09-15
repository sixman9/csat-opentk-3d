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

using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSat
{
    public class Camera : Node
    {
        public static Camera cam = null;
        public float[] CameraMatrix = new float[16];

        public Camera()
        {
            Name = "camera";
            cam = this;
        }
        public Camera(string name)
        {
            this.Name = name;
            cam = this;
        }

        public override void Render()
        {
        }

        /// <summary>
        /// p‰ivit‰ kameran paikka XZ tasolla.
        /// </summary>
        public void UpdateXZ()
        {
            GL.LoadIdentity();
            GL.Rotate(-Rotation.X, 1.0f, 0, 0);
            GL.Rotate(-Rotation.Y, 0, 1.0f, 0);
            GL.Rotate(-Rotation.Z, 0, 0, 1.0f);
            GL.Translate(-Position);

            GL.GetFloat(GetPName.ModelviewMatrix, CameraMatrix);
        }

        /// <summary>
        /// p‰ivit‰ kameran paikka (6DOF kamera)
        /// </summary>
        public void Update6DOF()
        {
            GL.LoadIdentity();
            MathExt.LookAt(Position, Position + Front, Up);

            GL.GetFloat(GetPName.ModelviewMatrix, CameraMatrix);
        }

        /// <summary>
        /// k‰‰nn‰ kuvakulma pos:iin
        /// </summary>
        /// <param name="pos"></param>
        public new void LookAt(Vector3 pos)
        {
            GL.LoadIdentity();
            MathExt.LookAt(Position, pos, Up);
        }

    }
}

