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
        public Vector3 diffuse = new Vector3(0.8f, 0.8f, 0.8f);
        public Vector3 specular = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 ambient = new Vector3(0.1f, 0.1f, 0.1f);
        public bool enabled = false;
        int lightNum = 0;

        /// <summary>
        /// lisää valo. 
        /// </summary>
        /// <param name="light"></param>
        /// <param name="lightNum">valon paikkanumero, arvo saa olla 0-7 (max 8 valoa päällä)</param>
        public static void Add(Light light, int lightNum)
        {
            if (lightNum >= 8) throw new Exception("Light: (lightNum>=8)");
            lights.Add(light);
            light.UpdateColor();
            light.SetLight(true);
            light.lightNum = lightNum;
            Log.WriteDebugLine("Light added..", 2);
        }
        public static void Dispose(Light light)
        {
            lights.Remove(light);
            Log.WriteDebugLine("Light removed..", 2);
        }
        public static void DisposeAll()
        {
            lights.Clear();
            Log.WriteDebugLine("All lights removed..", 2);
        }

        /// <summary>
        /// valot käyttöön
        /// </summary>
        public static void Enable()
        {
            GL.Enable(EnableCap.Lighting);
        }
        /// <summary>
        /// valot pois käytöstä
        /// </summary>
        public static void Disable()
        {
            GL.Disable(EnableCap.Lighting);
        }

        /// <summary>
        /// aseta valon tila (päällä/pois päältä)
        /// </summary>
        /// <param name="enable"></param>
        public void SetLight(bool enable)
        {
            if (enable == false)
            {
                GL.Disable(EnableCap.Light0 + lightNum);
                enabled = false;
                return;
            }
            enabled = true;
            GL.Enable(EnableCap.Light0 + lightNum);
        }

        /// <summary>
        /// päivitä kameran paikka ettei se liiku kameran mukana
        /// </summary>
        public void UpdateLight()
        {
            if (enabled == false) return;
            GL.Lightv(LightName.Light0 + lightNum, LightParameter.Position, new float[] { Position.X, Position.Y, Position.Z, 1 });
        }

        /// <summary>
        /// päivitä valon väri. tarvii päivittää vain jos muuttaa.
        /// </summary>
        public void UpdateColor()
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
                Light l = (Light)lights[q];
                l.UpdateLight();
            }

        }
    }
}
