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
    internal sealed class CSharpCompiler : ICompiler
    {
        #region ICompiler 成员

        public Assembly CompileFromFile(IEnumerable<string> sourceFiles)
        {
            using (var provider = CreateCodeProvider())
            {
                var options = CreateCompilerParameters();
                var result = provider.CompileAssemblyFromFile(options, sourceFiles.ToArray());

                if (result.Errors.Count != 0)
                {
                    LogErrors(result.Errors);

                    throw new CompileException("Failed to compile files", result.Errors);
                }

                return result.CompiledAssembly;
            }
        }

        private static CSharpCodeProvider CreateCodeProvider()
        {
            var additionalOptions = new Dictionary<string, string>() 
            {
                { "CompilerVersion", "v4.0" } 
            };
            return new CSharpCodeProvider(additionalOptions);
        }

        private static CompilerParameters CreateCompilerParameters()
        {
            var selfAssembly = Assembly.GetExecutingAssembly();

            //设置编译参数，加入所需的组件 
            var options = new CompilerParameters();
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            options.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            options.ReferencedAssemblies.Add("System.Data.dll");
            options.ReferencedAssemblies.Add("System.Transactions.dll");
            options.ReferencedAssemblies.Add(selfAssembly.Location);
            options.GenerateInMemory = true;
            options.GenerateExecutable = false;
            options.IncludeDebugInformation = Infrastructure.Configuration.Debug;

            return options;
        }

        #endregion

        private static void LogErrors(CompilerErrorCollection errors)
        {
            foreach (CompilerError error in errors)
            {
                var msg = string.Format("{0}({1},{2}): {3}",
                 error.FileName,
                 error.Line,
                 error.Column,
                 error.ErrorText);

                if (error.IsWarning)
                {
                    Logger.Warn(() => error.ToString());
                }
                else
                {
                    Logger.Error(() => error.ToString());
                }
            }
        }
    }
}
