using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public static class PackageParser
    {
        public static Package Parse(string input)
        {
            const string DimsToken = "LxWxH: ";
            string trimmed = input.Trim();
            Package package = new Package();
            int pos0 = trimmed.IndexOf(' ');
            package.Type = trimmed.Substring(0, pos0).Trim();
            int pos1 = trimmed.IndexOf(DimsToken);
            if (pos1 == -1) return package;
            int[] dims = ParseDims(trimmed.Substring(pos1 + DimsToken.Length));
            if (dims.Length > 0 ) package.LengthInMm = dims[0];
            if (dims.Length > 1) package.WidthInMm= dims[1];
            if (dims.Length > 2) package.HeightInMm = dims[2];
            if (dims.Length > 3) package.WeightInGrams = dims[3];
            return package;
        }

        private static int[] ParseDims(string input)
        {
            List<int> rslt = new();
            string[] flds0 = input.Split('/');
            var fldsD = new List<string>(flds0[0].Replace("mm", "").Split(" x "));
            fldsD.ForEach(f => rslt.Add(int.Parse(f.Trim())));
            if (flds0.Length > 1 ) 
            {
                rslt.Add(int.Parse(flds0[1].Trim().Replace("g", "")));
            }
            return rslt.ToArray();
        }
    }
}
