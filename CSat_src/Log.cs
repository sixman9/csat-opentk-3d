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

namespace CSat
{
    public static class Log
    {
        private static System.IO.StreamWriter logWriter = null;

        public static void Open(string filename)
        {
            if (logWriter == null) logWriter = new System.IO.StreamWriter(filename);
        }
        public static void Close()
        {
            if (logWriter == null) return;
            logWriter.Close();
            logWriter = null;
        }
        public static void WriteToFile(string str)
        {
            if (logWriter == null) return;
            logWriter.WriteLine("[" + DateTime.Now + "]: " + str);
            logWriter.Flush();
        }

        public static void WriteLineAndWait(string str)
        {
            WriteDebugLine(str);
            Console.ReadKey(true);
        }

        public static void WriteDebugLine(string str)
        {
            // kirjoitetaanko konsoliin virheet
            if (Settings.WriteDebug)
            {
                System.Diagnostics.Trace.WriteLine(str);
                Console.WriteLine(str);
            }

            // tiedostoon kirjoitetaan kaikki jos logi tiedosto on avattu
            WriteToFile(str);
        }

    }

}
