using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using NHibernate.SqlCommand;

using ObjectServer.Data;

namespace ObjectServer
{
    [Export("sql", typeof(IUserSessionStore))]
    internal class SqlUserSessionStore : IUserSessionStore
    {
        private static readonly SqlString SelectByIdSql =
                 new SqlString(@"select * from ""core_session"" where ""token"" = ", Parameter.Placeholder);
        private static readonly SqlString SelectByUserIdSql =
            new SqlString(@"select * from ""core_session"" where ""userid"" = ", Parameter.Placeholder);
        private static readonly SqlString UpdateLastActivityTimeSql = SqlString.Parse(
            @"update ""core_session"" set ""last_activity_time"" = ? where ""last_activity_time"" < ? and ""token"" = ?");

        private readonly IDataContext _dataContext;

        public SqlUserSessionStore(IDataContext dataContext)
        {
            this._dataContext = dataContext;
        }

        public UserSession GetByToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }

            var records = this._dataContext.QueryAsDictionary(SelectByIdSql, token);
            if (records.Length > 1)
            {
                throw new Exceptions.DataException("More than one session id in table [core_session]!");
            }
            else if (records.Length < 1)
            {
                return null;
            }
            else
            {
                var record = records[0];
                return new UserSession(record);
            }
        }

        public UserSession GetByUserId(long userId)
        {
            var records = this._dataContext.QueryAsDictionary(SelectByUserIdSql, userId);
            if (records.Length > 1)
            {
                throw new Exceptions.DataException("More than one user id in table [core_session]!");
            }
            else if (records.Length < 1)
            {
                return null;
            }
            else
            {
                var record = records[0];
                return new UserSession(record);
            }
        }

        public void Put(UserSession session)
        {

            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            //TODO 移为静态变量
            var sql = SqlString.Parse(
                @"insert into ""core_session""(""token"", ""start_time"", ""last_activity_time"", ""userid"", ""login"") values(?,?,?,?,?)");
            var n = this._dataContext.Execute(sql, session.Token, session.StartTime, session.LastActivityTime, session.UserId, session.Login);
            if (n != 1)
            {
                throw new Exceptions.DataException("Failed to put session");
            }
        }

        public void Remove(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }

            var sql = SqlString.Parse("delete from core_session where token=?");
            var n = this._dataContext.Execute(sql, token);
            if (n != 1)
            {
                throw new Exceptions.DataException("Failed to remove session");
            }
        }

        public void Pulse(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }

            var s = this.GetByToken(token);
            if (s.IsActive)
            {
                var now = DateTime.Now;
                this._dataContext.Execute(UpdateLastActivityTimeSql, now, now, token);
            }
            else
            {
                this.Remove(token);
            }
        }
    }
}
