using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder.Tests
{
    public class TestHelper
    {
        public static IConfiguration Configuration { get; set; }
        static TestHelper()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("TestSettings.json")
                .Build();
        }
        public static string GetMSSQLConnectionString() => Configuration.GetConnectionString("MSSQLConnection");
        public static string GetPostgreSQLConnectionString() => Configuration.GetConnectionString("PostgreSQLConnection");

    }
}
