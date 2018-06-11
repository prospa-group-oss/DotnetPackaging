# <img src="https://github.com/prospa-group/DotnetPackaging/blob/master/prospa60x60.png" alt="Prospa Engineering" width="50px"/> Prospa DotNet Packaging 


## Usage

Add the following package reference to you csproj:

`<PackageReference Include="Prospa.Packaging" Version="*" PrivateAssets="All" />`

The package description defaults to the package id, so add the following project to provide a real description:

```xml
  <PropertyGroup>
    <Description>{The package's description}</Description>
  </PropertyGroup>
```

## How to Build

```csharp
./build.ps1 -Target Local
```

Package will be output to `/.artifacts/packages/Prospa.Packaging.{Version}.nupkg`

## Versioning

To version bump, edit the `VersionPrefix` and/or `VersionSuffix` in `./version.props`

> When on non-release branches the `VersionSuffix` will always be set to `alpha` with the build number appended.
