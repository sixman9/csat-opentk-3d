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
            logWriter.WriteLine("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "]: " + str);
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
