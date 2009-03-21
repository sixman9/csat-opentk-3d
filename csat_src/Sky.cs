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

using OpenTK.Graphics;

namespace CSat
{
    public class Sky : Node
    {
        ObjModel sky = new ObjModel("skybox");

        public Sky(string name)
        {
            Name = name;
        }

        public override void Dispose()
        {
            sky.Dispose();
        }

        /// <summary>
        /// lataa skybox.
        /// </summary>
        /// <param name="skyName">skyboxin nimi, eli esim plainsky_  jos tiedostot on plainsky_front.jpg, plainsky_back.jpg jne</param>
        /// <param name="ext">tiedoston p‰‰te, eli jpg, png, dds, ..</param>
        /// <param name="scale"></param>
        public void LoadSkybox(string skyName, string ext, float scale)
        {
            Texture.LoadTextures = false; // ‰l‰ lataa objektin textureita automaattisesti
            sky = new ObjModel("skybox", "skybox.obj", scale, scale, scale); // lataa skybox
            Texture.LoadTextures = true; // seuraava saa ladatakin..
            sky.Boundings = null;

            string[] side = { "bottom", "left", "back", "right", "top", "front" };

            for (int q = 0; q < 6; q++)
            {
                sky.Meshes()[q].Boundings = null;

                string fileName = skyName + side[q] + "." + ext;
                Texture newSkyTex = new Texture();

                TextureLoaders.TextureLoaderParameters.WrapModeS = TextureWrapMode.ClampToEdge;
                TextureLoaders.TextureLoaderParameters.WrapModeT = TextureWrapMode.ClampToEdge;

                newSkyTex = Texture.Load(fileName);

                // etsi sivun materiaali
                Material matInf = sky.GetMaterial(sky.Meshes()[q].MaterialName);

                if (matInf != null)
                {
                    // korvaa vanhat texturet
                    matInf.DiffuseTex = newSkyTex;
                }
            }
            TextureLoaders.TextureLoaderParameters.WrapModeS = TextureWrapMode.Repeat;
            TextureLoaders.TextureLoaderParameters.WrapModeT = TextureWrapMode.Repeat;
        }

        /// <summary>
        /// lataa kupu.
        /// </summary>
        /// <param name="skyName">texturen nimi</param>
        /// <param name="scale"></param>
        public void LoadSkydome(string skyName, float scale)
        {
            Texture.LoadTextures = false; // ‰l‰ lataa objektin textureita automaattisesti
            sky = new ObjModel("skydome", "skydome.obj", scale, scale, scale); // lataa kupu
            Texture.LoadTextures = true; // seuraava saa ladatakin..
            sky.Boundings = null;
            sky.Meshes()[0].Boundings = null;

            Texture newSkyTex = new Texture();
            newSkyTex = Texture.Load(skyName);

            // etsi materiaali
            Material matInf = sky.GetMaterial(sky.Meshes()[0].MaterialName);

            if (matInf != null)
            {
                // korvaa vanha texture
                matInf.DiffuseTex = newSkyTex;
            }
        }

        protected override void RenderObject()
        {
            RenderMesh();
        }

        public override void Render()
        {
            base.Render(); // renderoi objektin ja kaikki siihen liitetyt objektit
        }

        public void RenderMesh()
        {
            GL.PushMatrix();

            GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
            Util.ModelMatrix[12] = Util.ModelMatrix[13] = Util.ModelMatrix[14] = 0;

            GL.Disable(EnableCap.Lighting);
            GL.DepthMask(false); // ei kirjoiteta z-bufferiin

            GL.LoadMatrix(Util.ModelMatrix);
            for (int q = 0; q < sky.Objects.Length; q++) sky.Meshes()[q].RenderMesh();
            Settings.NumOfObjects -= sky.Objects.Length - 1;

            GL.DepthMask(true);
            GL.Enable(EnableCap.Lighting);

            GL.PopMatrix();
        }
    }
}
