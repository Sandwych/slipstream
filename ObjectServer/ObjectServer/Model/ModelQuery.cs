using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ModelQuery
    {
        private IModel model;
        private ISession session;

        public ModelQuery(ISession session, IModel model)
        {
            this.session = session;
            this.model = model;
        }

        public object[] Search(object[][] domain, long offset, long limit)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("exp");
            }

            var exp = new DomainParser(this.model, domain);

            //这里检查权限等
            if (this.model.ContainsField(ModelBase.ActiveFieldName)
                && !exp.ContainsField(ModelBase.ActiveFieldName))
            {
                exp.AddExpression(new object[] { ModelBase.ActiveFieldName, "=", true });
            }

            var sql = string.Format(
                "select id from \"{0}\" where {1}",
                this.model.TableName, exp.ToSql());

            using (var cmd = this.session.Database.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    var result = new List<object>();
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt64(0));
                    }
                    return result.ToArray();
                }
            }
        }
    }
}
