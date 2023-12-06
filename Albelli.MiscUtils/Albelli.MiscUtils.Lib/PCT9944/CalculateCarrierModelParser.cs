namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class CalculateCarrierModelParser
    {
        private int _lastParsedSegmentPos = 0;
        private const string EmptyArrayString = "empty";
        public CalculateCarrierModel Parse(string input)
        {
            //"YPB/bonusprint => MT/'VCT1102', Box LxWxH: 440 x 330 x 160mm / 1743g, ArticleTypes: empty, DeliveryTypes: Standard, Express",
            CalculateCarrierModel rslt = new CalculateCarrierModel();
            rslt.PlantCode = ParsePlantCode(input);
            if (string.IsNullOrWhiteSpace(rslt.PlantCode)) return rslt;
            rslt.Brand = ParseBrand(input);
            if (string.IsNullOrWhiteSpace(rslt.Brand)) return rslt;
            rslt.CountryId = ParseCountryId(input);
            if (string.IsNullOrWhiteSpace(rslt.CountryId)) return rslt;
            rslt.ZipCode = ParseZipCode(input);
            if (string.IsNullOrWhiteSpace(rslt.ZipCode)) return rslt;
            rslt.Package = ParsePackage(input);
            //if (rslt.Package == null) return rslt;
            rslt.ArticleTypes = ParseArticleTypes(input);
            rslt.DeliveryTypes = ParseDeliveryTypes(input);
            return rslt;
        }

        private IEnumerable<string> ParseDeliveryTypes(string input)
        {
            string valueStr = input.Substring(_lastParsedSegmentPos).Trim();
            if (string.IsNullOrWhiteSpace(valueStr) || EmptyArrayString ==  valueStr) return new string[] { };
            return valueStr.Split(", ");
        }

        private IEnumerable<string> ParseArticleTypes(string input)
        {
            string currEnd = $", {nameof(CalculateCarrierModel.DeliveryTypes)}: ";
            string currStart = $", {nameof(CalculateCarrierModel.ArticleTypes)}: ";

            if (string.IsNullOrWhiteSpace(input))
                return new string[] { };
            int pos0 = input.IndexOf(currStart);
            if (pos0 == -1) return new string[] { };
            int pos1 = input.IndexOf(currEnd, pos0+1);
            if (pos1 == -1) return new string[] { };
            _lastParsedSegmentPos = pos1 + currEnd.Length;
            string valueStr = input.Substring(pos0 + currStart.Length, pos1 - pos0 - currStart.Length).Trim();
            if (string.IsNullOrWhiteSpace(valueStr) || EmptyArrayString ==  valueStr) return new string[] { };
            return valueStr.Split(", ");
        }

        private Package ParsePackage(string input)
        {
            const string currDelim = "g,";
            if (string.IsNullOrWhiteSpace(input))
                return null;
            int pos0 = input.IndexOf(currDelim, _lastParsedSegmentPos + 1);
            if (pos0 == -1) return null;
            int prevParsedSegmentEndPos = _lastParsedSegmentPos;
            _lastParsedSegmentPos = pos0 + currDelim.Length;
            try
            {
                return PackageParser.Parse(input.Substring(prevParsedSegmentEndPos, pos0 - prevParsedSegmentEndPos).Trim());
            }
            catch
            {
                return null; //todo
            }
        }

        private string ParseZipCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            int pos0 = input.IndexOf(',', _lastParsedSegmentPos + 1);
            if (pos0 == -1) return string.Empty;
            int prevParsedSegmentEndPos = _lastParsedSegmentPos;
            _lastParsedSegmentPos = pos0 + 1;
            var valueStr = input.Substring(prevParsedSegmentEndPos, pos0 - prevParsedSegmentEndPos).Trim();
            if (valueStr[0] == '\'') valueStr = valueStr.Substring(1);
            if (valueStr[valueStr.Length-1] == '\'') valueStr = valueStr.Substring(0, valueStr.Length-1);
            return valueStr;
        }

        private string ParseCountryId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            int pos0 = input.IndexOf('/', _lastParsedSegmentPos+1);
            if (pos0 == -1) return string.Empty;
            int prevParsedSegmentEndPos = _lastParsedSegmentPos;
            _lastParsedSegmentPos = pos0 + 1;
            return input.Substring(prevParsedSegmentEndPos, pos0 - prevParsedSegmentEndPos).Trim();
        }

        private string ParseBrand(string input)
        {
            const string currDelim = " => ";
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            int pos0 = input.IndexOf(currDelim, _lastParsedSegmentPos + 1);
            if (pos0 == -1) return string.Empty;
            int prevParsedSegmentEndPos = _lastParsedSegmentPos;
            _lastParsedSegmentPos = pos0 + currDelim.Length;
            return input.Substring(prevParsedSegmentEndPos, pos0 - prevParsedSegmentEndPos).Trim();
        }


        private string ParsePlantCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            int pos0 = input.IndexOf('/');
            if (pos0 == -1) return string.Empty;
            _lastParsedSegmentPos = pos0 + 1;
            return input.Substring(0, pos0);
        }
    }
}
