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
| `--mode` | Converter mode | **Default**: OpenApi2Xsd, Available options: `OpenApi2Xsd` | `--mode OpenApi2Xsd`   |
| `--includeDescriptions` | Include Annotation-Elements in the .xsd-Result | **Default** : false | `--includeDescriptions true` |
| `--limitDescriptionLength` | Limit the length of the annotations | **Default** : Int32 maximum | `--limitDescriptionLength 250` |

