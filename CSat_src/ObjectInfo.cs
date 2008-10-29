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
using OpenTK.Math;

namespace CSat
{

    public class ObjectInfo : Group
    {
        protected string name; // objektin nimi
        public string Name { get { return name; } }

        public Vector3 position; // objektin paikka
        public Vector3 rotation; // objektin asento
        public Vector3 fixRotation; // jos tarvii korjata asento ennen liikuttelua ja k‰‰nt‰mist‰

        public Vector3 wpos; // world coords

        public Vector3 view;
        public Vector3 front, right, up;

        /// <summary>
        /// k‰‰nn‰ y-akselin ymp‰ri
        /// </summary>
        /// <param name="f"></param>
        public void TurnXZ(float f)
        {
            rotation.Y -= f;
        }

        /// <summary>
        /// k‰‰nn‰ x-akselin ymp‰ri
        /// </summary>
        /// <param name="f"></param>
        public void LookUpXZ(float f)
        {
            rotation.X -= f;
        }

        /// <summary>
        /// k‰‰nn‰ z-akselin ymp‰ri
        /// </summary>
        /// <param name="f"></param>
        public void RollXZ(float f)
        {
            rotation.Z -= f;
        }

        public void MoveXZ(float forward, float strafe)
        {
            if (forward != 0)
            {
                MoveXZ(forward);
            }
            if (strafe != 0)
            {
                StrafeXZ(strafe);
            }
        }

        /// <summary>
        /// liikuta objektia xz tasossa
        /// </summary>
        /// <param name="f">paljonko liikutaan eteen/taaksep‰in</param>
        public void MoveXZ(float f)
        {
            position.X -= ((float)Math.Sin(rotation.Y * MathExt.PiOver180) * f);
            position.Z -= ((float)Math.Cos(rotation.Y * MathExt.PiOver180) * f);
        }

        /// <summary>
        /// liikuta xz-tasossa
        /// </summary>
        /// <param name="f">paljonko liikutaan sivuttain</param>
        public void StrafeXZ(float f)
        {
            position.X += ((float)Math.Cos(-rotation.Y * MathExt.PiOver180) * f);
            position.Z += ((float)Math.Sin(-rotation.Y * MathExt.PiOver180) * f);
        }

        /*
         * Metodit t‰ysin vapaaseen liikkumiseen (6DOF)
         * 
         */

        /// <summary>
        /// eteenp‰in/taaksep‰in f/-f
        /// </summary>
        /// <param name="f"></param>
        public void MoveForward(float f)
        {
            f = -f;
            position += (front * f);

        }
        public void StrafeRight(float f)
        {
            position += (right * f);
        }

        public void MoveUp(float f)
        {
            position += (up * f);
        }


        // todo 6dof rotate

    }
}
