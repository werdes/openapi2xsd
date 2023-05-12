# openapi2xsd

CLI tool for converting OpenAPI (formerly Swagger) component schemas into XML Schema files

## Available options
### File imports
| Parameter | Value |  | Example |
|--|--|--|--|
| `--input:{n}` | Input OpenAPI-Files (.yaml, .json) | supports multiple input files (increment {n}) | `--input:1 swagger.yaml --input:2 schemas.yaml`   |
| `--output` | Output directory | **Default**: directory of the first input file | `--output "D:\files\"` |

### Other options
| Parameter | Value |  | Example |
|--|--|--|--|
| `--mode` | Converter mode | **Default**: OpenApi2XsdNoReferences, Available options: `OpenApi2XsdNoReferences` | `--mode OpenApi2XsdNoReferences`   |
| `--includeDescriptions` | Include Annotation-Elements in the .xsd-Result | **Default** : false | `--includeDescriptions true` |
| `--limitDescriptionLength` | Limit the length of the annotations | **Default** : Int32 maximum | `--limitDescriptionLength 250` |

### Modes
- `OpenApi2XsdNoReferences`: OpenAPI (2.0 or 3.0) component schemas to XML Schema Definitions (XSD won't contain cross-file references, but will bring all required types)
