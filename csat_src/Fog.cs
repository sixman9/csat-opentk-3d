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
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSat
{
    public class Fog
    {
        public static Vector3 Color = new Vector3(0.3f, 0.2f, 0.5f);

        /// <summary>
        /// luo sumu
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="density"></param>
        public static void CreateFog(FogMode mode, float start, float end, float density)
        {
            GL.Fog(FogParameter.FogMode, (int)mode);
            
            GL.Fog(FogParameter.FogColor, new float[] { Color.X, Color.Y, Color.Z });
            GL.Fog(FogParameter.FogDensity, density);
            GL.Hint(HintTarget.FogHint, HintMode.DontCare);

            GL.Fog(FogParameter.FogStart, start);
            GL.Fog(FogParameter.FogEnd, end);
            GL.Enable(EnableCap.Fog);
        }

        public static void DisableFog()
        {
            GL.Disable(EnableCap.Fog);
        }
    }
}