using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;


namespace ObjectServer.Runtime
{
    /// <summary>
    /// Boo 语言代码编译器，用于把模块的代码编译成内存中的 Assembly
    /// </summary>
    public class BooCompiler : ICompiler
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
           MethodBase.GetCurrentMethod().DeclaringType);

        #region ICompiler 成员

        public Assembly Compile(IEnumerable<string> sourceFiles)
        {
            var coreAssembly = typeof(Model.ModelBase).Assembly;
            var compiler = new Boo.Lang.Compiler.BooCompiler();
            compiler.Parameters.Pipeline = new CompileToMemory();
            compiler.Parameters.Ducky = true;
            compiler.Parameters.WarnAsError = true;
            compiler.Parameters.AddAssembly(coreAssembly);

            foreach (var source in sourceFiles)
            {
                compiler.Parameters.Input.Add(new FileInput(source));
            }

            CompilerContext context = compiler.Run();

            //编译失败
            if (context.GeneratedAssembly == null)         
            {
                LogError(context);
            }

            return context.GeneratedAssembly;
        }

        private static void LogError(CompilerContext context)
        {
            foreach (CompilerError error in context.Errors)
            {
                Log.ErrorFormat(error.ToString());
            }
        }

        #endregion
    }
}
