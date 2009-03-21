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
    public class Light : Node
    {
        int lightNum = 0;

        /// <summary>
        /// valotaulukko. kaikki valot lisätään tähän
        /// </summary>
        public static List<Light> Lights = new List<Light>();
        public Vector3 Diffuse = new Vector3(0.8f, 0.8f, 0.8f);
        public Vector3 Specular = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 Ambient = new Vector3(0.1f, 0.1f, 0.1f);
        public bool Enabled = true;

        public Light(string name)
            : base(name)
        {

        }

        public static void Remove(Light light)
        {
            light.SetLight(false);
            Lights.Remove(light);
        }
        public void Remove()
        {
            SetLight(false);
            Lights.Remove(this);
        }

        public static void Dispose(Light light)
        {
            Lights.Remove(light);

            Log.WriteDebugLine("Disposed: Light");
        }
        public new static void DisposeAll()
        {
            Lights.Clear();

            Log.WriteDebugLine("Disposed: All Lights");
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
                Enabled = false;
                return;
            }
            Enabled = true;
            GL.Enable(EnableCap.Light0 + lightNum);
        }

        /// <summary>
        /// päivitä kameran paikka ettei se liiku kameran mukana
        /// </summary>
        public void UpdateLight()
        {
            if (Enabled == false) return;
            GL.Lightv(LightName.Light0 + lightNum, LightParameter.Position, new float[] { Position.X, Position.Y, Position.Z, 1 });
        }

        /// <summary>
        /// päivitä valon väri. tarvii päivittää vain jos muuttaa.
        /// </summary>
        public void UpdateColor()
        {
            GL.Lightv(LightName.Light0 + lightNum, LightParameter.Ambient, new float[] { Ambient.X, Ambient.Y, Ambient.Z });
            GL.Lightv(LightName.Light0 + lightNum, LightParameter.Diffuse, new float[] { Diffuse.X, Diffuse.Y, Diffuse.Z });
            GL.Lightv(LightName.Light0 + lightNum, LightParameter.Specular, new float[] { Specular.X, Specular.Y, Specular.Z });
        }

        /// <summary>
        /// päivitä kaikki valot
        /// </summary>
        public static void UpdateLights()
        {
            for (int q = 0; q < Lights.Count; q++)
            {
                Light l = (Light)Lights[q];
                l.UpdateLight();
            }

        }
    }
}
