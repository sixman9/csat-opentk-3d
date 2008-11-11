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

using OpenTK.Graphics;

namespace CSat
{
    public class Skydome : ObjectInfo
    {
        Object3D skydome = new Object3D();

        public void Dispose()
        {
            skydome.Dispose();
        }

        /// <summary>
        /// lataa kupu.
        /// </summary>
        /// <param name="skyName">texturen nimi</param>
        /// <param name="scale"></param>
        public void Load(string skyName, float scale)
        {
            Object3D.Textured = false; // älä lataa objektin textureita automaattisesti
            skydome = new Object3D("skydome.obj", scale, scale, scale); // lataa kupu
            Object3D.Textured = true; // seuraava saa ladatakin..

            skydome.SetBoundingMode(BoundingVolume.None);

            Texture newSkyTex = new Texture();
            newSkyTex = Texture.Load(skyName);

            // etsi materiaali
            string mat = skydome.GetObject(0).MaterialName;
            Material matInf = Material.GetMaterial(mat);

            if (matInf != null)
            {
                // korvaa vanha texture
                matInf.diffuseTex = newSkyTex;
            }

        }

        /// <summary>
        /// rendaa taivas
        /// </summary>
        public new void Render()
        {
            GL.PushMatrix();

            GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
            Util.ModelMatrix[12] = Util.ModelMatrix[13] = Util.ModelMatrix[14] = 0;
            GL.LoadMatrix(Util.ModelMatrix);

            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);

            skydome.GetObject(0).RenderFast();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);

            GL.PopMatrix();
        }

    }
}
