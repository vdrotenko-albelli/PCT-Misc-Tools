//using LogLurker;

using RevStackCore.LogParser;
using System.IO;

namespace TestRunner.Logs.Parser.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //[0]
            //var file = new LogFileLurker(args[0]);
            //var t = file.Lurk
            //string filePath = args[0];

            //[1]
            //using (TextReader reader = File.OpenText(filePath))
            //{
            //    var w3cLogModel = new W3CReader(reader).Read();
            //    Console.WriteLine(w3cLogModel.Count());
            //}

            Analogy.LogViewer.Log4Net.Log4NetFactory factory = new Analogy.LogViewer.Log4Net.Log4NetFactory();
            
        }
    }
}