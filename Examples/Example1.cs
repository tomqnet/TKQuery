using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKQuery.Examples
{

    /// <summary>
    /// Using TKQuery basics
    /// </summary>
    class Example1
    {
        public void SelectOne()
        {
            //IDBConnection is needed, here is example of using Microsoft Sql Server connection
            var dbConnection = new System.Data.SqlClient.SqlConnection("your connection string");
            //We need QueryManager instance
            var queryManager = new QueryManager(dbConnection);
            //We get result of our sql query
            //Query must contains exact fields names like ones used in class
            var result = queryManager.Execute<BasicClass>(new Query("select 1 'ID', 'tomqNET' 'Name', getdate() 'RegisterDate'"));

            //result.ID -> 1
            //result.Name -> tomqNET
            //result.RegisterDate -> '2014-01-01 00:00:00'
        }

        public void SelectOneWithParameters()
        {
            //IDBConnection is needed, here is example of using Microsoft Sql Server connection
            var dbConnection = new System.Data.SqlClient.SqlConnection("your connection string");
            //We need QueryManager instance
            var queryManager = new QueryManager(dbConnection);
            //You can pass parameters to query (stored procedures and parameters)            
            var result = queryManager.Execute<BasicClass>(new Query("select @1 'ID', 'tomqNET' @2, getdate() 'RegisterDate'", new object[]{
                1, //@1
                "tomqNET" //@2
            }));

            //result.ID -> 1
            //result.Name -> tomqNET
            //result.RegisterDate -> '2014-01-01 00:00:00'
        }

        public void SelectListWithParameters()
        {
            //IDBConnection is needed, here is example of using Microsoft Sql Server connection
            var dbConnection = new System.Data.SqlClient.SqlConnection("your connection string");
            //We need QueryManager instance
            var queryManager = new QueryManager(dbConnection);
            //You can fill lists with your class           
            var result = queryManager.Execute<List<BasicClass>>(new Query("select @1 'ID', 'tomqNET' @2, getdate() 'RegisterDate'", new object[]{
                1, //@1
                "tomqNET" //@2
            }));

            //result.First().ID -> 1
            //result.First().Name -> tomqNET
            //result.First().RegisterDate -> '2014-01-01 00:00:00'

            //var first = result[0]
            //first.ID -> 1
            //first.Name -> tomqNET
            //first.RegisterDate -> '2014-01-01 00:00:00'
        }

        public class BasicClass
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public DateTime RegisterDate { get; set; }
        }
    }
}
