#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2009 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See mit-license.txt for licensing details.
 */
#endregion

using System;
using System.Windows.Forms;
using CSat;
using OpenTK;

namespace CSatExamples
{
    class MainClass
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Log.Open("log.txt");

            Log.WriteDebugLine("CSat " + Settings.Version + " examples (c) 2008-2009 mjt[matola@sci.fi]");

            // muutetaan data hakemistot
            // näin siksi että debug ja release versio löytää data-hakemiston bin/ hakemistosta.
            Settings.DataDir = "../data/model/";
            Settings.TextureDir = "../data/texture/";
            Settings.ShaderDir = "../data/shader/";

            // jos true, käynnistä vain haluttu demo
            //if (true)
            if (false)
            {
                GameWindow game = new Game5(800, 600); // mikä demo halutaan
                //game.WindowBorder = WindowBorder.Fixed;
                game.WindowState = OpenTK.WindowState.Normal; // .FullScreen;
                Menu.PrintInfo();
                game.Run(30.0, 0.0);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Menu());


        }
    }
}
