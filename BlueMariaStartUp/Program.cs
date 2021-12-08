using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlueMariaStartUp
{
    class Program
    {
        static void Main(string[] args)
        {
            
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", @"cd"+ System.IO.Directory.GetCurrentDirectory() );
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardInput = true;
            procStartInfo.UseShellExecute = false;
            //procStartInfo.Arguments = @"BlueMaria.exe --Startup";
            Process proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            
            proc.StandardInput.WriteLine( @"BlueMaria.exe --Startup" );
            //proc.StandardInput.Flush();
           // proc.StandardOutput.ReadToEnd();
            //proc.WaitForExit();
            //Console.WriteLine(proc.StandardOutput.ReadToEnd());
            Thread.Sleep(10000);
            Environment.Exit(1);
        }
    }
}
