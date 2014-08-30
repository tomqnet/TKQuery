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
            //We can fill classes in classes. Look on aliases.
            var result = queryManager.Execute<AdvancedClass>(new Query("select 1000 'ID', 'Lorem ipsum..' 'Text', 1 'Owner.ID', 'tomqNET' 'Owner.Name', getdate() 'Owner.RegisterDate'"));
            
            //result.ID -> 1000
            //result.Owner.ID -> 1
        }

        public class AdvancedClass
        {
            public int ID { get; set; }
            public int Text { get; set; }
            public BasicClass Owner { get; set; }
        }

        public class BasicClass
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public DateTime RegisterDate { get; set; }
        }
    }
}
