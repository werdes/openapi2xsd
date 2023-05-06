using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI2XSD.Parser
{
    public class OpenApiBaseParser
    {
        private HostBuilderContext _hostBuilderContext;
        private IConfiguration _configuration;
        private ILogger<OpenApiBaseParser> _logger;

        public OpenApiBaseParser(HostBuilderContext hostBuilderContext, IConfiguration configuration, ILogger<OpenApiBaseParser> logger)
        {
            _hostBuilderContext = hostBuilderContext;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Loads multiple input files as a workspace
        /// </summary>
        /// <param name="inputFiles"></param>
        /// <returns></returns>
        protected OpenApiWorkspace GetOpenApiWorkspace(string[] inputFiles)
        {
            OpenApiWorkspace openApiWorkspace = new OpenApiWorkspace();

            foreach(string? inputFile in inputFiles)
            {
                OpenApiDocument openApiDocument = GetOpenApiDocument(inputFile);
                string location = Path.GetFileName(inputFile);
                openApiWorkspace.AddDocument(location, openApiDocument);
            }

            return openApiWorkspace;
        }

        /// <summary>
        /// Loads the supplied OpenAPI document
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        private OpenApiDocument GetOpenApiDocument(string inputFile)
        {
            OpenApiDocument openApiDocument = null;
            string location = Path.GetFileName(inputFile);

            using (FileStream fileStream = File.Open(inputFile, FileMode.Open))
            {
                openApiDocument = new OpenApiStreamReader().Read(fileStream, out OpenApiDiagnostic openApiDiagnostic);
                _logger.LogDebug($"Parsed [{inputFile}]: [{openApiDocument}]");
            }


            // Some fixes for common mistakes in references
            if (openApiDocument.Components != null && openApiDocument.Components.Schemas != null)
            {
                foreach (string schemaKey in openApiDocument.Components.Schemas.Keys)
                {
                    OpenApiSchema? schema = openApiDocument.Components.Schemas[schemaKey];
                    if (schema.Reference != null && schema.Reference.ExternalResource != null)
                    {
                        // /#/ is not resolvable, since the leading slash will be part of the location
                        schema.Reference.ExternalResource = schema.Reference.ExternalResource.Replace("/#/", "#/");
                    }
                }
            }
            else throw new ArgumentException($"[{inputFile}] does not contain any component schemas");

            return openApiDocument;
        }
    }
}
