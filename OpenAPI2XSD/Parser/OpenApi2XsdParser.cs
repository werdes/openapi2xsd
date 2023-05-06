using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI2XSD.Parser
{
    public class OpenApi2XsdParser : IParser
    {
        private HostBuilderContext _hostBuilderContext;
        private IConfiguration _configuration;
        private ILogger<OpenApi2XsdParser> _logger;

        public OpenApi2XsdParser(HostBuilderContext hostBuilderContext, IConfiguration configuration, ILogger<OpenApi2XsdParser> logger)
        {
            _hostBuilderContext = hostBuilderContext;
            _configuration = configuration;
            _logger = logger;
        }

        public bool Parse(string inputFile, string outputDirectory)
        {
            throw new NotImplementedException();
        }
    }
}
