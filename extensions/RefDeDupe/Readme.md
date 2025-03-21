# Ref Dedupe

## Description

Ref Dedupe identifies the necessary packages to include in `.nuspec` files for clients to declare dependencies.

## Usage

```shell
RefDeDupe.exe <assets-file-path> [targetFramework=.NETStandard,Version=v2.1]
```

* Example

```shell
RefDeDupe.exe c:\MyProject\obj\project.assets.json
```

## Methodology

Ref Dedupe relies on the `project.assets.json` file. You can generate this file for your project using the following command:

```shell
dotnet restore --no-cache
```

### Steps

1. **Scan Package Targets**  
   The tool scans all package targets (excluding project targets) to identify all packages referenced by the project.

2. **Analyze Dependencies**  
   It examines the dependencies of each package to detect duplicated or transitive references. If a package is found to be a transitive dependency, it is removed from the required list. For example, if a project references both `Microsoft.Extensions.Logging.Console` and `Microsoft.Extensions.Logging`, the tool will exclude `Microsoft.Extensions.Logging` from the dependency list because it is already a transitive dependency of `Microsoft.Extensions.Logging.Console`. This ensures that only the necessary dependencies are explicitly declared.
