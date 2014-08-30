using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKQuery
{
    /// <summary>
    /// Container for parameter and value
    /// </summary>
    public class QueryParameter
    {
        private string name;
        private object value;

        public string Name { get { return name; } }
        public object Value { get { return value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public QueryParameter(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

    }
}
