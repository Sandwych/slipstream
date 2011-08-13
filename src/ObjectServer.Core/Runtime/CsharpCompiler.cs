using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Diagnostics;

using Microsoft.CSharp;

namespace ObjectServer.Runtime
{
    internal sealed class CSharpCompiler : ICompiler
    {
        #region ICompiler 成员

        public Assembly CompileFromFile(IEnumerable<string> sourceFiles)
        {
            if (sourceFiles == null)
            {
                throw new ArgumentNullException("sourceFiles");
            }

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
            var infrastructureAssembly = typeof(Config).Assembly;

            //设置编译参数，加入所需的组件 
            var options = new CompilerParameters();
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            options.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            options.ReferencedAssemblies.Add("System.Data.dll");
            options.ReferencedAssemblies.Add("System.Transactions.dll");
            options.ReferencedAssemblies.Add(selfAssembly.Location);
            options.ReferencedAssemblies.Add(infrastructureAssembly.Location);
            options.GenerateInMemory = true;
            options.GenerateExecutable = false;
            options.IncludeDebugInformation = Platform.Configuration.Debug;

            return options;
        }

        #endregion

        private static void LogErrors(CompilerErrorCollection errors)
        {
            Debug.Assert(errors != null);
               
            foreach (CompilerError error in errors)
            {
                var msg = string.Format("{0}({1},{2}): {3}",
                 error.FileName,
                 error.Line,
                 error.Column,
                 error.ErrorText);

                if (error.IsWarning)
                {
                    LoggerProvider.PlatformLogger.Warn(() => error.ToString());
                }
                else
                {
                    LoggerProvider.PlatformLogger.Error(() => error.ToString());
                }
            }
        }
    }
}
