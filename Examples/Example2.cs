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
    class Example2
    {

        public void InsertEntity()
        {
            //IDBConnection is needed, here is example of using Microsoft Sql Server connection
            var dbConnection = new System.Data.SqlClient.SqlConnection("your connection string");
            //We need QueryManager instance
            var queryManager = new QueryManager(dbConnection);
            //We create entity that will be inserted to database
            var entity = new BasicClass()
            {
                Name = "tomqNET",
                RegisterDate = DateTime.Now
            };
            //We execute query but we need inserted row id (database design)
            var id = queryManager.Execute<int>(new Query("insert user select @1,@2; select @@identity;", new object[]{                
                entity.Name, //@1
                entity.RegisterDate //@2
            }));
            //We set id from database for our entity
            entity.ID = id;
        }

        public void NonQuery()
        {
            //IDBConnection is needed, here is example of using Microsoft Sql Server connection
            var dbConnection = new System.Data.SqlClient.SqlConnection("your connection string");
            //We need QueryManager instance
            var queryManager = new QueryManager(dbConnection);
            //Execute method does not need return results all the time (update, delete)        
            queryManager.Execute(new Query("delete from user"));
        }

        public class BasicClass
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public DateTime RegisterDate { get; set; }
        }
    }
}
