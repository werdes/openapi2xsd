using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI2XSD.Parser
{
    public interface IParser
    {
        bool Parse(string inputFile, string outputDirectory);
    }
}
