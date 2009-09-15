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
using System.IO;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace CSat
{
    public class GLSL
    {
        int program, vertexObject, fragmentObject;

        public bool IsSupported = false;
        bool hardware = true;
        public bool Hardware { get { return hardware; } }

        public GLSL()
        {
            // tarkista voidaanko shadereita k‰ytt‰‰.
            if (GL.GetString(StringName.Extensions).Contains("vertex_shader") &&
                GL.GetString(StringName.Extensions).Contains("fragment_shader")) IsSupported = true;
        }


        public void Load(string vertexShader, string fragmentShader)
        {
            if (IsSupported == false) return;

            using (StreamReader vs = new StreamReader(Settings.ShaderDir + vertexShader))
            {
                using (StreamReader fs = new StreamReader(Settings.ShaderDir + fragmentShader))
                {
                    CreateShaders(vertexShader + fragmentShader, vs.ReadToEnd(), fs.ReadToEnd());
                }
            }
            Log.WriteDebugLine("GLSL: " + vertexShader + " " + fragmentShader);
        }

        void CreateShaders(string name, string vs, string fs)
        {
            int status_code;
            string info;

            vertexObject = GL.CreateShader(ShaderType.VertexShader);
            fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex Shader
            GL.ShaderSource(vertexObject, vs);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out status_code);
            if (info.Contains("software")) hardware = false;

            if (status_code != 1) throw new Exception("GLSL: " + info);

            // Compile vertex Shader
            GL.ShaderSource(fragmentObject, fs);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out status_code);
            if (info.Contains("software"))
            {
                hardware = false;
                Log.WriteDebugLine("GLSL: software mode.");
            }
            //else Log.WriteDebugLine("GLSL: hardware mode.");

            if (status_code != 1) throw new Exception("GLSL: " + info);

            program = GL.CreateProgram();
            GL.AttachShader(program, fragmentObject);
            GL.AttachShader(program, vertexObject);
            GL.LinkProgram(program);

            Util.CheckGLError("GLSL");
        }

        /// <summary>
        /// k‰yt‰ shaderia
        /// </summary>
        public void UseProgram()
        {
            if (IsSupported == false) return;
            GL.UseProgram(program);
        }

        /// <summary>
        /// lopeta shaderin k‰ytt‰minen
        /// </summary>
        public void RemoveProgram()
        {
            if (IsSupported == false) return;
            GL.UseProgram(0);
        }

        public void Dispose()
        {
            if (program != 0) GL.DeleteProgram(program);
            if (fragmentObject != 0) GL.DeleteShader(fragmentObject);
            if (vertexObject != 0) GL.DeleteShader(vertexObject);

            program = fragmentObject = vertexObject = 0;
        }

    }
}
