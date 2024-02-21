using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT10481
{
    public static class PCT10481MiscParser
    {
        public static string CPDErrMsg_ParsePlant(string src)
        {
            const string Token = "_";
            var modeOfTrt = CPDErrMsg_ParseModeOfTransport(src);
            if (string.IsNullOrWhiteSpace(modeOfTrt)) return null;
            var flds = modeOfTrt.Split(Token);
            if (flds.Length > 1 ) return flds[1].Trim();
            return string.Empty;
        }
        public static string CPDErrMsg_ParseModeOfTransport(string src)
        {
            const string Token = "Mode Of Transport:";
            int pos0 = src.IndexOf(Token) + Token.Length;
            if (pos0 ==-1)
            {
                return null;
            }
            int pos1 = src.IndexOf(",", pos0);
            if (pos1 == -1) return null;
            return src.Substring(pos0, pos1 - pos0)?.Trim();
        }

    }
}
