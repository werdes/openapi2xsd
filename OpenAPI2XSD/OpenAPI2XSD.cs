using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAPI2XSD.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI2XSD
{
    public class OpenAPI2XSD
    {
        private HostBuilderContext _hostBuilderContext;
        private IConfiguration _configuration;
        private ILogger<OpenAPI2XSD> _logger;
        private ParserFactory _parserFactory;

        public OpenAPI2XSD(HostBuilderContext hostBuilderContext, IConfiguration configuration, ILogger<OpenAPI2XSD> logger, ParserFactory parserFactory)
        {
            _hostBuilderContext = hostBuilderContext;
            _configuration = configuration;
            _logger = logger;
            _parserFactory = parserFactory;
        }

        /// <summary>
        /// Running the parser process
        /// </summary>
        public void Execute()
        {
            try
            {
                string inputFile = GetInputFile();
                string outputDirectory = GetOutputDirectory();
                Enums.Mode mode = GetMode();

                IParser parser = _parserFactory.GetParserByMode(mode);
                if (parser != null)
                {
                    parser.Parse(inputFile, outputDirectory);
                }
                else throw new ArgumentException($"Parser is unspecified");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unexpected error");
            }
        }

        /// <summary>
        /// Reads the input parameter
        /// </summary>
        /// <returns>Input file</returns>
        /// <exception cref="ArgumentException"></exception>
        private string GetInputFile()
        {
            string? configValue = _configuration["input"];
            if (configValue == null || !File.Exists(configValue))
                throw new ArgumentException("No input specified or file doesn't exist");

            return configValue;
        }

        /// <summary>
        /// Reads the mode parameter
        /// </summary>
        /// <returns>specified mode enum, OpenApi2Xsd if unspecified</returns>
        /// <exception cref="ArgumentException"></exception>
        private Enums.Mode GetMode()
        {
            Enums.Mode mode = Enums.Mode.Undefined;
            string? configValue = _configuration["mode"];
            if (configValue == null || !Enum.TryParse<Enums.Mode>(configValue, out mode))
            {
                mode = Enums.Mode.OpenApi2Xsd;
                _logger.LogInformation($"No mode specified, using default [{mode}]");
            }
            return mode;
         }

        /// <summary>
        /// Reads the output parameter
        /// </summary>
        /// <returns>specified output path, directory of the input file if unspecified</returns>
        private string GetOutputDirectory()
        {
            string? configValue = _configuration["output"];
            if (configValue == null)
            {
                configValue = Path.GetDirectoryName(GetInputFile());
                _logger.LogInformation($"No output directory specified, using input directory {configValue}");
            }

            return configValue;
        }
    }
}
