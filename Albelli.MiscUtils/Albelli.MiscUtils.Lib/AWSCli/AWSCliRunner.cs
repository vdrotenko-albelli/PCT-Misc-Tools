using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.AWSCli
{
    public static class AWSCliRunner
    {
        public static string CatchCMDOutput(string cmdLn)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = @"C:\Program Files\Amazon\AWSCLIV2\aws.exe";
            cmd.StartInfo.Arguments = cmdLn;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = false;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            return cmd.StandardOutput.ReadToEnd();
        }
    }
}
