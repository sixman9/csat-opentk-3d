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

/* 
 * texturen lataus.
 * 
 */
using System;
using System.Collections.Generic;
using OpenTK.Graphics;

namespace CSat
{
    public class Texture
    {
        /// <summary>
        /// ladataanko texturet
        /// </summary>
        public static bool LoadTextures = true;

        /// <summary>
        /// texture taulukko jossa kaikki ladatut texturet
        /// </summary>
        public static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

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
        /// aina kun ladataan texture, tätä lisätään. useampi objekti voi käyttää samaa
        /// texturea jolloin tämä on käyttöjen lkm. kun objekteja poistetaan, katsotaan
        /// jos vielä joku käyttää tätä texturea, ettei mennä poistamaan sitä väärään aikaan!
        /// </summary>
        int textureCount = 0;

        /// <summary>
        /// texture, cubemap
        /// </summary>
        TextureTarget target;

        /// <summary>
        /// mikä texture bindattu mihinkin unittiin (tarkistusta varten ettei tarvitse useaan kertaan bindata)
        /// </summary>
        static uint[] bind = new uint[256];

        /// <summary>
        /// bindaa texture ekaan textureunittiin
        /// </summary>
        public void Bind()
        {
            if (textureID != bind[0])
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
                bind[0] = textureID;
            }
        }

        /// <summary>
        /// aseta texture haluttuun textureunittiin
        /// </summary>
        /// <param name="textureUnit"></param>
        public void Bind(int textureUnit)
        {
            bind[textureUnit] = textureID;
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
        }

        /// <summary>
        /// poista texture käytöstä tietystä textureunitista
        /// </summary>
        /// <param name="textureUnit"></param>
        public void UnBind(int textureUnit)
        {
            bind[textureUnit] = 0;
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.Disable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Vapauta kaikki resurssit kun kutsutaan Disposea
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool removeName)
        {
            if (textureID != 0)
            {
                if (textureCount == 0) // ei muita käyttäjiä, saa poistaa
                {
                    GL.DeleteTextures(1, ref textureID);
                    textureID = 0;

                    if (removeName)
                    {
                        if (textureName != null) textures.Remove(textureName);
                    }

                    Log.WriteDebugLine("Disposed: " + textureName + " " + textureCount);

                    textureName = null;
                }
                else textureCount--;
            }
        }

        public static void DisposeAll()
        {
            foreach (KeyValuePair<string, Texture> dta in textures)
            {
                dta.Value.Dispose(false);
            }
            textures.Clear();
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
            Texture temptex;
            // jos texture on jo ladattu, ei ladata uudelleen
            textures.TryGetValue(fileName, out temptex);
            if (temptex != null)
            {
                temptex.textureCount++; // monta muuta käyttöä samalla texturella
                return temptex;
            }

            Texture t = new Texture();

            t.textureName = fileName;
            Log.WriteDebugLine("Texture: " + t.textureName);

            if (useTexDir) fileName = Settings.TextureDir + fileName;

            try
            {
                if (fileName.Contains(".dds")) // jos dds texture
                {
                    TextureLoaders.ImageDDS.LoadFromDisk(fileName, out t.textureID, out t.target);
                }
                else
                {
                    TextureLoaders.ImageGDI.LoadFromDisk(fileName, out t.textureID, out t.target);
                }
            }
            catch (Exception e)
            {
                Log.WriteDebugLine(e.ToString());
                throw new Exception(e.ToString());
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

    }
}
