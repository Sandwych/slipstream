using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;

using Microsoft.CSharp;

namespace ObjectServer.Runtime
{
    public class CsharpCompiler : ICompiler
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        #region ICompiler 成员

        public Assembly CompileFromFile(IEnumerable<string> sourceFiles)
        {
            var selfAssembly = Assembly.GetExecutingAssembly();
            //声明C#或VB的CodeDOM 
            var additionalOptions = new Dictionary<string, string>() 
            {
                { "CompilerVersion", "v3.5" } 
            };

            using (var provider = new CSharpCodeProvider(additionalOptions))
            {

                //设置编译参数，加入所需的组件 
                var options = new CompilerParameters();
                options.ReferencedAssemblies.Add("System.dll");
                options.ReferencedAssemblies.Add("System.Core.dll");
                options.ReferencedAssemblies.Add("System.Data.dll");
                options.ReferencedAssemblies.Add("System.Transactions.dll");
                options.ReferencedAssemblies.Add(selfAssembly.Location);
                options.GenerateInMemory = true;
                options.GenerateExecutable = false;              
                //options.OutputAssembly = "MyDemo";

                //开始编译
                var result = provider.CompileAssemblyFromFile(options, sourceFiles.ToArray());

                if (result.Errors.Count != 0)
                {
                    LogErrors(result.Errors);

                    throw new CompileException("Failed to compile files", result.Errors);
                }

                return result.CompiledAssembly;
            }
        }

        #endregion

        private static void LogErrors(CompilerErrorCollection errors)
        {
            foreach (CompilerError error in errors)
            {
                Log.ErrorFormat(error.ToString());
            }
        }
    }
}
