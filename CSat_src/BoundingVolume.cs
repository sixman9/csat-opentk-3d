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
 * BoundingVolume määrää alueen jonka sisällä objekti on. Tätä
 * käytetään esim collision detectionissa, frustum cullingis..
 * 
 */
using OpenTK.Math;

namespace CSat
{
    public class BoundingVolume
    {
        public const int None = 0, Box = 1, Sphere = 2;

        Vector3 min = new Vector3(99999, 99999, 99999), max = new Vector3(-99999, -99999, -99999);
        public Vector3 Min { get { return min; } }
        public Vector3 Max { get { return max; } }

        /// <summary>
        /// bboxin kulmat
        /// </summary>
        Vector3[] corner = new Vector3[8];
        public Vector3[] Corner { get { return corner; } }

        /// <summary>
        /// bounding boxin normaalit "sisällepäin"
        /// </summary>
        Vector4[] planes = new Vector4[6];

        float r = 999999;
        public float R { get { return r; } }

        /// <summary>
        /// miten tarkistetaan frustumiin (Box/Sphere)
        /// </summary>
        public byte Mode = Sphere;

        public void FindMinMax(Object3D o, Vector3 pos)
        {
            for (int c = 0; c < o.VertexInd.Count; c++)
            {
                int q = (int)o.VertexInd[c];
                Vector3 v = o.Vertex[q] + pos;

                if (v.X < min.X) min.X = v.X;
                if (v.Y < min.Y) min.Y = v.Y;
                if (v.Z < min.Z) min.Z = v.Z;

                if (v.X > max.X) max.X = v.X;
                if (v.Y > max.Y) max.Y = v.Y;
                if (v.Z > max.Z) max.Z = v.Z;
            }
        }

        public void CalcR()
        {
            Vector3 v = max - min;
            r = v.Length;
        }

        public void CreateBoundingBox(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
            CalcR();
            SetCorners();
            CalcPlanes();
        }

        /// <summary>
        /// ota objektin + childien bounding box.
        /// 
        /// jos objekteja liittää toisiinsa, tämä pitäis suorittaa että lasketaan uusi bbox.
        /// </summary>
        /// <param name="obj"></param>
        public void CalcBounds(Object3D obj)
        {
            for (int q = 0; q < obj.Objects.Count; q++)
            {
                Object3D child = (Object3D)obj.Objects[q];
                FindMinMax(child, obj.Position);
                CalcBounds(child);
            }
            CalcR();
            SetCorners();
            CalcPlanes();
        }

        /// <summary>
        /// ota objektin bounding box
        /// </summary>
        /// <param name="obj"></param>
        public void CalcMeshBounds(Object3D obj)
        {
            min = new Vector3(99999, 99999, 99999);
            max = new Vector3(-99999, -99999, -99999);

            FindMinMax(obj, new Vector3(0, 0, 0));

            Vector3 v = max - min;
            r = v.Length;

            v.Scale(0.5f, 0.5f, 0.5f); // puoleen väliin
            obj.ObjCenter = min + v; // joka on objektin keskikohta

            SetCorners();
            CalcPlanes();
        }

        /// <summary>
        /// laske tasonormaalit bounding boxille
        /// </summary>
        void CalcPlanes()
        {
            // laske 6 tasonormaalia
            MathExt.CalcPlane(ref corner[0], ref corner[1], ref corner[2], out planes[0]); // min z
            MathExt.CalcPlane(ref corner[4], ref corner[5], ref corner[6], out planes[1]); // max z

            MathExt.CalcPlane(ref corner[0], ref corner[3], ref corner[5], out planes[2]); // min x
            MathExt.CalcPlane(ref corner[4], ref corner[1], ref corner[7], out planes[3]); // max x

            MathExt.CalcPlane(ref corner[0], ref corner[1], ref corner[7], out planes[4]); // min y
            MathExt.CalcPlane(ref corner[4], ref corner[3], ref corner[5], out planes[5]); // max y

            // todo tarkista
        }

        void SetCorners()
        {
            // aseta kulmat
            corner[0] = min;

            corner[1] = min;
            corner[1].X = max.X;

            corner[2] = min;
            corner[2].X = max.X;
            corner[2].Y = max.Y;

            corner[3] = min;
            corner[3].Y = max.Y;

            corner[4] = max;

            corner[5] = max;
            corner[5].X = min.X;

            corner[6] = max;
            corner[6].X = min.X;
            corner[6].Y = min.Y;

            corner[7] = max;
            corner[7].Y = min.Y;
        }

        /// <summary>
        /// jos vertex on jonkun tason "takana", vartex ei ole boxissa. normaalit osoittaa "sisäänpäin".
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool PointInBox(float x, float y, float z)
        {
            // tasoyhtälö: A*x + B*y + C*z + D = 0
            // ABC on normaalin X, Y ja Z
            // D on tason etäisyys origosta
            // =0 vertex on tasolla
            // <0 tason takana
            // >0 tason edessä
            for (int a = 0; a < 6; a++)
            {
                // jos vertex jonkun tason takana, niin palauta false (vertex boxin ulkopuolella)
                if (planes[a].X * x + planes[a].Y * y + planes[a].Z * z + planes[a].W <= 0)
                {
                    return false;
                }
            }

            // boxin sisällä
            return true;
        }

    }
}
