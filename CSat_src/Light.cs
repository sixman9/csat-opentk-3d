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
using System.Collections;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public class Light : ObjectInfo
    {
        /// <summary>
        /// valotaulukko. kaikki valot lisätään tähän
        /// </summary>
        public static ArrayList lights = new ArrayList();
        public Vector3 diffuse = new Vector3(1, 1, 1);
        public Vector3 specular = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 ambient = new Vector3(0.2f, 0.2f, 0.2f);
        public bool enabled = false;


        public static void Add(Light light)
        {
            lights.Add(light);
            Log.WriteDebugLine("Light added..");
        }
        public static void Dispose(Light light)
        {
            lights.Remove(light);
            Log.WriteDebugLine("Light removed..");
        }
        public static void DisposeAll()
        {
            lights.Clear();
            Log.WriteDebugLine("All lights removed..");
        }

        public static void Enable()
        {
            GL.Enable(EnableCap.Lighting);
        }
        public static void Disable()
        {
            GL.Disable(EnableCap.Lighting);
        }

        /// <summary>
        /// aseta valon tila ja paikka ettei se liiku kameran mukana
        /// </summary>
        /// <param name="lightNum"></param>
        /// <param name="enable"></param>
        public void SetLight(int lightNum, bool enable)
        {
            if (enable == false)
            {
                enabled = false;
                return;
            }
            enabled = true;

            GL.Lightv(LightName.Light0 + lightNum, LightParameter.Position, new float[] { position.X, position.Y, position.Z });
            GL.Enable(EnableCap.Light0 + lightNum);
        }

        public void UpdateColor(int lightNum)
        {
            GL.Lightv(LightName.Light0 + lightNum, LightParameter.Ambient, new float[] { ambient.X, ambient.Y, ambient.Z });
            GL.Lightv(LightName.Light0 + lightNum, LightParameter.Diffuse, new float[] { diffuse.X, diffuse.Y, diffuse.Z });
            GL.Lightv(LightName.Light0 + lightNum, LightParameter.Specular, new float[] { specular.X, specular.Y, specular.Z });
        }

        /// <summary>
        /// päivitä kaikki valot
        /// </summary>
        public static void UpdateLights()
        {
            for (int q = 0; q < lights.Count; q++)
            {
                ((Light)lights[q]).SetLight(q, ((Light)lights[q]).enabled);
            }

        }
    }
}
