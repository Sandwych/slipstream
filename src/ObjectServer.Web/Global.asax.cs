using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Configuration;

namespace ObjectServer.Web
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            this.EnsureFrameworkInitialized();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        private void EnsureFrameworkInitialized()
        {
            //TODO: 初始化为测试配置
            if (!ObjectServerStarter.Initialized)
            {
                var cfg = (Config)ConfigurationManager.GetSection("objectserver-config");
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