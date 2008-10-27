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
    public class Fog
    {
        static Vector3 color = new Vector3(0.3f, 0.2f, 0.5f);

        /**
         * aseta sumun väri
         */
        public static void SetColor(Vector3 color)
        {
            Fog.color = color;
        }

        /**
         * luo sumu
         */
        public static void CreateFog(FogMode mode, float start, float end, float density)
        {
            GL.Fog(FogParameter.FogMode, (int)mode);

            GL.Fogv(FogParameter.FogColor, new float[] { color.X, color.Y, color.Z });
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