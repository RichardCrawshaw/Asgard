using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Generated.Test
{
    class OpCodesTest
    {
        public static void Main()
        {
            // Working Directory:
            // C:\Users\richa\source\repos\Asgard\Asgard\bin\Debug\net5.0
            // Required file path:
            // C:\Users\richa\source\repos\Asgard\Asgard\Generated\cbus-opcodes.txt
            var loader = new Loader(@"..\..\..\Generated\cbus-opcodes.txt");
            loader.Load();

            Console.WriteLine($"EnumerationLines: {loader.EnumerationLines.Count}");
            Console.WriteLine($"EnumerationNames: {loader.EnumerationNames.Count}");
            Console.WriteLine($"FileCommentLines: {loader.FileCommentLines.Count}");
            Console.WriteLine($"HistoryLines:     {loader.HistoryLines.Count}");
            Console.WriteLine($"LicenceLines:     {loader.LicenceLines.Count}");
            Console.WriteLine($"OpCodeLines:      {loader.OpCodeLines.Count}");
            Console.WriteLine($"OpCodeNumbers:    {loader.OpCodeNumbers.Count}");
            Console.WriteLine($"PropertyLines:    {loader.PropertyLines.Count}");

            var builder = new Builder(loader);
            builder.Build();

            foreach(var opCodeBlock in builder.OpCodeBlocks)
            {
                Console.WriteLine("{0:X2} V\t{1}\t{2}\t{3}\t\"{4}\"", opCodeBlock.Value, 
                    opCodeBlock.Code, opCodeBlock.Priority, opCodeBlock.Group, opCodeBlock.Name);

                Console.WriteLine("{0:X2} D\t{1}", opCodeBlock.Value, opCodeBlock.Description);
                
                foreach (var comment in opCodeBlock.Comments)
                    Console.WriteLine("{0:X2} C\t{1}", opCodeBlock.Value, comment);
                
                foreach (var property in opCodeBlock.Properties)
                    Console.WriteLine("{0:X2} P\t{1}\t{2}\t{3}", opCodeBlock.Value, 
                        property.Source, property.Name, property.DataType);

                Console.WriteLine("{0:X2} S\t{1}", opCodeBlock.Value, opCodeBlock.ToStringText);
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
