using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleCodes2CategoriesParser
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var articls = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Article>>(File.ReadAllText( @"F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-8770\ArticleCodes2Categories.json"));
            Console.WriteLine(articls.Count);
            List<Tuple<string, string, int>> rows = new List<Tuple<string, string, int>>();
            foreach (var a in articls.Keys)
            {
                rows.Add(new Tuple<string, string, int>(a, articls[a].ArticleType.Code, articls[a].ArticleType.Id));
            }

            Console.WriteLine("ArticleCode\tCategory\tCategoryId");
            rows.ForEach(r => Console.WriteLine($"{r.Item1}\t{r.Item2}\t{r.Item3}"));
        }
    }
}
