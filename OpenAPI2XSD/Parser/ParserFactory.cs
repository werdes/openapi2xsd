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
    public class ParserFactory
    {
        private HostBuilderContext _hostBuilderContext;
        private IConfiguration _configuration;
        private ILogger<ParserFactory> _logger;
        private OpenApi2XsdParser _openApi2XsdParser;

        public ParserFactory(
            HostBuilderContext hostBuilderContext,
            IConfiguration configuration,
            ILogger<ParserFactory> logger,
            OpenApi2XsdParser openApi2XsdParser)
        {
            _hostBuilderContext = hostBuilderContext;
            _configuration = configuration;
            _logger = logger;
            _openApi2XsdParser = openApi2XsdParser;
        }

        /// <summary>
        /// Returns a parser specified by given mode
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">on undefined parser</exception>
        public IParser GetParserByMode(Enums.Mode mode)
        {
            switch (mode)
            {
                case Enums.Mode.OpenApi2Xsd:
                    return _openApi2XsdParser;
                default:
                    throw new ArgumentException($"Undefined parser for mode [{mode}]");
            }
        }
    }
}
