﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public static class CountryStorage
    {
        private static readonly HashSet<Country> Countries = new HashSet<Country>
    {
         new Country{ ID = "AD", InEu =  false },
         new Country{ ID = "AE", InEu =  false },
         new Country{ ID = "AF", InEu =  false },
         new Country{ ID = "AG", InEu =  false },
         new Country{ ID = "AI", InEu =  false },
         new Country{ ID = "AL", InEu =  false },
         new Country{ ID = "AM", InEu =  false },
         new Country{ ID = "AN", InEu =  false },
         new Country{ ID = "AO", InEu =  false },
         new Country{ ID = "AQ", InEu =  false },
         new Country{ ID = "AR", InEu =  false },
         new Country{ ID = "AS", InEu =  false },
         new Country{ ID = "AT", InEu =  true },
         new Country{ ID = "AU", InEu =  false },
         new Country{ ID = "AW", InEu =  false },
         new Country{ ID = "AZ", InEu =  false },
         new Country{ ID = "BA", InEu =  false },
         new Country{ ID = "BB", InEu =  false },
         new Country{ ID = "BD", InEu =  false },
         new Country{ ID = "BE", InEu =  true },
         new Country{ ID = "BF", InEu =  false },
         new Country{ ID = "BG", InEu =  true },
         new Country{ ID = "BH", InEu =  false },
         new Country{ ID = "BI", InEu =  false },
         new Country{ ID = "BJ", InEu =  false },
         new Country{ ID = "BM", InEu =  false },
         new Country{ ID = "BN", InEu =  false },
         new Country{ ID = "BO", InEu =  false },
         new Country{ ID = "BQ", InEu =  false },
         new Country{ ID = "BR", InEu =  false },
         new Country{ ID = "BS", InEu =  false },
         new Country{ ID = "BT", InEu =  false },
         new Country{ ID = "BV", InEu =  false },
         new Country{ ID = "BW", InEu =  false },
         new Country{ ID = "BY", InEu =  false },
         new Country{ ID = "BZ", InEu =  false },
         new Country{ ID = "CA", InEu =  false },
         new Country{ ID = "CC", InEu =  false },
         new Country{ ID = "CD", InEu =  false },
         new Country{ ID = "CF", InEu =  false },
         new Country{ ID = "CG", InEu =  false },
         new Country{ ID = "CH", InEu =  false },
         new Country{ ID = "CI", InEu =  false },
         new Country{ ID = "CK", InEu =  false },
         new Country{ ID = "CL", InEu =  false },
         new Country{ ID = "CM", InEu =  false },
         new Country{ ID = "CN", InEu =  false },
         new Country{ ID = "CO", InEu =  false },
         new Country{ ID = "CQ", InEu =  false },
         new Country{ ID = "CR", InEu =  false },
         new Country{ ID = "CU", InEu =  false },
         new Country{ ID = "CV", InEu =  false },
         new Country{ ID = "CW", InEu =  false },
         new Country{ ID = "CX", InEu =  false },
         new Country{ ID = "CY", InEu =  true },
         new Country{ ID = "CZ", InEu =  true },
         new Country{ ID = "DE", InEu =  true },
         new Country{ ID = "DJ", InEu =  false },
         new Country{ ID = "DK", InEu =  true },
         new Country{ ID = "DM", InEu =  false },
         new Country{ ID = "DO", InEu =  false },
         new Country{ ID = "DZ", InEu =  false },
         new Country{ ID = "EC", InEu =  false },
         new Country{ ID = "EE", InEu =  true },
         new Country{ ID = "EG", InEu =  false },
         new Country{ ID = "ER", InEu =  false },
         new Country{ ID = "ES", InEu =  true },
         new Country{ ID = "ET", InEu =  false },
         new Country{ ID = "FI", InEu =  true },
         new Country{ ID = "FJ", InEu =  false },
         new Country{ ID = "FK", InEu =  false },
         new Country{ ID = "FM", InEu =  false },
         new Country{ ID = "FO", InEu =  false },
         new Country{ ID = "FR", InEu =  true },
         new Country{ ID = "GA", InEu =  false },
         new Country{ ID = "GB", InEu =  false },
         new Country{ ID = "GD", InEu =  false },
         new Country{ ID = "GE", InEu =  false },
         new Country{ ID = "GF", InEu =  false },
         new Country{ ID = "GG", InEu =  false },
         new Country{ ID = "GH", InEu =  false },
         new Country{ ID = "GI", InEu =  false },
         new Country{ ID = "GL", InEu =  false },
         new Country{ ID = "GM", InEu =  false },
         new Country{ ID = "GN", InEu =  false },
         new Country{ ID = "GP", InEu =  false },
         new Country{ ID = "GQ", InEu =  false },
         new Country{ ID = "GR", InEu =  true },
         new Country{ ID = "GS", InEu =  false },
         new Country{ ID = "GT", InEu =  false },
         new Country{ ID = "GU", InEu =  false },
         new Country{ ID = "GW", InEu =  false },
         new Country{ ID = "GY", InEu =  false },
         new Country{ ID = "HK", InEu =  false },
         new Country{ ID = "HM", InEu =  false },
         new Country{ ID = "HN", InEu =  false },
         new Country{ ID = "HR", InEu =  true },
         new Country{ ID = "HT", InEu =  false },
         new Country{ ID = "HU", InEu =  true },
         new Country{ ID = "ID", InEu =  false },
         new Country{ ID = "IE", InEu =  true },
         new Country{ ID = "IL", InEu =  false },
         new Country{ ID = "IN", InEu =  false },
         new Country{ ID = "IO", InEu =  false },
         new Country{ ID = "IQ", InEu =  false },
         new Country{ ID = "IR", InEu =  false },
         new Country{ ID = "IS", InEu =  false },
         new Country{ ID = "IT", InEu =  true },
         new Country{ ID = "JE", InEu =  false },
         new Country{ ID = "JM", InEu =  false },
         new Country{ ID = "JO", InEu =  false },
         new Country{ ID = "JP", InEu =  false },
         new Country{ ID = "KE", InEu =  false },
         new Country{ ID = "KG", InEu =  false },
         new Country{ ID = "KH", InEu =  false },
         new Country{ ID = "KI", InEu =  false },
         new Country{ ID = "KM", InEu =  false },
         new Country{ ID = "KN", InEu =  false },
         new Country{ ID = "KP", InEu =  false },
         new Country{ ID = "KR", InEu =  false },
         new Country{ ID = "KW", InEu =  false },
         new Country{ ID = "KY", InEu =  false },
         new Country{ ID = "KZ", InEu =  false },
         new Country{ ID = "LA", InEu =  false },
         new Country{ ID = "LB", InEu =  false },
         new Country{ ID = "LC", InEu =  false },
         new Country{ ID = "LI", InEu =  false },
         new Country{ ID = "LK", InEu =  false },
         new Country{ ID = "LR", InEu =  false },
         new Country{ ID = "LS", InEu =  false },
         new Country{ ID = "LT", InEu =  true },
         new Country{ ID = "LU", InEu =  true },
         new Country{ ID = "LV", InEu =  true },
         new Country{ ID = "LY", InEu =  false },
         new Country{ ID = "MA", InEu =  false },
         new Country{ ID = "MC", InEu =  true },
         new Country{ ID = "MD", InEu =  false },
         new Country{ ID = "MG", InEu =  false },
         new Country{ ID = "MH", InEu =  false },
         new Country{ ID = "MI", InEu =  false },
         new Country{ ID = "MK", InEu =  false },
         new Country{ ID = "ML", InEu =  false },
         new Country{ ID = "MM", InEu =  false },
         new Country{ ID = "MN", InEu =  false },
         new Country{ ID = "MO", InEu =  false },
         new Country{ ID = "MP", InEu =  false },
         new Country{ ID = "MQ", InEu =  false },
         new Country{ ID = "MR", InEu =  false },
         new Country{ ID = "MS", InEu =  false },
         new Country{ ID = "MT", InEu =  true },
         new Country{ ID = "MU", InEu =  false },
         new Country{ ID = "MV", InEu =  false },
         new Country{ ID = "MW", InEu =  false },
         new Country{ ID = "MX", InEu =  false },
         new Country{ ID = "MY", InEu =  false },
         new Country{ ID = "MZ", InEu =  false },
         new Country{ ID = "NA", InEu =  false },
         new Country{ ID = "NC", InEu =  false },
         new Country{ ID = "NE", InEu =  false },
         new Country{ ID = "NF", InEu =  false },
         new Country{ ID = "NG", InEu =  false },
         new Country{ ID = "NI", InEu =  false },
         new Country{ ID = "NL", InEu =  true },
         new Country{ ID = "NO", InEu =  false },
         new Country{ ID = "NP", InEu =  false },
         new Country{ ID = "NR", InEu =  false },
         new Country{ ID = "NU", InEu =  false },
         new Country{ ID = "NZ", InEu =  false },
         new Country{ ID = "OM", InEu =  false },
         new Country{ ID = "PA", InEu =  false },
         new Country{ ID = "PE", InEu =  false },
         new Country{ ID = "PF", InEu =  false },
         new Country{ ID = "PG", InEu =  false },
         new Country{ ID = "PH", InEu =  false },
         new Country{ ID = "PK", InEu =  false },
         new Country{ ID = "PL", InEu =  true },
         new Country{ ID = "PM", InEu =  false },
         new Country{ ID = "PN", InEu =  false },
         new Country{ ID = "PR", InEu =  false },
         new Country{ ID = "PT", InEu =  true },
         new Country{ ID = "PW", InEu =  false },
         new Country{ ID = "PY", InEu =  false },
         new Country{ ID = "QA", InEu =  false },
         new Country{ ID = "QO", InEu =  false },
         new Country{ ID = "RE", InEu =  false },
         new Country{ ID = "RO", InEu =  true },
         new Country{ ID = "RU", InEu =  false },
         new Country{ ID = "RW", InEu =  false },
         new Country{ ID = "SA", InEu =  false },
         new Country{ ID = "SB", InEu =  false },
         new Country{ ID = "SC", InEu =  false },
         new Country{ ID = "SD", InEu =  false },
         new Country{ ID = "SE", InEu =  true },
         new Country{ ID = "SG", InEu =  false },
         new Country{ ID = "SH", InEu =  false },
         new Country{ ID = "SI", InEu =  true },
         new Country{ ID = "SJ", InEu =  false },
         new Country{ ID = "SK", InEu =  true },
         new Country{ ID = "SL", InEu =  false },
         new Country{ ID = "SM", InEu =  false },
         new Country{ ID = "SN", InEu =  false },
         new Country{ ID = "SO", InEu =  false },
         new Country{ ID = "SR", InEu =  false },
         new Country{ ID = "ST", InEu =  false },
         new Country{ ID = "SV", InEu =  false },
         new Country{ ID = "SY", InEu =  false },
         new Country{ ID = "SZ", InEu =  false },
         new Country{ ID = "TC", InEu =  false },
         new Country{ ID = "TD", InEu =  false },
         new Country{ ID = "TF", InEu =  false },
         new Country{ ID = "TG", InEu =  false },
         new Country{ ID = "TH", InEu =  false },
         new Country{ ID = "TJ", InEu =  false },
         new Country{ ID = "TK", InEu =  false },
         new Country{ ID = "TM", InEu =  false },
         new Country{ ID = "TN", InEu =  false },
         new Country{ ID = "TO", InEu =  false },
         new Country{ ID = "TP", InEu =  false },
         new Country{ ID = "TR", InEu =  false },
         new Country{ ID = "TT", InEu =  false },
         new Country{ ID = "TV", InEu =  false },
         new Country{ ID = "TW", InEu =  false },
         new Country{ ID = "TZ", InEu =  false },
         new Country{ ID = "UA", InEu =  false },
         new Country{ ID = "UG", InEu =  false },
         new Country{ ID = "UK", InEu =  true },
         new Country{ ID = "UM", InEu =  false },
         new Country{ ID = "US", InEu =  false },
         new Country{ ID = "UY", InEu =  false },
         new Country{ ID = "UZ", InEu =  false },
         new Country{ ID = "VA", InEu =  false },
         new Country{ ID = "VC", InEu =  false },
         new Country{ ID = "VE", InEu =  false },
         new Country{ ID = "VG", InEu =  false },
         new Country{ ID = "VI", InEu =  false },
         new Country{ ID = "VN", InEu =  false },
         new Country{ ID = "VU", InEu =  false },
         new Country{ ID = "WF", InEu =  false },
         new Country{ ID = "WS", InEu =  false },
         new Country{ ID = "YE", InEu =  false },
         new Country{ ID = "YT", InEu =  false },
         new Country{ ID = "YU", InEu =  false },
         new Country{ ID = "ZA", InEu =  false },
         new Country{ ID = "ZM", InEu =  false },
         new Country{ ID = "ZW", InEu =  false }
    };

        public static bool CountryIsInEu(string countryId)
        {
            var country = Countries.SingleOrDefault(c => c.ID.Equals(countryId, StringComparison.InvariantCultureIgnoreCase));
            return country?.InEu ?? false;
        }
    }
}