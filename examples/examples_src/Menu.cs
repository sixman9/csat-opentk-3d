#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Windows.Forms;
using CSat;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatExamples
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void Form1_Load(Object sender, EventArgs e)
        {
            listBox1.Items.Add("2D test");
            listBox1.Items.Add("3D test, obj loader");
            listBox1.Items.Add("Particles");
            listBox1.Items.Add("Animation test");
            listBox1.Items.Add("Collision detection");
            listBox1.Items.Add("Billboard");
            listBox1.Items.Add("BitmapFont");
            listBox1.Items.Add("Scene test");
            listBox1.Items.Add("GLSL test");
            listBox1.Items.Add("Path test");
            listBox1.Items.Add("City");
            listBox1.SelectedIndex = 0;

            textBox1.Lines = new string[] { "Moving:", "A D W S", "R F (up/down)", "shift - run", "", "Push left mouse button to rotate the camera." };

            DisplayDevice dev = DisplayDevice.Default;
            for (int q = 0; q < dev.AvailableResolutions.Count; q++)
            {
                if (dev.AvailableResolutions[q].BitsPerPixel >= 16)
                    comboBox1.Items.Add(dev.AvailableResolutions[q].Width+"x"+
                	                    dev.AvailableResolutions[q].Height+"x"+
                	                    dev.AvailableResolutions[q].BitsPerPixel);
            }
            int ind = comboBox1.FindString("800x600");
            comboBox1.SelectedIndex = ind;
        }

        private void button1_Click(Object sender, EventArgs e)
        {
            // starttaa esimerkki
            GameWindow game = null;
            DisplayDevice dev = DisplayDevice.Default;
            int ind = comboBox1.SelectedIndex;

            string[] strs = ((string)(comboBox1.Items[ind])).Split('x');
            int width = int.Parse(strs[0]);
            int height = int.Parse(strs[1]);
            int bpp = int.Parse(strs[2]);

            switch (listBox1.SelectedIndex + 1)
            {
                case 1:
                    game = new Game1(width, height);
                    break;
                case 2:
                    game = new Game2(width, height);
                    break;
                case 3:
                    game = new Game3(width, height);
                    break;
                case 4:
                    game = new Game4(width, height);
                    break;
                case 5:
                    game = new Game5(width, height);
                    break;
                case 6:
                    game = new Game6(width, height);
                    break;
                case 7:
                    game = new Game7(width, height);
                    break;
                case 8:
                    game = new Game8(width, height);
                    break;
                case 9:
                    game = new Game9(width, height);
                    break;
                case 10:
                    game = new Game10(width, height);
                    break;
                case 11:
                    game = new Game11(width, height);
                    break;
            }

            // fullscreen?
            if (checkBox1.Checked)
            {
                dev.ChangeResolution(dev.SelectResolution(width, height, bpp, 60f));

                //game.WindowBorder = WindowBorder.Fixed;
                //game.WindowState = OpenTK.WindowState.Maximized;
                game.WindowState = OpenTK.WindowState.Fullscreen;
            }
            else
            {
                //game.WindowBorder = WindowBorder.Fixed;
                game.WindowState = OpenTK.WindowState.Normal;
            }

            PrintInfo();
            game.Run(30.0, 0.0);

            if (checkBox1.Checked)
                dev.RestoreResolution();

            Log.WriteDebugLine("Test finished..");

            game = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static bool _showInfo = true;
        public static void PrintInfo()
        {
            if (_showInfo == false) return;
            _showInfo = false;
            Log.WriteDebugLine("Vendor: " + GL.GetString(StringName.Vendor));
            Log.WriteDebugLine("Renderer: " + GL.GetString(StringName.Renderer));
            Log.WriteDebugLine("Version: " + GL.GetString(StringName.Version));
            Log.WriteDebugLine("OS: " + System.Environment.OSVersion.ToString());
        }

    }
}
