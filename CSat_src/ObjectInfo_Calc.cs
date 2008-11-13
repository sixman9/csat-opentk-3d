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
        /// objektin paikka ja asento world coordinaateissa (esim frustum cullaus vaatii tämän)
        /// </summary>
        public float[] WMatrix = null;
        /// <summary>
        /// objektin paikka ja asento kamerasta katsottuna
        /// </summary>
        public float[] Matrix = null;

        /// <summary>
        /// laske objektien paikat ja asennot. 
        /// otetaan matriisi talteen.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="pos"></param>
        public void CalcAndGetMatrix(ref float[] matrix, Vector3 pos)
        {
            // liikuta haluttuun kohtaan
            GL.Translate(Position + pos);
            if (lookAtNextPoint) GL.MultMatrix(lookAtMatrix);
            GL.Rotate(Rotation.X, 1, 0, 0);
            GL.Rotate(Rotation.Y, 0, 1, 0);
            GL.Rotate(Rotation.Z, 0, 0, 1);
            GL.Rotate(FixRotation.X, 1, 0, 0);
            GL.Rotate(FixRotation.Y, 0, 1, 0);
            GL.Rotate(FixRotation.Z, 0, 0, 1);
            GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
            Util.CopyArray(ref Util.ModelMatrix, ref matrix);
        }

        /// <summary>
        /// laske objekteille world coordinates (WMatrix)
        /// </summary>
        public void CalculateWorldCoords(object obj)
        {
            GL.PushMatrix();
            if (obj is Skybox || obj is Skydome) { }
            else
            {
                CalcAndGetMatrix(ref WMatrix, ObjCenter);
            }

            for (int q = 0; q < Objects.Count; q++)
            {
                GL.PushMatrix();
                ObjectInfo o = (ObjectInfo)Objects[q];
                o.CalcAndGetMatrix(ref o.WMatrix, o.ObjCenter);
                if (o.Objects.Count > 0) o.CalculateWorldCoords(Objects[q]);
                GL.PopMatrix();
            }
            GL.PopMatrix();
        }

        void CheckObject(Object obj)
        {
            if (obj is Object3D)
            {
                Object3D o = (Object3D)obj;
                // tarkista onko objekti näkökentässä
                if (Frustum.ObjectInFrustum(o.WMatrix[12], o.WMatrix[13], o.WMatrix[14], o.MeshBoundingVolume))
                {
                    if (o.IsTranslucent == true)
                    {
                        translucentObjects.Add(obj);
                    }
                    else
                    {
                        visibleObjects.Add(obj);
                    }
                }
            }
            else if (obj is AnimatedModel)
            {
                AnimatedModel o = (AnimatedModel)obj;

                // tarkista onko objekti näkökentässä 
                if (Frustum.ObjectInFrustum(o.WMatrix[12], o.WMatrix[13], o.WMatrix[14], o.GetBoundingVolume()))
                {
                    visibleObjects.Add(obj);
                }
            }
            else if (obj is Particles)
            {
                Particles o = (Particles)obj;
                if (o.IsTranslucent == false) visibleObjects.Add(obj);
                else translucentObjects.Add(obj);
            }
            else // loput eli billboard, Object2d, ..
            {
                // TODO pitäis tsekata onko ruudulla: billboards, Object2d
                ObjectInfo o = (ObjectInfo)obj;
                visibleObjects.Add(obj);
            }
        }

        /// <summary>
        /// laske objekteille paikat (Matrix).
        /// ota listoihin näkyvät obut.
        /// </summary>
        public void CalculateCoords(Object obj)
        {
            GL.PushMatrix();
            if (obj is Skybox || obj is Skydome) // skybox/skydome AINA ekana.
            {
                visibleObjects.Add(obj);
            }
            else
            {
                CalcAndGetMatrix(ref Matrix, Vector3.Zero);
                CheckObject(obj);
            }

            for (int q = 0; q < Objects.Count; q++)
            {
                GL.PushMatrix();

                ObjectInfo oi = (ObjectInfo)Objects[q];
                oi.CalcAndGetMatrix(ref oi.Matrix, Vector3.Zero);
                oi.CheckObject(Objects[q]);

                if (oi.Objects.Count > 0) oi.CalculateCoords(Objects[q]);

                GL.PopMatrix();
            }
            GL.PopMatrix();
        }
    }
}
