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
using System.Collections.Generic;

namespace CSat
{
    public class Node
    {
        /// <summary>
        /// objektin nimi
        /// </summary>
        public string Name;

        /// <summary>
        /// objektin paikka
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// objektin asento
        /// </summary>
        public Vector3 Rotation;
        /// <summary>
        /// jos tarvii korjata asento ennen liikuttelua ja kääntämistä
        /// </summary>
        public Vector3 FixRotation;

        public Vector3 Front = new Vector3(0, 0, 1), Right = new Vector3(1, 0, 0), Up = new Vector3(0, 1, 0);

        /// <summary>
        /// objektin keskikohta. tarvitaan frustum cullauksessa.
        /// </summary>
        public Vector3 ObjCenter = new Vector3(0, 0, 0);

        /// <summary>
        /// objektin paikka ja asento kamerasta katsottuna
        /// </summary>
        public float[] Matrix = null;

        /// <summary>
        /// objektin paikka ja asento world coordinaateissa (esim frustum cullaus vaatii tämän)
        /// </summary>
        public float[] WMatrix = null;

        protected List<Node> objects = new List<Node>();
        public Node[] Objects
        {
            get
            {
                return objects.ToArray();
            }
        }

        /// <summary>
        /// calculatepositions ottaa kaikki täysin näkyvät objektit tähän talteen renderointia varten
        /// </summary>
        static protected List<Node> visibleObjects = new List<Node>();
        /// <summary>
        /// calculatepositions ottaa kaikki ruudulla olevat läpikuultavat objektit tähän talteen renderointia varten
        /// </summary>
        static protected List<Node> translucentObjects = new List<Node>();

        /// <summary>
        /// kaikki objektit menee tähän,joten saadaan helposti poistettua kaikki datat
        /// </summary>
        static protected List<Node> allObjects = new List<Node>();

        public Node()
        {
            Name = "node";
            allObjects.Add(this);
        }
        public Node(string name)
        {
            Name = name;
            allObjects.Add(this);
        }

        static public void DisposeAll()
        {
            for (int q = 0; q < allObjects.Count; q++)
            {
                allObjects[q].Dispose();

            }
        }
        public virtual void Dispose()
        {
        }

        public void Add(Node obj)
        {
            objects.Add(obj);

            if (obj is Light)
            {
                Light.Lights.Add((Light)obj);
            }
            else if (obj is Camera)
            {
                Camera.cam = (Camera)obj;
            }

            Log.WriteDebugLine(obj.Name + " added to " + Name + ".");
        }

        public void Remove(Node obj)
        {
            objects.Remove(obj);
            if (obj is Light)
            {
                Light.Remove((Light)obj);
            }
            else if (obj is Camera)
            {
                Camera.cam = null;
            }

            Log.WriteDebugLine(obj.Name + " removed from " + Name + ".");
        }
        public void Remove(string name)
        {
            Remove(Search(name));
        }

        public Node Search(string name)
        {
            foreach (Node i in objects)
            {
                if (i.Name == name) return i;
                if (i.Objects.Length > 0) return i.Search(name);
            }
            return null;
        }

        /// <summary>
        /// käännä y-akselin ympäri
        /// </summary>
        /// <param name="f"></param>
        public void TurnXZ(float f)
        {
            Rotation.Y -= f;
        }

        /// <summary>
        /// käännä x-akselin ympäri
        /// </summary>
        /// <param name="f"></param>
        public void LookUpXZ(float f)
        {
            Rotation.X -= f;
        }

        /// <summary>
        /// käännä z-akselin ympäri
        /// </summary>
        /// <param name="f"></param>
        public void RollXZ(float f)
        {
            Rotation.Z -= f;
        }

        public void MoveXZ(float forward, float strafe)
        {
            if (forward != 0)
            {
                MoveXZ(forward);
            }
            if (strafe != 0)
            {
                StrafeXZ(strafe);
            }
        }

        /// <summary>
        /// liikuta xz tasossa
        /// </summary>
        /// <param name="f">paljonko liikutaan eteen/taaksepäin</param>
        public void MoveXZ(float f)
        {
            Position.X -= ((float)Math.Sin(Rotation.Y * MathExt.PiOver180) * f);
            Position.Z -= ((float)Math.Cos(Rotation.Y * MathExt.PiOver180) * f);
        }

        /// <summary>
        /// liikuta xz-tasossa sivuttain
        /// </summary>
        /// <param name="f">paljonko liikutaan sivuttain</param>
        public void StrafeXZ(float f)
        {
            Position.X += ((float)Math.Cos(-Rotation.Y * MathExt.PiOver180) * f);
            Position.Z += ((float)Math.Sin(-Rotation.Y * MathExt.PiOver180) * f);
        }

        /*
         * Metodit täysin vapaaseen liikkumiseen (6DOF)
         * 
         */
        /// <summary>
        /// eteenpäin/taaksepäin f/-f  ja jos xzPlane on true, liikutaan vain xz tasolla
        /// </summary>
        /// <param name="f"></param>
        /// <param name="xzPlane"></param>
        public void MoveForward(float f, bool xzPlane)
        {
            if (xzPlane == false)
            {
                Position += (Front * f);
            }
            else
            {
                Position.X += Front.X * f;
                Position.Z += Front.Z * f;
            }
        }
        /// <summary>
        /// liikuta sivusuunnassa. jos xzPlane on true, liikutaan vain xz tasolla
        /// </summary>
        /// <param name="f"></param>
        /// <param name="xzPlane"></param>
        public void StrafeRight(float f, bool xzPlane)
        {
            if (xzPlane == false)
            {
                Position += (Right * f);
            }
            else
            {
                Position.X += Right.X * f;
                Position.Z += Right.Z * f;
            }
        }

        /// <summary>
        /// noustaan ylös joko up vektorin suuntaisesti (jos pyöritty, se voi osoittaa mihin vain),
        /// tai kohtisuoraan xz tasosta jos xzPlane on true.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="xzPlane"></param>
        public void MoveUp(float f, bool xzPlane)
        {
            if (xzPlane == false)
            {
                Position += (Up * f);
            }
            else
            {
                Position.Y += Up.Y * f;
            }
        }


        public void RotateX(float f)
        {
            Rotation.X -= f;
            Front *= (float)Math.Cos(f * MathExt.PiOver180);
            Up *= (float)Math.Sin(f * MathExt.PiOver180);

            Front += Up;
            Front.Normalize();

            Up = Vector3.Cross(Front, Right);
            Up = -Up;
        }

        public void RotateY(float f)
        {
            Rotation.Y -= f;
            Front *= (float)Math.Cos(f * MathExt.PiOver180);
            Right *= (float)Math.Sin(f * MathExt.PiOver180);

            Front -= Right;
            Front.Normalize();

            Right = Vector3.Cross(Front, Up);
        }

        public void RotateZ(float f)
        {
            Rotation.Z -= f;
            Right *= (float)Math.Cos(f * MathExt.PiOver180);
            Up *= (float)Math.Sin(f * MathExt.PiOver180);

            Right += Up;
            Right.Normalize();

            Up = Vector3.Cross(Front, Right);
            Up = -Up;
        }

        public void LookAt(Vector3 pos)
        {
            GL.LoadIdentity();
            Glu.LookAt(Position, pos, Up);
        }

        void Translate(Node obj)
        {
            // liikuta haluttuun kohtaan
            GL.Translate(obj.Position + obj.ObjCenter);
            GL.Rotate(obj.Rotation.X, 1, 0, 0);
            GL.Rotate(obj.Rotation.Y, 0, 1, 0);
            GL.Rotate(obj.Rotation.Z, 0, 0, 1);
            GL.Rotate(obj.FixRotation.X, 1, 0, 0);
            GL.Rotate(obj.FixRotation.Y, 0, 1, 0);
            GL.Rotate(obj.FixRotation.Z, 0, 0, 1);
        }

        protected void Translate()
        {
            // liikuta haluttuun kohtaan
            GL.Translate(Position);

            // aseta oikea asento
            GL.Rotate(Rotation.X, 1, 0, 0);
            GL.Rotate(Rotation.Y, 0, 1, 0);
            GL.Rotate(Rotation.Z, 0, 0, 1);

            // ja korjaa asentoa jos tarvii
            GL.Rotate(FixRotation.X, 1, 0, 0);
            GL.Rotate(FixRotation.Y, 0, 1, 0);
            GL.Rotate(FixRotation.Z, 0, 0, 1);
        }

        /// <summary>
        /// luo visible ja translucent listat näkyvistä objekteista.
        /// </summary>
        public void MakeLists()
        {
            foreach (Node o in Objects)
            {
                Mesh m = o as Mesh;

                // jos objekti on Mesh
                if (m != null)
                {
                    // tarkista onko objekti näkökentässä
                    if (Frustum.ObjectInFrustum(m.WMatrix[12], m.WMatrix[13], m.WMatrix[14], m.Boundings))
                    {
                        if (m.IsTranslucent == false) visibleObjects.Add(m);
                        else translucentObjects.Add(m);
                    }
                }
                else
                {
                    visibleObjects.Add(o);
                }
                if (o.Objects.Length > 0) o.MakeLists();
            }
        }

        public void MakeList()
        {
            // jos objekti on Mesh
            if (this is Mesh)
            {
                Mesh m = (Mesh)this;
                // tarkista onko objekti näkökentässä
                if (Frustum.ObjectInFrustum(WMatrix[12], WMatrix[13], WMatrix[14], m.Boundings))
                {
                    if (m.IsTranslucent == false) visibleObjects.Add(this);
                    else translucentObjects.Add(this);
                }
            }
            else
            {
                visibleObjects.Add(this);
            }
        }

        /// <summary>
        /// laskee joka objektin paikan ja ottaa sen talteen joko Matrix tai WMatrix taulukkoon
        /// </summary>
        /// <param name="getWMatrix"></param>
        public void CalcPositions(bool getWMatrix)
        {
            GL.PushMatrix();
            CalcPosition(getWMatrix);

            foreach (Node o in Objects)
            {
                GL.PushMatrix();

                if (getWMatrix)
                {
                    o.Translate(o);
                    GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
                    Util.CopyArray(ref Util.ModelMatrix, ref o.WMatrix);
                }
                else
                {
                    o.Translate();
                    GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
                    Util.CopyArray(ref Util.ModelMatrix, ref o.Matrix);
                }

                if (o.Objects.Length > 0)
                {
                    o.CalcPositions(getWMatrix);
                }

                GL.PopMatrix();
            }
            GL.PopMatrix();
        }

        public void CalcPosition(bool wMatrix)
        {
            Mesh m = this as Mesh;
            if (m != null)
            {
                if (m.LookAtNextPoint)
                {
                    return;
                }
            }

            if (wMatrix)
            {
                Translate(this);
                GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
                Util.CopyArray(ref Util.ModelMatrix, ref WMatrix);
            }
            else
            {
                Translate();
                GL.GetFloat(GetPName.ModelviewMatrix, Util.ModelMatrix);
                Util.CopyArray(ref Util.ModelMatrix, ref Matrix);
            }
        }

        protected void CalculatePositions()
        {
            CalcPositions(false);

            GL.PushMatrix();
            GL.LoadIdentity();
            CalcPositions(true);
            GL.PopMatrix();

            MakeList();
            MakeLists();

            // TODO:
            // järjestä translucent listassa olevat objektit etäisyyden mukaan, kauimmaiset ekaks
        }

        protected virtual void RenderObject()
        {
        }

        public virtual void Render()
        {
            GL.PushMatrix();

            // lasketaan kaikkien objektien paikat valmiiksi. 
            // näkyvät objektit asetetaan visible ja translucent listoihin
            CalculatePositions();

            // renderointi
            foreach (Node o in visibleObjects)
            {
                o.RenderObject();
            }
            foreach (Node o in translucentObjects)
            {
                o.RenderObject();
            }

            visibleObjects.Clear();
            translucentObjects.Clear();

            GL.PopMatrix();
        }

        protected void Render(Node obj)
        {
            obj.Render();
        }

    }
}
