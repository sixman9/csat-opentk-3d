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

        Vector3[] corner = new Vector3[8]; // kulmat
        public Vector3[] Corner { get { return corner; } }

        // bounding boxin normaalit "sisällepäin"
        Vector4[] planes = new Vector4[6];

        float r;
        public float R { get { return r; } }

        byte mode = Sphere; // miten tarkistetaan frustumiin (BOX/SPHERE)
        public byte Mode
        {
            set { mode = value; }
            get { return mode; }
        }

        public void FindMinMax(Mesh m)
        {
            for (int c = 0; c < m.vertexInd.Count; c++)
            {
                int q = (int)m.vertexInd[c];
                if (m.object3d.Vertex[q].X < min.X) min.X = m.object3d.Vertex[q].X;
                if (m.object3d.Vertex[q].Y < min.Y) min.Y = m.object3d.Vertex[q].Y;
                if (m.object3d.Vertex[q].Z < min.Z) min.Z = m.object3d.Vertex[q].Z;

                if (m.object3d.Vertex[q].X > max.X) max.X = m.object3d.Vertex[q].X;
                if (m.object3d.Vertex[q].Y > max.Y) max.Y = m.object3d.Vertex[q].Y;
                if (m.object3d.Vertex[q].Z > max.Z) max.Z = m.object3d.Vertex[q].Z;
            }
        }

        public void CalcR()
        {
            Vector3 v = new Vector3(max - min);
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

        // objektin bounding box
        public void CalcObjectBounds(Object3D obj)
        {
            for (int q = 0; q < obj.Meshes.Length; q++)
            {
                FindMinMax(obj.Meshes[q]);
            }
            CalcR();
            SetCorners();
            CalcPlanes();
        }

        // mesh bounding box
        public void CalcMeshBounds(Object3D obj)
        {
            for (int q = 0; q < obj.Meshes.Length; q++)
            {
                min = new Vector3(99999, 99999, 99999);
                max = new Vector3(-99999, -99999, -99999);
                FindMinMax(obj.Meshes[q]);

                Vector3 v = new Vector3(max - min);
                r = v.Length;
                v.Scale(.5f, .5f, .5f);
                obj.Meshes[q].center = new Vector3(min + v); // meshin keskipiste
            }

            SetCorners();
            CalcPlanes();
        }

        void CalcPlanes()
        {
            // todo laske 6 tasonormaalia
            //Util.CalcPlane(
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

        // jos vertex on jonkun tason "takana", vartex ei ole boxissa. normaalit osoittaa "sisäänpäin".
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
