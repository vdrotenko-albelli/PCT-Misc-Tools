using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Albelli.MiscUtils.Lib.PCT9944.Constants;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class SenderCodeHelper
    {
        public static string GetSapiSenderCode(string plantCode, bool isVistaPrintSender)
        {
            return plantCode switch
            {
                "YPB" when isVistaPrintSender => "qj7du2w9",    //confirmed SAPI use
                "YPB" when !isVistaPrintSender => "c4yfavtf",   //not used anymore
                "ELA" => "c4yfavtf",    //should not be used?
                "NLH" => "f8fdytte",    //not used anymore
                _ => default
            };
        }

        public static string GetCentiroSenderCode(string plantCode, string brand)
        {
            return plantCode.ToUpper() switch
            {
                PlantCode.YPB => GetYpbSenderCode(brand),
                PlantCode.NLH => CentiroConstants.SenderCodes.NLH,
                PlantCode.RRD => CentiroConstants.SenderCodes.RRD,
                PlantCode.ELA => CentiroConstants.SenderCodes.ELA,
                PlantCode.KHS => CentiroConstants.SenderCodes.KHS,
                PlantCode.ORWO => CentiroConstants.SenderCodes.ORWO,
                PlantCode.PAT => CentiroConstants.SenderCodes.PAT,
                PlantCode.RAV => CentiroConstants.SenderCodes.RAV,
                PlantCode.EKT => CentiroConstants.SenderCodes.EKT,
                PlantCode.PP => CentiroConstants.SenderCodes.PP,
                PlantCode.WFR => CentiroConstants.SenderCodes.WFR,
                "TST" or "BHI" or "KIS" => string.Empty,
                _ => throw new ArgumentException(message: $"Unknown plantCode '{plantCode}'", paramName: nameof(plantCode))
            };
        }

        public static string GetYpbSenderCode(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
            {
                throw new ArgumentException(message: $"Invalid brand provided '{brand}'", paramName: nameof(brand));
            }

            if (Brand.IsVistaprintBrand(brand))
            {
                return CentiroConstants.SenderCodes.YPBVP;
            }
            else
            {
                return CentiroConstants.SenderCodes.YPB;
            }
        }

        public static string GetPlantCode(string senderCode)
        {
            return senderCode switch
            {
                CentiroConstants.SenderCodes.YPB or CentiroConstants.SenderCodes.YPBVP => PlantCode.YPB,
                CentiroConstants.SenderCodes.NLH => PlantCode.NLH,
                CentiroConstants.SenderCodes.RRD => PlantCode.RRD,
                CentiroConstants.SenderCodes.ELA => PlantCode.ELA,
                CentiroConstants.SenderCodes.KHS => PlantCode.KHS,
                CentiroConstants.SenderCodes.ORWO => PlantCode.ORWO,
                CentiroConstants.SenderCodes.PAT => PlantCode.PAT,
                CentiroConstants.SenderCodes.RAV => PlantCode.RAV,
                CentiroConstants.SenderCodes.EKT => PlantCode.EKT,
                CentiroConstants.SenderCodes.PP => PlantCode.PP,
                CentiroConstants.SenderCodes.WFR => PlantCode.WFR,
                _ => throw new ArgumentException(message: $"Unknown sender code '{senderCode}'", paramName: nameof(senderCode)),
            };
        }
    }
}
