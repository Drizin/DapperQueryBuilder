using Dapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DapperQueryBuilder.Tests
{
    public class CommandBuilderTests
    {
        IDbConnection cn;

        #region Setup
        [SetUp]
        public void Setup()
        {
            string connectionString = @"Data Source=LENOVOFLEX5\SQLEXPRESS;
                            Initial Catalog=AdventureWorks;
                            Integrated Security=True;";
            cn = new SqlConnection(connectionString);
        }
        #endregion

        int businessEntityID = 1;
        string nationalIDNumber = "295847284";
        DateTime birthDate = new DateTime(1969, 01, 29);
        string maritalStatus = "S"; // single
        string gender = "M";

        [Test]
        public void TestStoredProcedure()
        {
            var q = cn.CommandBuilder($"[HumanResources].[uspUpdateEmployeePersonalInfo]")
                .AddParameter("BusinessEntityID", businessEntityID)
                .AddParameter("NationalIDNumber", nationalIDNumber)
                .AddParameter("BirthDate", birthDate)
                .AddParameter("MaritalStatus", maritalStatus)
                .AddParameter("Gender", gender);
            int affected = q.Execute(commandType: CommandType.StoredProcedure);
        }

        [Test]
        public void TestStoredProcedureExec()
        {
            var q = cn.CommandBuilder($@"
                DECLARE @ret INT;
                EXEC @RET = [HumanResources].[uspUpdateEmployeePersonalInfo] 
                   @BusinessEntityID={businessEntityID}
                  ,@NationalIDNumber={nationalIDNumber}
                  ,@BirthDate={birthDate}
                  ,@MaritalStatus={maritalStatus}
                  ,@Gender={gender};
                SELECT @RET;
                ");

            int affected = q.Execute();
        }


        [Test]
        public void TestStoredProcedure2()
        {
            var q = cn.CommandBuilder($"[dbo].[uspLogError]")
                .AddParameter("ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue)
                .AddParameter("ErrorLogID ", dbType: DbType.Int32, direction: ParameterDirection.Output);
            int affected = q.Execute(commandType: CommandType.StoredProcedure);
            int returnValue = q.Parameters.Get<int>("ReturnValue");
        }

    }
}
