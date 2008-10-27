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

/* 
 * jokaiseen objektiin kuuluu group (ryhmä) joten jokaiseen voidaan liittää
 * toisia objekteja jotka liikkuu alkup objektin mukana.
 * 
 * hoitaa myös objektien törmäystarkistukset ja groupin rendauksen.
 * 
 * object2d renderoidaan vain render3d metodilla, ei render2d.
 * 
 */

using System;
using System.Collections;
using OpenTK.Math;
using OpenTK.Graphics;

namespace CSat
{
    public class Group
    {
        string groupName = "group";

        public Group() { }

        public Group(string name)
        {
            groupName = name;
        }

        protected ArrayList objects = new ArrayList();
        public Object Objects
        {
            get
            {
                return objects.ToArray();
            }
        }

        public void Add(Object obj)
        {
            objects.Add(obj);
            Log.WriteDebugLine(obj + " added to " + groupName + ".");
        }

        public void Remove(Object obj)
        {
            objects.Remove(obj);
            Log.WriteDebugLine(obj + " removed from " + groupName + ".");
        }

        public void Remove(string name)
        {
            objects.Remove(SearchObject(name));
            Log.WriteDebugLine(name + " removed from " + groupName + ".");
        }

        public Object SearchObject(string name)
        {
            for (int q = 0; q < objects.Count; q++)
            {
                Object3D o = (Object3D)objects[q];
                if (o.Name == name)
                    return o;
            }
            return null;
        }

        /** 
         * palauttaa true jos vektori oldpos->newpos välissä poly. sopii esim kameralle
         * dontTestThis - objekti jota ei testata (eli liikuteltava objekti tai null jos esim kamera)
         */
        public bool CheckCollision(Vector3 start, Vector3 end, ref Object3D dontTestThis)
        {
            Vector3 len = start - end;
            if (len.X == 0 && len.Y == 0 && len.Z == 0) return false;

            for (int q = 0; q < objects.Count; q++)
            {
                if (objects[q] is Object3D)
                {
                    if (objects[q] != dontTestThis)
                        if (Intersection.CheckIntersection(ref start, ref end, (Object3D)objects[q]) == true) return true;

                }

            }
            return false;
        }

        /**
         * palauttaa true jos objektin boundingboxin joku vertexi osuu johonkin polyyn
         */
        public bool CheckCollisionBB(Vector3 start, Vector3 end, ref Object3D obj)
        {
            Vector3 len = start - end;
            if (len.X == 0 && len.Y == 0 && len.Z == 0) return false;

            for (int q = 0; q < objects.Count; q++)
            {
                if (objects[q] is Object3D)
                {
                    if (objects[q] != obj)
                    {
                        // jos objekti käännetty (rotation ja/tai fixRotation != 0), pitää ottaa huomioon ja kääntää bounding boxia.
                        Matrix4 outm = new Matrix4();
                        Vector3 rot = -(obj.rotation + obj.fixRotation);
                        if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
                        {
                            rot = rot * MathExt.HalfPI;
                            Matrix4 mx = Matrix4.RotateX(rot.X);
                            Matrix4 my = Matrix4.RotateY(rot.Y);
                            Matrix4 mz = Matrix4.RotateZ(rot.Z);
                            Matrix4 outm0;
                            Matrix4.Mult(ref mx, ref my, out outm0);
                            Matrix4.Mult(ref outm0, ref mz, out outm);
                        }
                        // tarkistetaan bounding boxin kulmat, yrittääkö läpäistä jonkun polyn
                        for (int c = 0; c < 8; c++)
                        {
                            Vector3 v = obj.ObjectBoundingVolume.Corner[c];

                            Vector3 vout;
                            if (Math.Abs(rot.X + rot.Y + rot.Z) > 0.001f)
                            {
                                vout = MathExt.VectorMatrixMult(ref v, ref outm);
                            }
                            else vout = v;

                            vout = vout + obj.position;
                            Vector3 endv = vout + len;

                            if (Intersection.CheckIntersection(ref vout, ref endv, (Object3D)objects[q]) == true)
                            {
                                return true;
                            }
                        }

                    }
                }
            }
            return false;
        }




        /** 
         * laske objekteille paikat. pitää kutsua jos 
         * on liittänyt objekteihin toisia objekteja.
         */
        public void CalculatePositions()
        {
            GL.PushMatrix();
            GL.LoadIdentity();
            CalcPos();
            GL.PopMatrix();
        }

        static float[] modelMatrix = new float[16];
        void CalcPos()
        {
            for (int q = 0; q < objects.Count; q++)
            {
                if (objects[q] is Skybox || objects[q] is IModel) continue;

                ObjectInfo o = (ObjectInfo)objects[q];

                GL.PushMatrix();
                // liikuta haluttuun kohtaan
                GL.Translate(o.position.X, o.position.Y, o.position.Z);
                GL.Rotate(o.rotation.X, 1, 0, 0);
                GL.Rotate(o.rotation.Y, 0, 1, 0);
                GL.Rotate(o.rotation.Z, 0, 0, 1);

                // korjaa asento
                GL.Rotate(o.fixRotation.X, 1, 0, 0);
                GL.Rotate(o.fixRotation.Y, 0, 1, 0);
                GL.Rotate(o.fixRotation.Z, 0, 0, 1);

                GL.GetFloat(GetPName.ModelviewMatrix, modelMatrix);
                o.wpos.X = modelMatrix[12];
                o.wpos.Y = modelMatrix[13];
                o.wpos.Z = modelMatrix[14];

                if (o.objects.Count > 0)
                {
                    o.CalcPos();
                }
                GL.PopMatrix();
            }
        }

        public void Render()
        {
            CalculatePositions();
            RenderTree();
        }

        public void RenderTree()
        {
            for (int q = 0; q < objects.Count; q++)
            {
                if (objects[q] is Object3D)
                {
                    Object3D o = (Object3D)objects[q];
                    o.Render();
                }
                else if (objects[q] is AnimatedModel)
                {
                    AnimatedModel o = (AnimatedModel)objects[q];
                    o.Render();
                }
                else if (objects[q] is Billboard)
                {
                    Billboard o = (Billboard)objects[q];
                    o.Render();
                }
                else if (objects[q] is Object2D)
                {
                    Object2D o = (Object2D)objects[q];
                    o.Render3D();
                }
                else if (objects[q] is Skybox)
                {
                    Skybox o = (Skybox)objects[q];
                    o.Render();
                }

            }
        }
    }
}
