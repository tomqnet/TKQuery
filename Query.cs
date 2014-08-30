using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKQuery
{
    /// <summary>
    /// Contains query string and parameters
    /// </summary>
    public class Query
    {
        protected string text;
        protected QueryParameter[] parameters;
        protected string id;

        public string Text { get { return text; } }
        public QueryParameter[] Parameters { get { return parameters; } }
        public string ID
        {
            get
            {
                if (id == null)
                {
                    var random = System.Security.Cryptography.RandomNumberGenerator.Create();
                    byte[] data = new byte[24];
                    random.GetBytes(data);
                    id = String.Join("", data);
                }
                return id;
            }
            set
            {
                this.id = value;
            }
        }

        public Query(string text, QueryParameter[] parameters, string id = null)
        {
            this.text = text;
            if (parameters == null)
            {
                parameters = new List<QueryParameter>().ToArray();
            }
            this.parameters = parameters;
            this.id = id;
        }

        public Query(string text, object[] parameters, string id = null)
        {
            this.text = text;
            var unnamedParameters = new List<QueryParameter>();
            foreach (var unnamedParameter in parameters)
            {
                var sb = new StringBuilder();
                sb.Append("@");
                sb.Append((unnamedParameters.Count + 1).ToString());
                var parameterName = sb.ToString();
                unnamedParameters.Add(new QueryParameter(parameterName, unnamedParameter));
            }
            this.parameters = unnamedParameters.ToArray();
            this.id = id;
        }

        public Query(string text, string id = null)
        {
            this.text = text;
            this.parameters = new QueryParameter[] { };
            this.id = id;
        }

    }

}
