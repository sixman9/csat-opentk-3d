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

using System;
using System.IO;
using System.Collections;
using OpenTK.Graphics;

namespace CSat
{
    public class GLSL
    {
        // ohjelmavarasto josta katsotaan onko ohjelma jo k‰‰nnetty (ei tarvii jokaiselle objektille erikseen k‰‰nt‰‰)
        public static Hashtable programs = new Hashtable();

        int program, vertexObject, fragmentObject;

        bool hardware = true;
        public bool Hardware { get { return hardware; } }

        public void Load(string vertexShader, string fragmentShader)
        {
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
            // onko ohjelma jo k‰‰nnetty?
            if ((GLSL)programs[name] != null)
            {
                // kopsataan tiedot
                GLSL prog = (GLSL)programs[name];
                program = prog.program;
                vertexObject = prog.vertexObject;
                fragmentObject = prog.fragmentObject;

                return;
            }

            int status_code;
            string info;

            vertexObject = GL.CreateShader(ShaderType.VertexShader);
            fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex shader
            GL.ShaderSource(vertexObject, vs);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out status_code);
            if (info.Contains("software")) hardware = false;

            if (status_code != 1) throw new Exception("GLSL: " + info);

            // Compile vertex shader
            GL.ShaderSource(fragmentObject, fs);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out status_code);
            if (info.Contains("software"))
            {
                hardware = false;
                Log.WriteDebugLine("GLSL: software mode.");
            }
            else Log.WriteDebugLine("GLSL: hardware mode.");

            if (status_code != 1) throw new Exception("GLSL: " + info);

            program = GL.CreateProgram();
            GL.AttachShader(program, fragmentObject);
            GL.AttachShader(program, vertexObject);
            GL.LinkProgram(program);

            Util.CheckGLError("GLSL");

            programs.Add(name, this);
        }

        public void UseProgram()
        {
            GL.UseProgram(program);
        }

        public void DontUseProgram()
        {
            GL.UseProgram(0);
        }

        public void Dispose()
        {
            if (program != 0) GL.DeleteProgram(program);
            if (fragmentObject != 0) GL.DeleteShader(fragmentObject);
            if (vertexObject != 0) GL.DeleteShader(vertexObject);

            program = fragmentObject = vertexObject = 0;
        }

        public static void DisposeAll()
        {
            IDictionaryEnumerator en = programs.GetEnumerator();
            while (en.MoveNext())
            {
                GLSL g = (GLSL)en.Value;
                g.Dispose();
            }
            programs.Clear();
        }

    }
}
