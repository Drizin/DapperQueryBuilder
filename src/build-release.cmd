rmdir /s /q "%HOMEDRIVE%%HOMEPATH%\.nuget\packages\Dapper-QueryBuilder"

if not exist packages-local mkdir packages-local

dotnet build -c release DapperQueryBuilder\DapperQueryBuilder.csproj
dotnet test  DapperQueryBuilder.Tests\DapperQueryBuilder.Tests.csproj
dotnet pack -c release DapperQueryBuilder\DapperQueryBuilder.csproj
