using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using Microsoft.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp;

namespace SlipStream.Runtime
{
    internal sealed class CSharpCompiler : ICompiler
    {
        #region ICompiler 成员

        public Assembly BuildProject(string projectFile)
        {
            if (string.IsNullOrEmpty(projectFile))
            {
                throw new ArgumentNullException(nameof(projectFile));
            }
            using (var workspace = MSBuildWorkspace.Create())
            {
                var proj = workspace.OpenProjectAsync(projectFile).Result;
                var options = proj.CompilationOptions.WithOutputKind(OutputKind.DynamicallyLinkedLibrary);
                var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);
                var pc = proj.GetCompilationAsync().Result
                    .WithOptions(options);
                if (pc != null && !string.IsNullOrEmpty(pc.AssemblyName))
                {
                    using (var ms = new MemoryStream())
                    {
                        var emitResult = pc.Emit(ms);
                        if (emitResult.Success)
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            return Assembly.Load(ms.ToArray());
                        }
                        else
                        {
                            throw new CompileException("Failed to emit assembly: " + projectFile, null);
                        }
                    }
                }
                else
                {
                    throw new CompileException("Failed to compile project: " + projectFile, null);
                }
            }

        }

        private static CompilerParameters CreateCompilerParameters()
        {
            var selfAssembly = Assembly.GetExecutingAssembly();
            var infrastructureAssembly = typeof(ShellSettings).Assembly;
            var sharedAssembly = typeof(SlipStream.Model.Criterion).Assembly;
            var maltAssembly = typeof(ObjectExtensions).Assembly;

            //设置编译参数，加入所需的组件 
            var options = new CompilerParameters();
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            options.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            options.ReferencedAssemblies.Add("System.Data.dll");
            options.ReferencedAssemblies.Add("System.Transactions.dll");
            options.ReferencedAssemblies.Add(infrastructureAssembly.Location);
            options.ReferencedAssemblies.Add(sharedAssembly.Location);
            options.ReferencedAssemblies.Add(maltAssembly.Location);
            options.ReferencedAssemblies.Add(selfAssembly.Location);
            options.GenerateInMemory = true;
            options.GenerateExecutable = false;
            options.IncludeDebugInformation = SlipstreamEnvironment.Settings.Debug;

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
                    LoggerProvider.EnvironmentLogger.Warn(() => error.ToString());
                }
                else
                {
                    LoggerProvider.EnvironmentLogger.Error(() => error.ToString());
                }
            }
        }
    }

}
