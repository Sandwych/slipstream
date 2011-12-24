using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using ObjectServer.Data;
using ObjectServer.Runtime;

namespace ObjectServer
{
    public interface IServiceContext : IDisposable, IEquatable<IServiceContext>
    {
        UserSession UserSession { get; }
        IRuleConstraintEvaluator RuleConstraintEvaluator { get; }
        IUserSessionStore UserSessionService { get; }
        IResource GetResource(string resName);
        int GetResourceDependencyWeight(string resName);
        IDataContext DataContext { get; }
        IResourceContainer Resources { get; }
        ILogger BizLogger { get; }
    }
}
