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

// TODO: kirjaimet display listiin / vbohon?

/* 
 * fonttitiedoston pitää olla .PNG
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
 Löytyy myös (atm): https://archon2160.pbwiki.com/f/IrrFontTool.exe
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK.Graphics;

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
        static string chars = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_'abcdefghijklmnopqrstuvwxyz{|}                                                                      Ä                 Ö             ä                 ö";
        Texture fontTex = new Texture();
        Rect[] uv = new Rect[chars.Length];
        float height = 0;

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
                Texture.MinificationFilter = TextureMinFilter.Linear;
                Texture.MagnificationFilter = TextureMagFilter.Linear;

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
                this.height = (float)height / (float)data.Height;


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
            float hg = uv[0].h * size;
        }



        float curX = 0, curY = 0, size = 100;

        public void SetSize(float size)
        {
            this.size = size * 100;
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

            float xp = curX, yp = curY;

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
                    xp = curX;
                    yp -= height * size;
                    curY -= height * size;
                    continue;
                }

                float u = uv[ch].x;
                float v = uv[ch].y;
                float w = uv[ch].w;
                float h = uv[ch].h;
                float wm = w * size;
                float hm = h * size;

                GL.Begin(BeginMode.Quads);

                GL.TexCoord2(u, v);
                GL.Vertex2(xp, yp);

                GL.TexCoord2(u + w, v);
                GL.Vertex2(xp + wm, yp);

                GL.TexCoord2(u + w, v + h);
                GL.Vertex2(xp + wm, yp + h + hm);

                GL.TexCoord2(u, v + h);
                GL.Vertex2(xp, yp + h + hm);

                GL.End();

                xp += wm;
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
            size *= 5;
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

            GL.Translate(x, (float)Util.ScreeenHeight - (2*y), 0); // miks 2*y? 

            float xp = curX, yp = curY;

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
                    xp = curX;
                    yp -= height * size;
                    curY -= height * size;
                    continue;
                }

                float u = uv[ch].x;
                float v = uv[ch].y;
                float w = uv[ch].w;
                float h = uv[ch].h;
                float wm = w * size;
                float hm = h * size;

                GL.Begin(BeginMode.Quads);

                GL.TexCoord2(u, v);
                GL.Vertex2(xp, yp);

                GL.TexCoord2(u + w, v);
                GL.Vertex2(xp + wm, yp);

                GL.TexCoord2(u + w, v + h);
                GL.Vertex2(xp + wm, yp + h + hm);

                GL.TexCoord2(u, v + h);
                GL.Vertex2(xp, yp + h + hm);

                GL.End();

                xp += wm;
            }

            GL.PopMatrix();
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.AlphaTest);

            size /= 5;
        }


    }
}
