using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation; //reference Microsoft.Build.dll v4.0
using Microsoft.Build.Logging;

namespace ObjectServer.Runtime
{
    public sealed class MSBuildProjectBuildEngine : IProjectBuildEngine
    {
        public MSBuildProjectBuildEngine()
        {

        }

        public bool Build(string projPath, string outDir)
        {
            //.Microsoft.Microsoftvar x = Microsoft.Build.Framework.ILogger

            var options = new Dictionary<string, string>();
            using (var buildEngine = new ProjectCollection(options))
            {
                buildEngine.RegisterLogger(new Microsoft.Build.Logging.ConsoleLogger());
                //TODO 添加 logger
                var project = buildEngine.LoadProject(projPath);
                var buildResult = project.Build();

                buildEngine.UnloadAllProjects();
                buildEngine.UnregisterAllLoggers();
                return buildResult;
            }
        }
    }
}
