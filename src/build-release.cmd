rmdir /s /q "%HOMEDRIVE%%HOMEPATH%\.nuget\packages\Dapper-QueryBuilder"

if not exist packages-local mkdir packages-local

dotnet clean
dotnet build -c release DapperQueryBuilder\DapperQueryBuilder.csproj
dotnet test  DapperQueryBuilder.Tests\DapperQueryBuilder.Tests.csproj

REM TODO: Generate symbols into its own symbols nuget (in the new snupkg format)? We would remove PDB from main nupkg (AllowedOutputExtensionsInPackageBuildOutputFolder) and pack with options --include-symbols -p:SymbolPackageFormat=snupkg
dotnet pack DapperQueryBuilder\DapperQueryBuilder.csproj -c release -o .\packages-local\ /p:ContinuousIntegrationBuild=true