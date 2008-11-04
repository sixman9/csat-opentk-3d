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
 * jos groupissa on Object2D objekteja, ne renderoidaan sen Render3D metodilla.
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
        public ArrayList Objects
        {
            get
            {
                return objects;
            }
        }

        /// <summary>
        /// calculatepositions ottaa kaikki täysin näkyvät objektit tähän talteen renderointia varten
        /// </summary>
        static protected ArrayList visibleObjects = new ArrayList();
        /// <summary>
        /// calculatepositions ottaa kaikki ruudulla olevat läpikuultavat objektit tähän talteen renderointia varten
        /// </summary>
        static protected ArrayList translucentObjects = new ArrayList();

        /// <summary>
        /// laske objekteille world coordinates (WMatrix)
        /// </summary>
        public void CalculateWorldCoords()
        {
            for (int q = 0; q < objects.Count; q++)
            {
                if (objects[q] is Skybox || objects[q] is Skydome) continue;

                GL.PushMatrix();

                ObjectInfo o = (ObjectInfo)objects[q];
                o.CalcAndGetMatrix(ref o.WMatrix, o.ObjCenter);
                if (o.objects.Count > 0) o.CalculateWorldCoords();

                GL.PopMatrix();
            }
        }

        /// <summary>
        /// laske objekteille paikat (Matrix).
        /// ota listoihin näkyvät obut.
        /// </summary>
        public void CalculateCoords()
        {
            for (int q = 0; q < objects.Count; q++)
            {
                GL.PushMatrix();

                if (objects[q] is Skybox || objects[q] is Skydome) // nämä on aina näkyviä
                {
                    visibleObjects.Add(objects[q]);
                }
                else if (objects[q] is Object3D)
                {
                    Object3D o = (Object3D)objects[q];

                    o.CalcAndGetMatrix(ref o.Matrix, Vector3.Zero);
                    if (o.objects.Count > 0) o.CalculateCoords();

                    // tarkista onko objekti näkökentässä
                    if (Frustum.ObjectInFrustum(o.WMatrix[12], o.WMatrix[13], o.WMatrix[14], o.MeshBoundingVolume))
                    {
                        if (o.IsTranslucent == true)
                        {
                            translucentObjects.Add(objects[q]);
                        }
                        else
                        {
                            visibleObjects.Add(objects[q]);
                        }
                    }
                }
                else if (objects[q] is AnimatedModel)
                {
                    AnimatedModel o = (AnimatedModel)objects[q];

                    o.CalcAndGetMatrix(ref o.Matrix, Vector3.Zero);
                    if (o.objects.Count > 0) o.CalculateCoords();

                    // tarkista onko objekti näkökentässä
                    if (Frustum.ObjectInFrustum(o.WMatrix[12], o.WMatrix[13], o.WMatrix[14], o.GetBoundingVolume()))
                    {
                        visibleObjects.Add(objects[q]);
                    }
                }
                else if(objects[q] is Particles)
                {
                    Particles o = (Particles)objects[q];
                    o.CalcAndGetMatrix(ref o.Matrix, Vector3.Zero);
                    if (o.objects.Count > 0) o.CalculateCoords();
                    if (o.IsTranslucent == false) visibleObjects.Add(objects[q]);
                    else translucentObjects.Add(objects[q]);
                }
                else // loput eli billboard, object2d, ..
                {
                    ObjectInfo o = (ObjectInfo)objects[q];
                    o.CalcAndGetMatrix(ref o.Matrix, Vector3.Zero);
                    if (o.objects.Count > 0) o.CalculateCoords();
                    visibleObjects.Add(objects[q]);
                }
                
                // TODO piutäis tsekata onko ruudulla: billboards, object2d

                GL.PopMatrix();
            }

            // TODO
            // translucentObjects  array:  pitää sortata kauimmaisesta lähimpään
        }

        /// <summary>
        /// renderoi kaikki mitä grouppiin kuuluu
        /// </summary>
        public void Render()
        {
            visibleObjects.Clear();
            translucentObjects.Clear();

            GL.PushMatrix(); // kameramatrix talteen
            GL.LoadIdentity();
            CalculateWorldCoords();
            GL.PopMatrix(); // kameraan takas

            CalculateCoords();

            RenderArrays();
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

        /// <summary>
        /// renderoi näkyvät objektit
        /// </summary>
        public void RenderArrays()
        {
            GL.PushMatrix();
            ArrayList cur = (ArrayList)visibleObjects.Clone();
            int c = 0;
            for (int q = 0; q < visibleObjects.Count + translucentObjects.Count; q++, c++)
            {
                if (q == visibleObjects.Count) // listan vaihto, rendataan läpikuultavat
                {
                    c = 0;
                    cur = (ArrayList)translucentObjects.Clone(); 
                }

                // renderoi oikea objekti:

                if (cur[c] is Object3D)
                {
                    Object3D o = (Object3D)cur[c];
                    GL.LoadMatrix(o.Matrix);
                    o.RenderFast();
                }
                else if (cur[c] is AnimatedModel)
                {
                    AnimatedModel o = (AnimatedModel)cur[c];
                    GL.LoadMatrix(o.Matrix);
                    o.RenderFast();
                }
                else if (cur[c] is Object2D)
                {
                    Object2D o = (Object2D)cur[c];
                    GL.LoadMatrix(o.Matrix);
                    o.Render3D();
                }
                else if (cur[c] is Billboard)
                {
                    Billboard o = (Billboard)cur[c];
                    GL.LoadMatrix(o.Matrix);
                    o.Render();
                }
                else if (cur[c] is Particles)
                {
                    Particles o = (Particles)cur[c];
                    GL.LoadMatrix(o.Matrix);
                    o.Render();
                }
                else if (cur[c] is Skybox)
                {
                    Skybox o = (Skybox)cur[c];
                    o.Render();
                }
                else if (cur[c] is Skydome)
                {
                    Skydome o = (Skydome)cur[c];
                    o.Render();
                }

            }
            GL.PopMatrix();
        }

    }
}
