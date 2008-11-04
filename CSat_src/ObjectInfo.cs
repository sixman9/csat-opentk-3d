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
    public class ObjectInfo : Group
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
        /// objektin paikka ja asento world coordinaateissa (esim frustum cullaus vaatii tämän)
        /// </summary>
        public float[] WMatrix = null;
        /// <summary>
        /// objektin paikka ja asento kamerasta katsottuna
        /// </summary>
        public float[] Matrix = null;

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

        public Vector3 View; // mihin katsotaan
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
        public void MoveForward(float f, bool xzPlane)
        {
            f = -f;
            if (xzPlane == false) Position += (Front * f);
            else
            {
                Position.X += Front.X * f;
                Position.Z += Front.Z * f;
                View.X += Front.X * f;
                View.Z += Front.Z * f;
            }
        }
        /// <summary>
        /// liikuta sivusuunnassa. jos xzPlane on true, liikutaan vain xz tasolla
        /// </summary>
        /// <param name="f"></param>
        /// <param name="xzPlane"></param>
        public void StrafeRight(float f, bool xzPlane)
        {
            if (xzPlane == false) Position += (Right * f);
            else
            {
                Position.X += Right.X * f;
                Position.Z += Right.Z * f;
                View.X += Right.X * f;
                View.Z += Right.Z * f;
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
            if (xzPlane == false) Position += (Up * f);
            else
            {
                Position.Y += Up.Y * f;
                View.Y += Up.Y * f;
            }
        }

        /// <summary>
        /// pyörittää 6dof kameraa haluamaan asentoon
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void RotateXYZ(float angle, float x, float y, float z)
        {
            // suuntavektori
            Vector3 tmp = View - Position;
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            Vector3 nview;
            nview.X = (cos + (1 - cos) * x * x) * tmp.X;
            nview.X += ((1 - cos) * x * y - z * sin) * tmp.Y;
            nview.X += ((1 - cos) * x * z + y * sin) * tmp.Z;
            nview.Y = ((1 - cos) * x * y + z * sin) * tmp.X;
            nview.Y += (cos + (1 - cos) * y * y) * tmp.Y;
            nview.Y += ((1 - cos) * y * z - x * sin) * tmp.Z;
            nview.Z = ((1 - cos) * x * z - y * sin) * tmp.X;
            nview.Z += ((1 - cos) * y * z + x * sin) * tmp.Y;
            nview.Z += (cos + (1 - cos) * z * z) * tmp.Z;

            View = Position + nview;
        }

        /// <summary>
        /// laske objektien paikat ja asennot. 
        /// otetaan matriisi talteen.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="pos"></param>
        public void CalcAndGetMatrix(ref float[] matrix, Vector3 pos)
        {
            // liikuta haluttuun kohtaan
            GL.Translate(Position.X + pos.X, Position.Y + pos.Y, Position.Z + pos.Z);
            GL.Rotate(Rotation.X, 1, 0, 0);
            GL.Rotate(Rotation.Y, 0, 1, 0);
            GL.Rotate(Rotation.Z, 0, 0, 1);
            GL.Rotate(FixRotation.X, 1, 0, 0);
            GL.Rotate(FixRotation.Y, 0, 1, 0);
            GL.Rotate(FixRotation.Z, 0, 0, 1);

            GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
            Util.CopyArray(ref Util.ModelMatrix, ref matrix);
        }


        // reitti ---

        Vector3[] path = null;
        public bool Looping = true;
        public float Time = 0;
        public bool lookAtNextPoint = true;
        public void FollowPath(ref Object3D pathObj, bool loop, bool lookAtNextPoint)
        {
            path = pathObj.GetObject(0).Vertex;
            Position = path[0];
            Looping = loop;
            this.lookAtNextPoint = lookAtNextPoint;

            if (this is Camera)
            {
                Camera.cam = (Camera)this;
            }
        }

        public void UpdatePath(float updateTime)
        {
            Time += updateTime;

            int v1 = (int)Time;
            int v2 = v1 + 1;
            if ((v1 >= path.Length || v2 >= path.Length) && Looping == false) return;

            v1 %= path.Length;
            v2 %= path.Length;

            // laske Position reitillä
            Vector3 p1 = path[v1];
            Vector3 p2 = path[v2];
            Vector3 p = p2 - p1;
            float d = Time - (int)Time;
            p *= d;
            Position = p1 + p;

            Vector3 to;

            // laske kohta johon katsotaan
            if (lookAtNextPoint)
            {
                to = (path[(v2 + 1) % path.Length]) - p2;
                to = p2 + (to * d);
            }
            else to = View;

            // kamera asetetaan heti
            if (this is Camera)
            {
                GL.LoadIdentity();
                Glu.LookAt(Position, to, Up);
                GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
                Util.CopyArray(ref Util.ModelMatrix, ref Matrix);
            }
            else
            {
                if (lookAtNextPoint)
                {
                    // otetaan käännetyn objektin matriisi talteen
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    Glu.LookAt(Position, to, Up);
                    GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
                    Util.CopyArray(ref Util.ModelMatrix, ref Matrix);
                    GL.PopMatrix();
                }
            }
        }

        public void MakeCurve(int lod)
        {
            if (path == null) return;

            for (int c = 0; c < lod; c++)
            {
                ArrayList tmpv = new ArrayList();
                tmpv.Add(path[0]); // eka vertex talteen

                for (int q = 0; q < path.Length - 1; q++)
                {
                    Vector3 p0 = path[q];
                    Vector3 p1 = path[q + 1];
                    Vector3 Q, R;

                    // average the 2 original points to create 2 new points. For each
                    // CV, another 2 verts are created.
                    Q.X = 0.75f * p0.X + 0.25f * p1.X;
                    Q.Y = 0.75f * p0.Y + 0.25f * p1.Y;
                    Q.Z = 0.75f * p0.Z + 0.25f * p1.Z;

                    R.X = 0.25f * p0.X + 0.75f * p1.X;
                    R.Y = 0.25f * p0.Y + 0.75f * p1.Y;
                    R.Z = 0.25f * p0.Z + 0.75f * p1.Z;

                    tmpv.Add(Q);
                    tmpv.Add(R);
                }

                tmpv.Add(path[path.Length - 1]); // vika vertex

                // if(closed) tmpv.add(path.vertex[ 0 ]);
                // korvataan alkuperäinen reitti uudella kaarella
                path = null;
                path = new Vector3[tmpv.Count];
                for (int q = 0; q < path.Length; q++)
                    path[q] = (Vector3)tmpv[q];

            }
            Log.WriteDebugLine("NewPath: " + path.Length);
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
