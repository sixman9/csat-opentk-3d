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
 * fonttitiedoston pitää olla .PNG (ja tausta kannattaa olla läpinäkyvänä).
 * 
 * Load("times14.png");
 * Write("Hello!");
 * 
 * apuna:
 * http://www.codersource.net/csharp_image_Processing.aspx
 * 
 * 
 * 
 Fonttikuvat on luotu IrrFontToolilla  
 (tulee <a href="http://irrlicht.sourceforge.net/">Irrlicht</a> 3d-enginen mukana)
 jolloin kirjainten ei tarvitse olla saman levyisiä.
 Löytyy myös (ehkä): https://archon2160.pbwiki.com/f/IrrFontTool.exe
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK.Graphics.OpenGL;

namespace CSat
{
    class Rect
    {
        public float x, y, w, h;

        public Rect(float x, float y, float w, float h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = -h;
        }

    }

    public class BitmapFont
    {
        static string chars = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXyz[\\]^_'abcdefghijklmnopqrstuvwXyz{|}                                                                      Ä                 Ö             ä                 ö";
        Texture fontTex = new Texture();
        Rect[] uv = new Rect[chars.Length];
        int[] chDL = new int[chars.Length];
        float charHeight = 0;

        float curX = 0, curY = 0;
        public float Size = 500;

        public BitmapFont() { }
        public BitmapFont(string fileName)
        {
            Load(fileName);
        }

        public void Dispose()
        {
            for (int q = 0; q < chars.Length; q++) uv[q] = null;
            fontTex.Dispose();
        }

        public void Load(string fileName)
        {
            try
            {
                // lataa fonttitexture
                fontTex = Texture.Load(fileName);

                // lataa bitmap ja etsi kirjainten uv koordinaatit

                Bitmap CurrentBitmap = null;

                CurrentBitmap = new Bitmap(Settings.TextureDir + fileName);
                if (CurrentBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb || CurrentBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    BitmapData Data = CurrentBitmap.LockBits(new Rectangle(0, 0, CurrentBitmap.Width, CurrentBitmap.Height), ImageLockMode.ReadOnly, CurrentBitmap.PixelFormat);

                    SearchUV(ref Data, (CurrentBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb) ? 3 : 4);

                    CurrentBitmap.UnlockBits(Data);
                }
                else throw new Exception("Font: wrong pixel format.");
            }
            catch (Exception e)
            {
                throw new ArgumentException("Font: error loading file " + fileName + ".\n" + e);
            }
            CreateDL();
        }


        long GetColor(byte r, byte g, byte b)
        {
            return (long)((r << 16) + (g << 8) + b);
        }

        void SearchUV(ref BitmapData data, int bpp)
        {
            int width = 0, height = 0, x = 0, y = 0;
            int ch = 0;

            unsafe
            {
                // ota ylätarkistusväri
                byte* ptr = (byte*)(data.Scan0);
                long color1 = GetColor(*ptr, *(ptr + 1), *(ptr + 2));
                ptr += bpp;
                // ota alatarkistusväri
                long color2 = GetColor(*ptr, *(ptr + 1), *(ptr + 2));

                // etsi korkeus
                ptr = (byte*)(data.Scan0);
                for (int i = 0; i < data.Height; i++)
                {
                    ptr += data.Width * bpp;
                    height++;
                    long curcol = GetColor(*ptr, *(ptr + 1), *(ptr + 2));
                    if (curcol == color1) break;
                }
                this.charHeight = (float)height / (float)data.Height;


                // etsi kirjainten koot

                ptr = (byte*)(data.Scan0);
                while (true) // kunnes joka rivi käyty läpi
                {
                    while (true) // joka kirjain rivillä
                    {
                        long curcol = GetColor(*ptr, *(ptr + 1), *(ptr + 2));
                        if (curcol == color1) // ylänurkka
                        {
                            long b = 0;

                            // haetaan alanurkka
                            ptr += data.Width * bpp * (height - 1);
                            b += data.Width * bpp * (height - 1);
                            width = 0;

                            while (true)
                            {
                                curcol = GetColor(*ptr, *(ptr + 1), *(ptr + 2));
                                if (curcol == color2)
                                {
                                    // kirjaimen tiedot talteen
                                    uv[ch] = new Rect((float)x / (float)data.Width,
                                        (float)(y - 2.5f) / (float)data.Height,
                                        (float)width / (float)data.Width,
                                        (float)(height - 4f) / (float)data.Height);

                                    break;
                                }
                                ptr += bpp;
                                b += bpp;
                                width++;


                            }
                            ptr -= b;
                            ch++;
                            if (ch >= chars.Length) break;
                        }
                        x++;
                        if (x >= data.Width) break;
                        ptr += bpp;

                    }

                    x = 0;
                    y -= height;
                    if (y >= data.Height) break;
                    ptr = (byte*)(data.Scan0);
                    ptr += data.Width * bpp * (-y);

                    if (ch >= chars.Length)
                    {
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// luo display listit
        /// </summary>
        void CreateDL()
        {
            GL.PushMatrix();
            for (int ch = 0; ch < chars.Length; ch++)
            {
                chDL[ch] = GL.GenLists(1);
                GL.NewList(chDL[ch], ListMode.Compile);

                float u = uv[ch].x;
                float v = uv[ch].y;
                float w = uv[ch].w;
                float h = uv[ch].h;
                float wm = w * Size;
                float hm = h * Size;
                float xp = 0, yp = 0;

                GL.Begin(BeginMode.Quads);
                GL.TexCoord2(u, v);
                GL.Vertex2(xp, yp);

                GL.TexCoord2(u + w, v);
                GL.Vertex2(xp + wm, yp);

                GL.TexCoord2(u + w, v + h);
                GL.Vertex2(xp + wm, yp + hm);

                GL.TexCoord2(u, v + h);
                GL.Vertex2(xp, yp + hm);

                GL.End();

                GL.EndList();
            }
            GL.PopMatrix();
        }

        public void Write3D(string str)
        {
            Write3D(curX, curY, str);
        }

        public void Write3D(float x, float y, string str)
        {
            curX = x;
            curY = y;

            fontTex.Bind();
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.1f);

            GL.PushMatrix();
            GL.Translate(x, y, 0);
            GL.Scale(0.2f, .2f, .2f);

            float xp = 0;

            for (int q = 0; q < str.Length; q++)
            {
                int ch;

                // etsi kirjain
                for (ch = 0; ch < chars.Length; ch++)
                {
                    if (str[q] == chars[ch]) break;
                }
                if (str[q] == '\n')
                {
                    curY -= charHeight * Size;
                    GL.Translate(-xp, -charHeight * Size, 0);
                    xp = 0;
                    continue;
                }
                float u = uv[ch].x;
                float w = uv[ch].w;
                float wm = w * Size;
                xp += wm;
                GL.CallList(chDL[ch]);
                GL.Translate(wm, 0, 0);
            }

            GL.PopMatrix();
            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.AlphaTest);

        }


        public void Write(string str)
        {
            Write(curX, curY, str);
        }
        public void Write(float x, float y, string str)
        {
            curX = x;
            curY = y;

            fontTex.Bind();
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.1f);
            GL.PushMatrix();

            GL.Translate(x, (float)Util.ScreeenHeight - y, 0);

            float xp = 0;

            for (int q = 0; q < str.Length; q++)
            {
                int ch;

                // etsi kirjain
                for (ch = 0; ch < chars.Length; ch++)
                {
                    if (str[q] == chars[ch])
                    {
                        break;
                    }
                }
                if (str[q] == '\n')
                {
                    curY -= charHeight * Size;
                    GL.Translate(-xp, -charHeight * Size, 0);
                    xp = 0;
                    continue;
                }

                float w = uv[ch].w;
                float wm = w * Size;
                xp += wm;
                GL.CallList(chDL[ch]);
                GL.Translate(wm, 0, 0);
            }

            GL.PopMatrix();
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.AlphaTest);
        }

    }
}
