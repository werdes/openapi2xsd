using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;
using OpenAPI2XSD.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OpenAPI2XSD.Parser
{
    public class OpenApi2XsdParser : OpenApiBaseParser, IParser
    {
        private HostBuilderContext _hostBuilderContext;
        private IConfiguration _configuration;
        private ILogger<OpenApi2XsdParser> _logger;

        private readonly XNamespace NS = XNamespace.Get("http://www.w3.org/2001/XMLSchema");

        public OpenApi2XsdParser(HostBuilderContext hostBuilderContext, IConfiguration configuration, ILogger<OpenApi2XsdParser> logger)
            : base(hostBuilderContext, configuration, logger)
        {
            _hostBuilderContext = hostBuilderContext;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Parses the OpenAPI file
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputDirectory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool Parse(string[] inputFiles, string outputDirectory)
        {
            _logger.LogDebug($"Parsing [{inputFiles.Join(",")}] to [{outputDirectory}]");
            OpenApiWorkspace openApiWorkspace = GetOpenApiWorkspace(inputFiles);
            int i = 0;

            foreach (OpenApiDocument openApiDocument in openApiWorkspace.Documents)
            {
                _logger.LogInformation($"[{openApiDocument.Components?.Schemas?.Count}] component schemas found in [{inputFiles[i++]}]");

                if (openApiDocument != null && openApiDocument.Components != null && openApiDocument.Components.Schemas != null)
                {
                    foreach (string schemaKey in openApiDocument.Components.Schemas.Keys.Where(x => x == "ReturnRequestList"))
                    {
                        OpenApiSchema schema = openApiDocument.Components.Schemas[schemaKey];
                        _logger.LogDebug($"Reading schema [{schemaKey}]");

                        XDocument document = new XDocument();
                        Dictionary<string, XContainer> typeDefinitions = new Dictionary<string, XContainer>();
                        List<string> stack = new List<string>();
                        XElement rootElement = new XElement(NS + "schema",
                            new XAttribute(XNamespace.Xmlns + "xs", NS),
                            GetElementFromSchema(schemaKey, schema, null, openApiWorkspace, typeDefinitions, string.Empty, stack)
                        );
                        rootElement.Add(typeDefinitions.Values);
                        document.Add(rootElement);

                        SaveDocument(document, schema, outputDirectory);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Saves the Schema as .xsd-File
        /// </summary>
        /// <param name="document"></param>
        /// <param name="schema"></param>
        /// <param name="outputDirectory"></param>
        private void SaveDocument(XDocument document, OpenApiSchema schema, string outputDirectory)
        {
            string outputPath = Path.Combine(outputDirectory, schema.Reference.Id + ".xsd");
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

            using (XmlWriter writer = XmlWriter.Create(outputPath, settings))
            {
                document.WriteTo(writer);
            }
        }

        /// <summary>
        /// Maps the OpenAPI Type and format to an XSD Datatype
        /// </summary>
        /// <param name="type"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private string GetAtomicType(string type, string format)
        {
            switch (type)
            {
                case "string":
                    switch (format)
                    {
                        case "date": return "xs:dateTime";
                        default: return "xs:string";
                    }
                case "number":
                    switch (format)
                    {
                        case "int64": return "xs:long";
                        case "int32": return "xs:integer";
                        default: return "xs:integer";
                    };
                case "integer": return "xs:integer";
                case "boolean": return "xs:boolean";
            }
            return "xs:string";
        }

        /// <summary>
        /// Returns an XML Element based on the given OpenAPI schema
        /// </summary>
        /// <param name="key"></param>
        /// <param name="schema"></param>
        /// <param name="openApiWorkspace"></param>
        /// <returns></returns>
        private XContainer? GetElementFromSchema(string key, OpenApiSchema schema, OpenApiSchema parentSchema, OpenApiWorkspace openApiWorkspace, Dictionary<string, XContainer> typeDefinitions, string path, List<string> stack)
        {
            XElement element = new XElement(NS + "element", new XAttribute("name", key));
            bool minOccursSet = false;
            bool includeDescriptions = _configuration.GetValue<bool>(Constants.Options.IncludeDescriptions, false);
            path = path + "/" + key;

            List<string> newStack = new List<string>(stack.Count + 1);
            newStack.AddRange(stack);
            newStack.Add(key);


            // _logger.LogDebug($"Resolving [{path}]");

            // Set minOccurs for required properties
            if (parentSchema != null && parentSchema.Required.Contains(key))
            {
                element.Add(new XAttribute("minOccurs", 1));
                minOccursSet = true;
            }

            // Set description if available
            if (includeDescriptions && !string.IsNullOrEmpty(schema.Description))
            {
                string schemaDescription = schema.Description;
                int descriptionLengthLimit = _configuration.GetValue<int>(Constants.Options.LimitDescriptionLength, int.MaxValue);
                schemaDescription = schemaDescription.Substring(0, Math.Min(schemaDescription.Length, descriptionLengthLimit));

                element.Add(new XElement(NS + "annotation",
                    new XElement(NS + "documentation",
                        schemaDescription)));
            }

            List<XContainer> children = new List<XContainer>();

            // Object Properties -> recursively load child properties
            if (schema.Type == "object")
            {

                foreach (string propertyKey in schema.Properties.Keys)
                {
                    OpenApiSchema propertySchema = schema.Properties[propertyKey];

                    if (propertySchema.Reference != null)
                    {
                        string typeKey = propertySchema.Reference.Id.ReduceToSimple();

                        if (typeDefinitions.ContainsKey(typeKey))
                        {
                            children.Add(new XElement(NS + "element",
                                new XAttribute("name", propertyKey),
                                new XAttribute("type", typeKey)));
                        }
                        else
                        {
                            typeDefinitions.Add(typeKey, null);
                            XContainer? container = GetElementFromSchema(propertyKey, schema.Properties[propertyKey], schema, openApiWorkspace, typeDefinitions, path, newStack);
                            XContainer?[] childDescendants = container.Elements().SelectMany(x => x.Elements()).ToArray();

                            XContainer typeContainer = new XElement(NS + "complexType",
                                new XAttribute("name", typeKey),
                                childDescendants);

                            typeDefinitions[typeKey] = typeContainer;
                            children.Add(new XElement(NS + "element",
                                new XAttribute("name", propertyKey),
                                new XAttribute("type", typeKey)));
                        }
                    }
                    else
                    {
                        XContainer? container = GetElementFromSchema(propertyKey, schema.Properties[propertyKey], schema, openApiWorkspace, typeDefinitions, path, newStack);
                        if (container != null)
                        {
                            children.Add(container);
                        }
                    }
                }

                element.Add(
                    new XElement(NS + "complexType",
                        new XElement(NS + "sequence",
                            children)
                    )
                );
                //}

            }
            else if (schema.Type == "array")
            {
                if (!minOccursSet)
                {
                    element.Add(new XAttribute("minOccurs", "0"));
                }

                element.Add(
                    new XAttribute("maxOccurs", "unbounded")
                );

                if (schema.Items != null)
                {
                    if (schema.Items.Reference != null)
                    {
                        if (typeDefinitions.ContainsKey(schema.Items.Reference?.Id.ReduceToSimple()))
                        {
                            children.Add(new XElement(NS + "element",
                                new XAttribute("name", key),
                                new XAttribute("type", schema.Items.Reference.Id.ReduceToSimple())));
                        }
                        else
                        {
                            List<XObject> referenceObjects = ResolveReference(schema.Items, openApiWorkspace, key, typeDefinitions, path, newStack);
                            element.Add(referenceObjects);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(schema.Items.Type))
                        {
                            element.Add(new XAttribute("type", GetAtomicType(schema.Items.Type, schema.Items.Format)));
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(schema.Type))
            {
                element.Add(
                    new XAttribute("type", GetAtomicType(schema.Type, schema.Format))
                );
            }
            else if (schema.Reference != null)
            {
                List<XObject> resolvedDescendants = ResolveReference(schema, openApiWorkspace, key, typeDefinitions, path, newStack);
                foreach (XObject descendantObjects in resolvedDescendants)
                {
                    element.Add(descendantObjects);
                }
            }

            return element;
        }

        /// <summary>
        /// Resolves a reference
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="openApiWorkspace"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private List<XObject> ResolveReference(OpenApiSchema schema, OpenApiWorkspace openApiWorkspace, string key, Dictionary<string, XContainer> typeDefinitions, string path, List<string> stack)
        {
            List<XObject> containers = new List<XObject>();
            IOpenApiReferenceable? resolvable = openApiWorkspace.ResolveReference(schema.Reference);
            if (resolvable != null && resolvable is OpenApiSchema)
            {
                OpenApiSchema resolvedSchema = (OpenApiSchema)resolvable;
                string schemaKey = schema.Reference.Id.ReduceToSimple();
                if (!typeDefinitions.ContainsKey(schemaKey))
                {
                    typeDefinitions.Add(schemaKey, null);
                    XContainer? childElement = GetElementFromSchema(key, resolvedSchema, schema, openApiWorkspace, typeDefinitions, path, stack);
                    XContainer?[] childDescendants = childElement.Elements().SelectMany(x => x.Elements()).ToArray();

                    XContainer typeContainer = new XElement(NS + "complexType",
                        new XAttribute("name", schemaKey),
                        childDescendants);
                    typeDefinitions[schemaKey] = typeContainer;
                }

                containers.Add(new XAttribute("type", schemaKey));

                //foreach (XElement descendant in childDescendants)
                //{
                //    containers.Add(descendant);
                //}


            }
            else throw new ArgumentException($"Unsresolved reference at [{schema.Reference.ReferenceV3}]");
            return containers;
        }
    }
}
