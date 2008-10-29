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
//#define MOREDEBUG

using System;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public struct Vertex
    {
        public Vector2 uv1, uv2; // 2 texturekoordinaattia max
        public Vector2 temp; // tilaa viem‰s, mit‰s t‰h‰n.
        public Vector3 normal;
        public Vector4 color;
        public Vector3 vertex;

        public Vertex(Vector3 vertex, Vector3 normal, Vector2 uv1)
        {
            this.vertex = vertex;
            this.normal = normal;
            this.uv1 = uv1;
            color = new Vector4(1, 1, 1, 1);
            uv2 = uv1;
            temp = uv1;
        }

    } // 64 tavua


    public class VBO
    {
        private const int VERTEX_SIZE = 64;

        private int vertexID = 0, indexID = 0;
        private BufferUsageHint usage = BufferUsageHint.StaticDraw;
        
        /// <summary>
        /// liput, mit‰ kaikkea k‰ytet‰‰n (normal, uv, color)
        /// </summary>
        private short vertexFlags = 0; 
        int numOfIndices = 0;

        public VBO() { }
        public VBO(BufferUsageHint usage)
        {
            this.usage = usage;
        }

        /// <summary>
        /// luo VBO. monelleko vertexille ja indexille varataan tilaa
        /// </summary>
        /// <param name="vertSize"></param>
        /// <param name="indSize"></param>
        void AllocVBO(int vertSize, int indSize)
        {
            GL.GenBuffers(1, out vertexID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertSize * VERTEX_SIZE), null, usage);

            GL.GenBuffers(1, out indexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indSize * sizeof(int)), null, usage);

            numOfIndices = indSize;
#if MOREDEBUG
            Log.WriteDebugLine("AllocVBO: (verts:" + vertSize + " indices:" + indSize + ")");
#endif
        }

        public void Dispose()
        {
            if (indexID != 0) GL.DeleteBuffers(1, ref indexID);
            if (vertexID != 0) GL.DeleteBuffers(1, ref vertexID);
            vertexID = 0;
            indexID = 0;
        }

        /// <summary>
        /// kopioi objekti vbo:hon
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="uvs1"></param>
        /// <param name="uvs2"></param>
        /// <param name="uvs3"></param>
        /// <param name="colors"></param>
        /// <param name="obj"></param>
        public void DataToVBO(Vector3[] vertices, Vector3[] normals, Vector2[] uvs1, Vector2[] uvs2, Vector2[] uvs3, Vector4[] colors, ref Object3D obj)
        {
            int[] ind = new int[obj.vertexInd.Count];
            Vector3[] vert = new Vector3[ind.Length];
            Vector3[] norm = new Vector3[ind.Length];
            Vector2[] uv = new Vector2[ind.Length];

            for (int q = 0; q < ind.Length; q++)
            {
                vert[q] = vertices[(int)obj.vertexInd[q]];
                norm[q] = normals[(int)obj.normalInd[q]];
                uv[q] = uvs1[(int)obj.uvInd[q]];
            }

            // index taulukko
            for (int q = 0; q < ind.Length; q++) ind[q] = q;

            DataToVBO(vert, ind, norm, uv, null, null, null);
        }

        /// <summary>
        /// kopioi objekti vbo:hon
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="normals"></param>
        /// <param name="uvs1"></param>
        /// <param name="uvs2"></param>
        /// <param name="temp"></param>
        /// <param name="colors"></param>
        public void DataToVBO(Vector3[] vertices, int[] indices, Vector3[] normals, Vector2[] uvs1, Vector2[] uvs2, Vector2[] temp, Vector4[] colors)
        {
            Vertex[] verts = new Vertex[vertices.Length];

            // koppaa vertex infot Vertexiin
            for (int q = 0; q < vertices.Length; q++)
            {
                verts[q].vertex = vertices[q];
            }
            if (normals != null)
            {
                for (int q = 0; q < normals.Length; q++)
                {
                    verts[q].normal = normals[q];
                }
                vertexFlags |= 1;
            }
            if (colors != null)
            {
                for (int q = 0; q < colors.Length; q++)
                {
                    verts[q].color = colors[q];
                }
                vertexFlags |= 2;
            }
            if (uvs1 != null)
            {
                for (int q = 0; q < uvs1.Length; q++)
                {
                    verts[q].uv1 = uvs1[q];
                    verts[q].uv2 = uvs1[q];
                }
                vertexFlags |= 4;
            }
            if (uvs2 != null)
            {
                for (int q = 0; q < uvs2.Length; q++)
                {
                    verts[q].uv2 = uvs2[q];
                }
                vertexFlags |= 8;
            }

            AllocVBO(verts.Length, indices.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(verts.Length * VERTEX_SIZE), verts);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)0, (IntPtr)(indices.Length * sizeof(int)), indices);

            Util.CheckGLError("VBO");
        }

        public void DataToVBO(Vertex[] verts, int[] indices)
        {
            vertexFlags |= 5;
            AllocVBO(verts.Length, indices.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(verts.Length * VERTEX_SIZE), verts);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)0, (IntPtr)(indices.Length * sizeof(int)), indices);

            Util.CheckGLError("VBO");
        }

        public void Update(Vertex[] verts)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(numOfIndices * VERTEX_SIZE), verts);
        }

        /// <summary>
        /// tilat p‰‰lle. pit‰‰ kutsua ennen Renderi‰.
        /// </summary>
         public void BeginRender()
        {
            if (vertexID == 0 || indexID == 0)
            {
                throw new Exception("VBO destroyed!");
            }

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);

            if ((vertexFlags & 4) == 4)
            {
                GL.ClientActiveTexture(TextureUnit.Texture0);
                GL.EnableClientState(EnableCap.TextureCoordArray);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, VERTEX_SIZE, (IntPtr)(0));
                GL.Enable(EnableCap.Texture2D);
            }
            else GL.Disable(EnableCap.Texture2D);

            if ((vertexFlags & 8) == 8)
            {
                GL.ClientActiveTexture(TextureUnit.Texture1);
                GL.EnableClientState(EnableCap.TextureCoordArray);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, VERTEX_SIZE, (IntPtr)(2 * sizeof(float)));
            }
            if ((vertexFlags & 16) == 16)
            {
                GL.ClientActiveTexture(TextureUnit.Texture2);
                GL.EnableClientState(EnableCap.TextureCoordArray);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, VERTEX_SIZE, (IntPtr)(4 * sizeof(float)));
            }
            if ((vertexFlags & 1) == 1)
            {
                GL.EnableClientState(EnableCap.NormalArray);
                GL.NormalPointer(NormalPointerType.Float, VERTEX_SIZE, (IntPtr)(6 * sizeof(float)));
            }
            if ((vertexFlags & 2) == 2)
            {
                GL.EnableClientState(EnableCap.ColorArray);
                GL.ColorPointer(4, ColorPointerType.Float, VERTEX_SIZE, (IntPtr)(9 * sizeof(float)));
            }

            GL.EnableClientState(EnableCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, VERTEX_SIZE, (IntPtr)(13 * sizeof(float)));

        }

        /// <summary>
         /// voi erikseen valita mit‰ texture unittei k‰ytet‰‰n jos multitexture
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public void UseTextureUnits(bool t0, bool t1, bool t2)
        {
            if (t0) vertexFlags |= 4; else vertexFlags &= ~4;
            if (t1) vertexFlags |= 8; else vertexFlags &= ~8;
            if (t2) vertexFlags |= 16; else vertexFlags &= ~16;
        }

        /// <summary>
        /// renderoi vbo
        /// </summary>
        public void Render()
        {
            BeginRender();
            GL.DrawElements(BeginMode.Triangles, numOfIndices, DrawElementsType.UnsignedInt, IntPtr.Zero);
            EndRender();
        }

        /// <summary>
        /// tilat pois p‰‰lt‰
        /// </summary>
        public void EndRender()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.DisableClientState(EnableCap.NormalArray);
            GL.DisableClientState(EnableCap.VertexArray);

            GL.ClientActiveTexture(TextureUnit.Texture2);
            GL.DisableClientState(EnableCap.TextureCoordArray);

            GL.ClientActiveTexture(TextureUnit.Texture1);
            GL.DisableClientState(EnableCap.TextureCoordArray);

            GL.ClientActiveTexture(TextureUnit.Texture0);
            GL.DisableClientState(EnableCap.TextureCoordArray);

        }

    }
}
