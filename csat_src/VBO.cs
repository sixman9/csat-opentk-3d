#region --- MIT License ---
/* 
 * This file is part of CSat - small C# 3D-library
 * 
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 ***
 * -mjt,  
 * email: matola@sci.fi
 */
#endregion

using System;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public struct Vertex
    {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector4 uv_or_color; // 1uv, 2uv tai väri(rgba)

        public Vertex(Vector3 vertex)
        {
            this.vertex = vertex;
            this.normal = new Vector3(0, 0, 1);
            this.uv_or_color = new Vector4(1, 1, 1, 1);
        }
        public Vertex(Vector3 vertex, Vector3 normal)
        {
            this.vertex = vertex;
            this.normal = normal;
            this.uv_or_color = new Vector4(1, 1, 1, 1);
        }
        public Vertex(Vector3 vertex, Vector3 normal, Vector2 uv)
        {
            this.vertex = vertex;
            this.normal = normal;
            this.uv_or_color.X = uv.X;
            this.uv_or_color.Y = uv.Y;
            this.uv_or_color.Z = uv.X;
            this.uv_or_color.W = uv.Y;
        }
        public Vertex(Vector3 vertex, Vector3 normal, Vector2 uv, Vector2 uv2)
        {
            this.vertex = vertex;
            this.normal = normal;
            this.uv_or_color.X = uv.X;
            this.uv_or_color.Y = uv.Y;
            this.uv_or_color.Z = uv2.X;
            this.uv_or_color.W = uv2.Y;
        }
        public Vertex(Vector3 vertex, Vector3 normal, Vector4 color)
        {
            this.vertex = vertex;
            this.normal = normal;
            this.uv_or_color = color;
        }
    }

    public class VBO
    {
        enum VertexMode { OnlyVertex, Normal, UV1, UV2, Color }; // mitä tietoja vertexissä

        static private int vertexSize = 40;

        private int vertexID = 0, indexID = 0;
        private BufferUsageHint usage = BufferUsageHint.StaticDraw;

        /// <summary>
        /// mitä textureunittei käytetään. bitti0 niin ykköstä, bitti1 niin kakkosta
        /// </summary>
        static int useTexUnits = 1;

        /// <summary>
        /// liput, mitä kaikkea käytetään (normal, uv, uv2, color)
        /// </summary>
        private VertexMode vertexFlags = VertexMode.OnlyVertex;
        int numOfIndices = 0;

        public VBO()
        {
        }
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
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertSize * vertexSize), null, usage);

            GL.GenBuffers(1, out indexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indSize * sizeof(int)), null, usage);

            numOfIndices = indSize;

            Log.WriteDebugLine("AllocVBO: (verts:" + vertSize + " indices:" + indSize + ")");
        }

        public void Dispose()
        {
            if (indexID != 0) GL.DeleteBuffers(1, ref indexID);
            if (vertexID != 0) GL.DeleteBuffers(1, ref vertexID);
            vertexID = 0;
            indexID = 0;

            Log.WriteDebugLine("Disposed: VBO");
        }

        /// <summary>
        /// kopioi objekti vbo:hon.
        /// jos käytetään uvs (tai +uvs2) tai colors, normals pitää myös löytyä.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="normals"></param>
        /// <param name="uvs"></param>
        public void DataToVBO(Vector3[] vertices, int[] indices, Vector3[] normals, Vector2[] uvs)
        {
            DataToVBO(vertices, indices, normals, uvs, null, null);
        }

        /// <summary>
        /// kopioi objekti vbo:hon.
        /// jos käytetään uvs (tai +uvs2) tai colors, normals pitää myös löytyä.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="normals"></param>
        /// <param name="uvs"></param>
        public void DataToVBO(Vector3[] vertices, int[] indices, Vector3[] normals, Vector2[] uvs, Vector2[] uvs2, Vector2[] colors)
        {
            Vertex[] verts = new Vertex[vertices.Length];

            if (normals != null) vertexFlags = VertexMode.Normal;
            if (uvs != null) vertexFlags = VertexMode.UV1;
            if (uvs2 != null) vertexFlags = VertexMode.UV2;
            if (colors != null) vertexFlags = VertexMode.Color;

            // koppaa vertex infot Vertexiin
            switch (vertexFlags)
            {
                case VertexMode.OnlyVertex:
                    for (int q = 0; q < vertices.Length; q++)
                    {
                        verts[q] = new Vertex(vertices[q]);
                    }
                    break;

                case VertexMode.Normal:
                    for (int q = 0; q < vertices.Length; q++)
                    {
                        verts[q] = new Vertex(vertices[q], normals[q]);
                    }
                    break;

                case VertexMode.UV1:
                    for (int q = 0; q < vertices.Length; q++)
                    {
                        verts[q] = new Vertex(vertices[q], normals[q], uvs[q]);
                    }
                    break;

                case VertexMode.UV2:
                    for (int q = 0; q < vertices.Length; q++)
                    {
                        verts[q] = new Vertex(vertices[q], normals[q], uvs[q], uvs2[q]);
                    }
                    break;

                case VertexMode.Color:
                    for (int q = 0; q < vertices.Length; q++)
                    {
                        verts[q] = new Vertex(vertices[q], normals[q], colors[q]);
                    }
                    break;

            }

            AllocVBO(verts.Length, indices.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(verts.Length * vertexSize), verts);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)0, (IntPtr)(indices.Length * sizeof(int)), indices);

            Util.CheckGLError("VBO");
        }

        public void DataToVBO(Vertex[] verts, int[] indices)
        {
            vertexFlags = VertexMode.UV1; // normaalit + uv

            AllocVBO(verts.Length, indices.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(verts.Length * vertexSize), verts);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)0, (IntPtr)(indices.Length * sizeof(int)), indices);

            Util.CheckGLError("VBO");
        }

        public void Update(Vertex[] verts)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(numOfIndices * vertexSize), verts);
        }

        /// <summary>
        /// tilat päälle. pitää kutsua ennen Renderiä.
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

            if (vertexFlags != VertexMode.OnlyVertex)
            {
                GL.EnableClientState(EnableCap.NormalArray);
                GL.NormalPointer(NormalPointerType.Float, vertexSize, (IntPtr)(3 * sizeof(float)));

                if (vertexFlags == VertexMode.Color)
                {
                    GL.EnableClientState(EnableCap.ColorArray);
                    GL.ColorPointer(4, ColorPointerType.Float, vertexSize, (IntPtr)(10 * sizeof(float)));
                }
                else
                {
                    if (vertexFlags == VertexMode.UV1 || vertexFlags == VertexMode.UV2) // vähintään yhdet texcoordsit objektilla
                    {

                        if ((useTexUnits & 1) == 1)
                        {

                            GL.ClientActiveTexture(TextureUnit.Texture0);
                            GL.EnableClientState(EnableCap.TextureCoordArray);
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, vertexSize, (IntPtr)(6 * sizeof(float)));
                            GL.Enable(EnableCap.Texture2D);
                        }
                        if ((useTexUnits & 2) == 2)
                        {
                            GL.ClientActiveTexture(TextureUnit.Texture1);
                            GL.EnableClientState(EnableCap.TextureCoordArray);
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, vertexSize, (IntPtr)(8 * sizeof(float)));
                            GL.Enable(EnableCap.Texture2D);
                        }

                    }
                    else GL.Disable(EnableCap.Texture2D);
                }
            }
            GL.EnableClientState(EnableCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, vertexSize, (IntPtr)(0));
        }

        public static void UseTexUnits(bool tu1, bool tu2)
        {
            useTexUnits = 0;
            if (tu1) useTexUnits = 1;
            if (tu2) useTexUnits += 2;
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
        /// tilat pois päältä
        /// </summary>
        public void EndRender()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.DisableClientState(EnableCap.NormalArray);
            GL.DisableClientState(EnableCap.VertexArray);

            GL.ClientActiveTexture(TextureUnit.Texture1);
            GL.DisableClientState(EnableCap.TextureCoordArray);

            GL.ClientActiveTexture(TextureUnit.Texture0);
            GL.DisableClientState(EnableCap.TextureCoordArray);
        }

    }
}
