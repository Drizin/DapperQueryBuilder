$msbuild = ( 
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\msbuild.exe",
          "$Env:programfiles\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe",
    "${Env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe",
    "${Env:ProgramFiles(x86)}\MSBuild\12.0\Bin\MSBuild.exe"
) | Where-Object { Test-Path $_ } | Select-Object -first 1

$configuration = 'Release'

Remove-Item -Recurse -Force -ErrorAction Ignore ".\packages-local"
Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\dapper-querybuilder"
Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\dapperquerybuilder.strongname"

New-Item -ItemType Directory -Force -Path ".\packages-local"

dotnet clean DapperQueryBuilder.sln
dotnet clean DapperQueryBuilder\DapperQueryBuilder.csproj
dotnet clean DapperQueryBuilder.StrongName\DapperQueryBuilder.StrongName.csproj

# DapperQueryBuilder + nupkg/snupkg (dotnet build is the best at restoring packages; but for deterministic builds we need msbuild)
dotnet build -c release DapperQueryBuilder\DapperQueryBuilder.csproj
& $msbuild "DapperQueryBuilder\DapperQueryBuilder.csproj" `
           /t:Pack                                        `
           /p:PackageOutputPath="..\packages-local\"      `
           '/p:targetFrameworks="net461;netstandard2.0;net5.0"'  `
           /p:Configuration=$configuration                `
           /p:IncludeSymbols=true                         `
           /p:SymbolPackageFormat=snupkg                  `
           /verbosity:minimal                             `
           /p:ContinuousIntegrationBuild=true

# DapperQueryBuilder.StrongName + nupkg/snupkg (dotnet build is the best at restoring packages; but for deterministic builds we need msbuild)
dotnet build -c release DapperQueryBuilder.StrongName\DapperQueryBuilder.StrongName.csproj
& $msbuild "DapperQueryBuilder.StrongName\DapperQueryBuilder.StrongName.csproj" `
           /t:Pack                                        `
           /p:PackageOutputPath="..\packages-local\"      `
           '/p:targetFrameworks="net461;netstandard2.0;net5.0"'  `
           /p:Configuration=$configuration                `
           /p:IncludeSymbols=true                         `
           /p:SymbolPackageFormat=snupkg                  `
           /verbosity:minimal                             `
           /p:ContinuousIntegrationBuild=true




# Unit tests
if ($configuration -eq "Debug")
{
    dotnet build -c release DapperQueryBuilder.Tests\DapperQueryBuilder.Tests.csproj
    dotnet test  DapperQueryBuilder.Tests\DapperQueryBuilder.Tests.csproj
}
