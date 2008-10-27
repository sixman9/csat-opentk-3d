#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008 mjt[matola@sci.fi]
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
        public static bool UseFonts=true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Log.Open("log.txt");

            Log.WriteDebugLine("CSat " + Settings.Version + " examples (c) 2008 mjt[matola@sci.fi]");

            // muutetaan data hakemistot
            // näin siksi että debug ja release versio löytää data-hakemiston bin/ hakemistosta.
            Settings.DataDir = "../data/model/";
            Settings.TextureDir = "../data/texture/";
            Settings.ShaderDir = "../data/shader/";

            // TÄRKEÄÄ: täytyy asettaa mitä metodeja kutsutaan kun ladataan kuva, csatin Texture-luokka käyttää sitten näitä.
            // nämä löytyy csloaders paketista
            Texture.SetLoadCallback(TextureLoaders.ImageGDI.LoadFromDisk, TextureLoaders.ImageDDS.LoadFromDisk);

            // jos true, käynnistä vain haluttu demo
            if (false)
            //if (true)
            {
                GameWindow game = new Game8(800, 600); // mikä demo halutaan
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
