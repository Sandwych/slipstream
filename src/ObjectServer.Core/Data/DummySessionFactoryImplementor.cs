using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate;
using NHibernate.Engine;

namespace ObjectServer.Data
{
    public class DummyISessionFactoryImplementor : ISessionFactoryImplementor
    {
        public NHibernate.Connection.IConnectionProvider ConnectionProvider
        {
            get { throw new NotImplementedException(); }
        }

        public NHibernate.Context.ICurrentSessionContext CurrentSessionContext
        {
            get { throw new NotImplementedException(); }
        }

        public NHibernate.Dialect.Dialect Dialect
        {
            get { return DataProvider.Dialect; }
        }

        public NHibernate.Proxy.IEntityNotFoundDelegate EntityNotFoundDelegate
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary<string, NHibernate.Cache.ICache> GetAllSecondLevelCacheRegions()
        {
            throw new NotImplementedException();
        }

        public NHibernate.Persister.Collection.ICollectionPersister GetCollectionPersister(string role)
        {
            throw new NotImplementedException();
        }

        public Iesi.Collections.Generic.ISet<string> GetCollectionRolesByEntityParticipant(string entityName)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Persister.Entity.IEntityPersister GetEntityPersister(string entityName)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Id.IIdentifierGenerator GetIdentifierGenerator(string rootEntityName)
        {
            throw new NotImplementedException();
        }

        public string[] GetImplementors(string entityOrClassName)
        {
            throw new NotImplementedException();
        }

        public string GetImportedClassName(string name)
        {
            throw new NotImplementedException();
        }

        public NamedQueryDefinition GetNamedQuery(string queryName)
        {
            throw new NotImplementedException();
        }

        public NamedSQLQueryDefinition GetNamedSQLQuery(string queryName)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Cache.IQueryCache GetQueryCache(string regionName)
        {
            throw new NotImplementedException();
        }

        public ResultSetMappingDefinition GetResultSetMapping(string resultSetRef)
        {
            throw new NotImplementedException();
        }

        public string[] GetReturnAliases(string queryString)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Type.IType[] GetReturnTypes(string queryString)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Cache.ICache GetSecondLevelCacheRegion(string regionName)
        {
            throw new NotImplementedException();
        }

        public IInterceptor Interceptor
        {
            get { throw new NotImplementedException(); }
        }

        public ISession OpenSession(System.Data.IDbConnection connection, bool flushBeforeCompletionEnabled, bool autoCloseSessionEnabled, ConnectionReleaseMode connectionReleaseMode)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Cache.IQueryCache QueryCache
        {
            get { throw new NotImplementedException(); }
        }

        public NHibernate.Engine.Query.QueryPlanCache QueryPlanCache
        {
            get { throw new NotImplementedException(); }
        }

        public NHibernate.Exceptions.ISQLExceptionConverter SQLExceptionConverter
        {
            get { throw new NotImplementedException(); }
        }

        public NHibernate.Dialect.Function.SQLFunctionRegistry SQLFunctionRegistry
        {
            get { throw new NotImplementedException(); }
        }

        public NHibernate.Cfg.Settings Settings
        {
            get { throw new NotImplementedException(); }
        }

        public NHibernate.Stat.IStatisticsImplementor StatisticsImplementor
        {
            get { throw new NotImplementedException(); }
        }

        public NHibernate.Transaction.ITransactionFactory TransactionFactory
        {
            get { throw new NotImplementedException(); }
        }

        public NHibernate.Persister.Entity.IEntityPersister TryGetEntityPersister(string entityName)
        {
            throw new NotImplementedException();
        }

        public string TryGetGuessEntityName(Type implementor)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Cache.UpdateTimestampsCache UpdateTimestampsCache
        {
            get { throw new NotImplementedException(); }
        }

        public string GetIdentifierPropertyName(string className)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Type.IType GetIdentifierType(string className)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Type.IType GetReferencedPropertyType(string className, string propertyName)
        {
            throw new NotImplementedException();
        }

        public bool HasNonIdentifierPropertyNamedId(string className)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public ICollection<string> DefinedFilterNames
        {
            get { throw new NotImplementedException(); }
        }

        public void Evict(Type persistentClass, object id)
        {
            throw new NotImplementedException();
        }

        public void Evict(Type persistentClass)
        {
            throw new NotImplementedException();
        }

        public void EvictCollection(string roleName, object id)
        {
            throw new NotImplementedException();
        }

        public void EvictCollection(string roleName)
        {
            throw new NotImplementedException();
        }

        public void EvictEntity(string entityName, object id)
        {
            throw new NotImplementedException();
        }

        public void EvictEntity(string entityName)
        {
            throw new NotImplementedException();
        }

        public void EvictQueries(string cacheRegion)
        {
            throw new NotImplementedException();
        }

        public void EvictQueries()
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, NHibernate.Metadata.IClassMetadata> GetAllClassMetadata()
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, NHibernate.Metadata.ICollectionMetadata> GetAllCollectionMetadata()
        {
            throw new NotImplementedException();
        }

        public NHibernate.Metadata.IClassMetadata GetClassMetadata(string entityName)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Metadata.IClassMetadata GetClassMetadata(Type persistentClass)
        {
            throw new NotImplementedException();
        }

        public NHibernate.Metadata.ICollectionMetadata GetCollectionMetadata(string roleName)
        {
            throw new NotImplementedException();
        }

        public ISession GetCurrentSession()
        {
            throw new NotImplementedException();
        }

        public FilterDefinition GetFilterDefinition(string filterName)
        {
            throw new NotImplementedException();
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public ISession OpenSession()
        {
            throw new NotImplementedException();
        }

        public ISession OpenSession(System.Data.IDbConnection conn, IInterceptor sessionLocalInterceptor)
        {
            throw new NotImplementedException();
        }

        public ISession OpenSession(IInterceptor sessionLocalInterceptor)
        {
            throw new NotImplementedException();
        }

        public ISession OpenSession(System.Data.IDbConnection conn)
        {
            throw new NotImplementedException();
        }

        public IStatelessSession OpenStatelessSession(System.Data.IDbConnection connection)
        {
            throw new NotImplementedException();
        }

        public IStatelessSession OpenStatelessSession()
        {
            throw new NotImplementedException();
        }

        public NHibernate.Stat.IStatistics Statistics
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
