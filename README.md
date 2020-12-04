# Sitko.Grpc.Tools.Doc

MSBuild Target for generate documentation from gRPC

# Installation

```
dotnet add package Sitko.Grpc.Tools.Doc
```

# Basic usage

Specify DocOutputDir on Protobuf

```xml
<ItemGroup>
    <Protobuf Include="**/*.proto" DocOutputDir="docs" />
</ItemGroup>
```


