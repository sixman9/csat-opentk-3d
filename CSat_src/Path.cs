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
    public partial class ObjectInfo : Group
    {
        Vector3[] path = null;
        public bool Looping = true;
        public float Time = 0;
        protected bool lookAtNextPoint = false;
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

            // kamera asetetaan heti
            if (this is Camera)
            {
                // laske kohta johon katsotaan
                if (lookAtNextPoint)
                {
                    Front = (path[(v2 + 1) % path.Length]) - p2;
                    Front = p2 + (Front * d);
                }

                GL.LoadIdentity();
                Glu.LookAt(Position, Front, Up);
                GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
                Util.CopyArray(ref Util.ModelMatrix, ref Matrix);
            }
            else
            {
                if (lookAtNextPoint)
                {
                    Front = (path[(v2 + 1) % path.Length]) - p2;
                    Front = p2 + (Front * d);


                    // otetaan käännetyn objektin matriisi talteen
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    Glu.LookAt(Position, Front, Up);
                    GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
                    Util.CopyArray(ref Util.ModelMatrix, ref Matrix);
                    GL.PopMatrix();

                    Rotation.X = (float)Math.Atan2(-Matrix[9], Matrix[5]) / MathExt.PiOver180;
                    Rotation.Y = -(float)Math.Atan2(-Matrix[2], Matrix[0]) / MathExt.PiOver180;
                    Rotation.Z = (float)Math.Asin(Matrix[1]) / MathExt.PiOver180;

                    // TODO huono tapa! parempi ottaa lookat ja matrix talteen
                    // ja koittaa käyttää sitä asettamaan objektin asento.
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

                // if(closed) tmpv.Add(path.vertex[ 0 ]);
                // korvataan alkuperäinen reitti uudella kaarella
                path = null;
                path = new Vector3[tmpv.Count];
                for (int q = 0; q < path.Length; q++)
                    path[q] = (Vector3)tmpv[q];

            }
            Log.WriteDebugLine("NewPath: " + path.Length, 2);
        }

    }
}
