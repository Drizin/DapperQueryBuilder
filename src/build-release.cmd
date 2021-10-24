rmdir /s /q "%HOMEPATH%\.nuget\packages\codegencs"
rmdir /s /q "%HOMEPATH%\.nuget\packages\codegencs.schema"

if not exist packages-local mkdir packages-local

dotnet build -c release DapperQueryBuilder\DapperQueryBuilder.csproj
dotnet test  DapperQueryBuilder.Tests\DapperQueryBuilder.Tests.csproj
