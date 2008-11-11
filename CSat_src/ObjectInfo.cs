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
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public partial class ObjectInfo : Group
    {
        /// <summary>
        /// objektin nimi
        /// </summary>
        protected string name;
        public string Name { get { return name; } }

        public GLSL Shader = null;
        public bool UseShader = false;
        public bool DoubleSided = false;

        /// <summary>
        /// objektin paikka
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// objektin asento
        /// </summary>
        public Vector3 Rotation;
        /// <summary>
        /// jos tarvii korjata asento ennen liikuttelua ja kääntämistä
        /// </summary>
        public Vector3 FixRotation;

        public Vector3 Front = new Vector3(0, 0, 1), Right = new Vector3(1, 0, 0), Up = new Vector3(0, 1, 0);

        /// <summary>
        /// objektin keskikohta. tarvitaan frustum cullauksessa.
        /// </summary>
        public Vector3 ObjCenter;

        /// <summary>
        /// käännä y-akselin ympäri
        /// </summary>
        /// <param name="f"></param>
        public void TurnXZ(float f)
        {
            Rotation.Y -= f;
        }

        /// <summary>
        /// käännä x-akselin ympäri
        /// </summary>
        /// <param name="f"></param>
        public void LookUpXZ(float f)
        {
            Rotation.X -= f;
        }

        /// <summary>
        /// käännä z-akselin ympäri
        /// </summary>
        /// <param name="f"></param>
        public void RollXZ(float f)
        {
            Rotation.Z -= f;
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
        /// liikuta xz tasossa
        /// </summary>
        /// <param name="f">paljonko liikutaan eteen/taaksepäin</param>
        public void MoveXZ(float f)
        {
            Position.X -= ((float)Math.Sin(Rotation.Y * MathExt.PiOver180) * f);
            Position.Z -= ((float)Math.Cos(Rotation.Y * MathExt.PiOver180) * f);
        }

        /// <summary>
        /// liikuta xz-tasossa sivuttain
        /// </summary>
        /// <param name="f">paljonko liikutaan sivuttain</param>
        public void StrafeXZ(float f)
        {
            Position.X += ((float)Math.Cos(-Rotation.Y * MathExt.PiOver180) * f);
            Position.Z += ((float)Math.Sin(-Rotation.Y * MathExt.PiOver180) * f);
        }

        /*
         * Metodit täysin vapaaseen liikkumiseen (6DOF)
         * 
         */
        /// <summary>
        /// eteenpäin/taaksepäin f/-f  ja jos xzPlane on true, liikutaan vain xz tasolla
        /// </summary>
        /// <param name="f"></param>
        /// <param name="xzPlane"></param>
        public void MoveForward(float f, bool xzPlane)
        {
            if (xzPlane == false)
            {
                Position += (Front * f);
            }
            else
            {
                Position.X += Front.X * f;
                Position.Z += Front.Z * f;
            }
        }
        /// <summary>
        /// liikuta sivusuunnassa. jos xzPlane on true, liikutaan vain xz tasolla
        /// </summary>
        /// <param name="f"></param>
        /// <param name="xzPlane"></param>
        public void StrafeRight(float f, bool xzPlane)
        {
            if (xzPlane == false)
            {
                Position += (Right * f);
            }
            else
            {
                Position.X += Right.X * f;
                Position.Z += Right.Z * f;
            }
        }

        /// <summary>
        /// noustaan ylös joko up vektorin suuntaisesti (jos pyöritty, se voi osoittaa mihin vain),
        /// tai kohtisuoraan xz tasosta jos xzPlane on true.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="xzPlane"></param>
        public void MoveUp(float f, bool xzPlane)
        {
            if (xzPlane == false)
            {
                Position += (Up * f);
            }
            else
            {
                Position.Y += Up.Y * f;
            }
        }


        public void RotateX(float f)
        {
            Rotation.X -= f;
            Front *= (float)Math.Cos(f * MathExt.PiOver180);
            Up *= (float)Math.Sin(f * MathExt.PiOver180);

            Front += Up;
            Front.Normalize();

            Up = Vector3.Cross(Front, Right);
            Up = -Up;
        }

        public void RotateY(float f)
        {
            Rotation.Y -= f;
            Front *= (float)Math.Cos(f * MathExt.PiOver180);
            Right *= (float)Math.Sin(f * MathExt.PiOver180);

            Front -= Right;
            Front.Normalize();

            Right = Vector3.Cross(Front, Up);
        }

        public void RotateZ(float f)
        {
            Rotation.Z -= f;
            Right *= (float)Math.Cos(f * MathExt.PiOver180);
            Up *= (float)Math.Sin(f * MathExt.PiOver180);

            Right += Up;
            Right.Normalize();

            Up = Vector3.Cross(Front, Right);
            Up = -Up;
        }

        public void LookAt(Vector3 pos)
        {
            GL.LoadIdentity();
            Glu.LookAt(Position, pos, Up);
        }

        public void LookAt(float x, float y, float z)
        {
            GL.LoadIdentity();
            Glu.LookAt(Position.X, Position.Y, Position.Z, x, y, z, Up.X, Up.Y, Up.Z);
        }

    }
}
