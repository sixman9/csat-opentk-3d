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
 * käytetään kun liitetään MD5 model worldiin.
 * 
 * MD5Model model = new MD5Model();
 * AnimatedModel uglyModel;
 * 
 * model.Load("Ugly/Ukko.mesh");
 * uglyModel = new AnimatedModel((IModel)model);
 * world.Add(uglyModel);
 * 
 * MD5Modelissa ei ole paikka ja asentotietoja, se pitää
 * liittää AnimatedModeliin joka hoitaa sen jälkeen
 * paikan laskemiset ym.
 * 
 */
using System;
using System.Collections;
using OpenTK.Graphics;
using OpenTK.Math;

namespace CSat
{
    public interface IModel
    {
        BoundingVolume GetBoundingVolume();
        void Load(string fileName);
        void Render();
    }

    public class AnimatedModel : ObjectInfo, IModel, ICloneable
    {
        IModel model;

        public AnimatedModel() { }

        public AnimatedModel(IModel model)
        {
            this.model = model;
        }
        public void SetAnimatedModel(IModel model)
        {
            this.model = model;
        }

        public BoundingVolume GetBoundingVolume()
        {
            return model.GetBoundingVolume();
        }

        /// <summary>
        /// renderoi objekti, lasketaan myös paikka
        /// </summary>        
        public new void Render()
        {
            visibleObjects.Clear();
            translucentObjects.Clear();

            GL.PushMatrix(); // kameramatrix talteen. seuraavat laskut frustum cullingia varten
            GL.LoadIdentity();
            CalculateWorldCoords(this);
            GL.PopMatrix(); // kameraan takas

            CalculateCoords(this);

            RenderArrays();
        }

        /// <summary>
        /// pelkkä renderointi.
        /// </summary>
        public void RenderFast()
        {
            if (Shader != null && UseShader) Shader.UseProgram();
            if (DoubleSided) GL.Disable(EnableCap.CullFace);

            model.Render();

            if (Shader != null && UseShader) Shader.RemoveProgram();
            if (DoubleSided) GL.Enable(EnableCap.CullFace);
            Settings.NumOfObjects++;
        }

        public void Load(string fileName)
        {
            model.Load(fileName);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public AnimatedModel Clone()
        {
            AnimatedModel clone = (AnimatedModel)this.MemberwiseClone();

            // eri grouppi eli kloonatut objektit voi lisäillä grouppiin mitä tahtoo
            // sen vaikuttamatta alkuperäiseen.
            clone.objects = new ArrayList(objects);

            for (int q = 0; q < objects.Count; q++)
            {
                object ob = objects[q];
                Object3D child = (Object3D)objects[q];
                clone.objects[q] = child.Clone();
            }
            return clone;
        }
    }
}
