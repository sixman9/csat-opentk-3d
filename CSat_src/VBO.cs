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
    public struct Vertex
    {
        public Vector2 uv1, uv2, uv3; // 3 texturekoordinaattia max
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
            uv3 = uv1;
        }

    } // 64 tavua

    public struct VBOInfo
    {
        public int vertPos, indexPos;
        public int indexLen;

        public override string ToString()
        {
            return "(" + vertPos + " " + indexPos + " " + indexLen + ")";
        }

    }

    public struct Vbo
    {
        public VBO vbo;
        public VBOInfo vi;
    }


    public class VBO
    {
        private const int VERTEX_SIZE = 64;

        private int vertexID = 0, indexID = 0;
        private BufferUsageHint usage = BufferUsageHint.StaticDraw;
        private short vertexFlags = 0;

        private int vertPos = 0, indexPos = 0;
        private int vertLen = 0, indexLen = 0;

        private int allocatedVertSize = 0, allocatedIndSize = 0;

        public void PrintInfo()
        {
            Log.WriteDebugLine("vbo [vertices: " + vertLen + " | indices: " + indexLen + "]");
        }


        /**
         * luo fbo
         * 
         * monelleko vertexille ja indexille varataan tilaa
         */
        public void AllocVBO(int vertSize, int indSize, BufferUsageHint usage)
        {
            allocatedVertSize = vertSize;
            allocatedIndSize = indSize;

            this.usage = usage;
            GL.GenBuffers(1, out vertexID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertSize * VERTEX_SIZE), null, usage);

            GL.GenBuffers(1, out indexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indSize * sizeof(int)), null, usage);
        }

        public void Dispose()
        {
            if (indexID != 0) GL.DeleteBuffers(1, ref indexID);
            if (vertexID != 0) GL.DeleteBuffers(1, ref vertexID);
            vertexID = 0;
            indexID = 0;
        }



        /**
         * kopioi objekti vbo:hon
         * 
         * jos muistia ei ole varattu AllocVBO:lla, varataan vain sen verran mit‰ objekti vie.
         * palauttaa muistialueiden alkukohdat ja pituudet
         */
        public VBOInfo LoadVBO(Vector3[] vertices, Vector3[] normals, Vector2[] uvs1, Vector2[] uvs2, Vector2[] uvs3, Vector4[] colors, Mesh mesh)
        {
            int[] ind = new int[mesh.vertexInd.Count];
            Vector3[] vert = new Vector3[ind.Length];
            Vector3[] norm = new Vector3[ind.Length];
            Vector2[] uv = new Vector2[ind.Length];

            for (int q = 0; q < ind.Length; q++)
            {
                vert[q] = vertices[(int)mesh.vertexInd[q]];
                norm[q] = normals[(int)mesh.normalInd[q]];
                uv[q] = uvs1[(int)mesh.uvInd[q]];
            }

            // index taulukko
            for (int q = 0; q < ind.Length; q++) ind[q] = q;

            return LoadVBO(vert, ind, norm, uv, null, null, null);
        }



        /**
         * kopioi objekti vbo:hon
         * 
         * jos muistia ei ole varattu AllocVBO:lla, varataan vain sen verran mit‰ objekti vie.
         * palauttaa muistialueiden alkukohdat ja pituudet
         */
        public VBOInfo LoadVBO(Vector3[] vertices, int[] indices, Vector3[] normals, Vector2[] uvs1, Vector2[] uvs2, Vector2[] uvs3, Vector4[] colors)
        {
            VBOInfo vi;
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
                    verts[q].uv3 = uvs1[q];
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
            if (uvs3 != null)
            {
                for (int q = 0; q < uvs3.Length; q++)
                {
                    verts[q].uv3 = uvs3[q];
                }
                vertexFlags |= 16;
            }

            for (int q = 0; q < indices.Length; q++)
            {
                indices[q] += vertLen;
            }


            vi.vertPos = vertPos;
            vi.indexPos = indexLen;

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)vertPos, (IntPtr)(verts.Length * VERTEX_SIZE), verts);
            vertLen += verts.Length;
            vertPos += (VERTEX_SIZE * verts.Length);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)indexPos, (IntPtr)(indices.Length * sizeof(int)), indices);
            indexLen += indices.Length;
            indexPos += (indices.Length * sizeof(int));

            vi.indexLen = indices.Length;

            if (vertLen > allocatedVertSize || indexLen > allocatedIndSize)
            {
                throw new Exception("VBO: allocated vertex or index buffer too small! vertlen: " + vertLen + "/" + allocatedVertSize +
                    " indlen: " + indexLen + "/" + allocatedIndSize);
            }

            return vi;
        }

        public VBOInfo LoadVBO(Vertex[] verts, int[] indices)
        {
            VBOInfo vi;
            vertexFlags |= 4;

            for (int q = 0; q < indices.Length; q++)
            {
                indices[q] += vertLen;
            }

            vi.vertPos = vertPos;
            vi.indexPos = indexLen;

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)vertPos, (IntPtr)(verts.Length * VERTEX_SIZE), verts);
            vertLen += verts.Length;
            vertPos += (VERTEX_SIZE * verts.Length);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)indexPos, (IntPtr)(indices.Length * sizeof(int)), indices);
            indexLen += indices.Length;
            indexPos += (indices.Length * sizeof(int));

            vi.indexLen = indices.Length;

            if (vertLen > allocatedVertSize || indexLen > allocatedIndSize)
            {
                throw new Exception("VBO: allocated vertex or index buffer too small! vertlen: " + vertLen + "/" + allocatedVertSize +
                    " indlen: " + indexLen + "/" + allocatedIndSize);
            }

            return vi;
        }

        public void Update(Vertex[] verts, VBOInfo vi)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)vi.vertPos, (IntPtr)(vi.indexLen * VERTEX_SIZE), verts);
        }

        // tilat p‰‰lle
        public void BeginRender()
        {
            // jos vbo on jo poistettu.
            // jos lataa monta objektia samaan vbo:hon, poistaa yhden objektin niin se koko vbo h‰vi‰‰.
            // t‰m‰ ilmoittamassa jos tulee semmoinen bugi.
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

        // voi erikseen valita mit‰ texture unittei k‰ytet‰‰n jos multitexture
        public void UseTextureUnits(bool t0, bool t1, bool t2)
        {
            if (t0) vertexFlags |= 4; else vertexFlags &= ~4;
            if (t1) vertexFlags |= 8; else vertexFlags &= ~8;
            if (t2) vertexFlags |= 16; else vertexFlags &= ~16;
        }

        public void Render()
        {
            BeginRender();
            GL.DrawElements(BeginMode.Triangles, indexLen, DrawElementsType.UnsignedInt, IntPtr.Zero);
            EndRender();
        }

        public void Render(VBOInfo vi)
        {
            GL.DrawRangeElements(BeginMode.Triangles, 0, 0, vi.indexLen, DrawElementsType.UnsignedInt, (IntPtr)(vi.indexPos * 4));
        }


        // tilat pois p‰‰lt‰
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
