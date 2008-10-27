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
using System.Collections;

namespace CSat
{
    public interface IModel
    {
        void Load(string fileName, VBO vbo);
        void Render();

    }

    public class AnimatedModel : Group, IModel, ICloneable
    {
        IModel model;
        public AnimatedModel(IModel model)
        {
            this.model = model;
        }
        public void SetAnimatedModel(IModel model)
        {
            this.model = model;
        }

        public new void Render()
        {
            model.Render();
        }

        public void Load(string fileName, VBO vbo)
        {
            model.Load(fileName, vbo);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public AnimatedModel Clone()
        {
            AnimatedModel o = (AnimatedModel)this.MemberwiseClone();
            o.objects = new ArrayList(objects);
            return o;
        }


    }
}
