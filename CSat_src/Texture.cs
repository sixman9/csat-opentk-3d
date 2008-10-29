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
 * texturen lataus.
 * 
 */

//#define MOREDEBUG

using System;
using System.Collections;
using OpenTK.Graphics;

namespace CSat
{
    public class Texture
    {
        /// <summary>
        /// Vapauta kaikki resurssit kun kutsutaan Disposea
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool removeName)
        {
            if (textureID != 0)
            {

#if MOREDEBUG
                Log.WriteDebugLine(" Dispose:" + textureName + " " + textureCount);
#endif

                if (textureCount == 0) // ei muita k‰ytt‰ji‰, saa poistaa
                {
                    GL.DeleteTextures(1, ref textureID);
                    textureID = 0;

                    if (removeName)
                    {
                        if (textureName != null) textures.Remove(textureName);
                    }
                    textureName = null;

                }
                else textureCount--;
            }
        }

        public static void DisposeAll()
        {
            IDictionaryEnumerator en = textures.GetEnumerator();
            while (en.MoveNext())
            {
                Texture t = (Texture)en.Value;
                t.Dispose(false);
            }

            textures.Clear();
        }

        /// <summary>
        /// texture taulukko jossa kaikki ladatut texturet
        /// </summary>
        public static Hashtable textures = new Hashtable();

        /*
         * textureparametrit, k‰ytet‰‰n latauksen yhteydess‰
         */ 
        public static TextureMagFilter MagnificationFilter = TextureMagFilter.Linear;
        public static TextureMinFilter MinificationFilter = TextureMinFilter.Linear;
        public static TextureWrapMode WrapModeS = TextureWrapMode.Repeat;
        public static TextureWrapMode WrapModeT = TextureWrapMode.Repeat;

        string textureName = null;
        uint textureID = 0;
        public uint TextureID
        {
            get { return textureID; }
        }
        int width, height;
        public int Width
        {
            get { return width; }
        }
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// aina kun ladataan texture, t‰t‰ lis‰t‰‰n. useampi objekti voi k‰ytt‰‰ samaa
        /// texturea jolloin t‰m‰ on k‰yttˆjen lkm. kun objekteja poistetaan, katsotaan
        /// jos viel‰ joku k‰ytt‰‰ t‰t‰ texturea, ettei menn‰ poistamaan sit‰ v‰‰r‰‰n aikaan!
        /// </summary>
        int textureCount = 0;

        /// <summary>
        /// texture, cubemap
        /// </summary>
        TextureTarget target; 

        /// <summary>
        /// mik‰ texture bindattu
        /// </summary>
        static uint bind = 0;
        public void Bind()
        {
            if (textureID != bind)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
                bind = textureID;
            }
        }

        /// <summary>
        /// aseta texture haluttuun textureunittiin
        /// </summary>
        /// <param name="textureUnit"></param>
        public void Bind(int textureUnit)
        {
            bind = 0;
            if (textureID == 0)
            {
                GL.Disable(EnableCap.Texture2D);
                return;
            }

            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
        }

        /// <summary>
        /// poista texture k‰ytˆst‰ tietyst‰ textureunitista
        /// </summary>
        /// <param name="textureUnit"></param>
        public static void UnBind(int textureUnit)
        {
            bind = 0;
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.Disable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// aseta aktiivinen textureunitti
        /// </summary>
        /// <param name="textureUnit"></param>
        public static void ActiveUnit(int textureUnit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
        }

        /// <summary>
        /// lataa texture
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Texture Load(string fileName)
        {
            return Load(fileName, true);
        }

        public static Texture Load(string fileName, bool useTexDir)
        {
            // jos texture on jo ladattu, ei ladata uudelleen
            if ((Texture)textures[fileName] != null)
            {
#if MOREDEBUG
                Log.WriteDebugLine("### Found: " + fileName);
#endif
                ((Texture)textures[fileName]).textureCount++; // monta muuta k‰yttˆ‰ samalla texturella
                return (Texture)textures[fileName];
            }
            Texture t = new Texture();

            t.textureName = fileName;
            Log.WriteDebugLine("Texture: " + t.textureName);

            if (useTexDir) fileName = Settings.TextureDir + fileName;

            try
            {
                if (fileName.Contains(".dds")) // jos dds texture
                {
                    LoadFromDiskDDS(fileName, out t.textureID, out t.target, MinificationFilter, MagnificationFilter, WrapModeS, WrapModeT);
                }
                else
                {
                    LoadFromDiskGDI(fileName, out t.textureID, out t.target, MinificationFilter, MagnificationFilter, WrapModeS, WrapModeT);
                }
            }
            catch (Exception e)
            {
                t.textureID = 0;
                Log.WriteDebugLine(e.ToString());
                return null;
            }

            float[] pwidth = new float[1];
            float[] pheight = new float[1];
            GL.BindTexture(t.target, t.textureID);
            GL.GetTexLevelParameter(t.target, 0, GetTextureParameter.TextureWidth, pwidth);
            GL.GetTexLevelParameter(t.target, 0, GetTextureParameter.TextureHeight, pheight);
            t.width = (int)pwidth[0];
            t.height = (int)pheight[0];

            textures.Add(t.textureName, t);

            return t;
        }

        /*
         * kuvien lataajat.
         */ 
        public delegate void LoadFromDiskCallback(string filename, out uint texturehandle, out TextureTarget dimension, TextureMinFilter minFilter, TextureMagFilter magFilter, TextureWrapMode wrapS, TextureWrapMode wrapT);
        public static LoadFromDiskCallback LoadFromDiskGDI = _loadfromdisk;
        public static LoadFromDiskCallback LoadFromDiskDDS = _loadfromdisk;
        static void _loadfromdisk(string filename, out uint texturehandle, out TextureTarget dimension, TextureMinFilter minFilter, TextureMagFilter magFilter, TextureWrapMode wrapS, TextureWrapMode wrapT) { throw new Exception("Texture: you must setup delegates."); }
        public static void SetLoadCallback(LoadFromDiskCallback loadGDI, LoadFromDiskCallback loadDDS)
        {
            LoadFromDiskGDI = loadGDI;
            LoadFromDiskDDS = loadDDS;
        }

    }
}
