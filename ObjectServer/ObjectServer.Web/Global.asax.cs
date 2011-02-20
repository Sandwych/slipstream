using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace ObjectServer.Web
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            this.EnsureFrameworkInitialized();

        }

        private void EnsureFrameworkInitialized()
        {
            //TODO: 初始化为测试配置
            if (!ObjectServerStarter.Initialized)
            {
                var cfg = new Configuration()
                {
                    ConfigurationPath = null,
                    DbType = global::ObjectServer.Backend.DatabaseType.Postgresql,
                    DbHost = "localhost",
                    DbName = "objectserver",
                    DbPassword = "objectserver",
                    DbPort = 5432,
                    DbUser = "objectserver",
                    ModulePath = null,//Server.MapPath("~/modules"),
                    RootPassword = "root",
                    Debug = true,
                };

                ObjectServerStarter.Initialize(cfg);
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}