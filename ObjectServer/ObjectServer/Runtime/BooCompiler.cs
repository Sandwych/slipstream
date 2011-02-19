using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Parser;


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

        public Assembly CompileFromFile(IEnumerable<string> sourceFiles)
        {
            var compiler = new Boo.Lang.Compiler.BooCompiler();
            var coreAssembly = typeof(Model.TableModel).Assembly;
            compiler.Parameters.Pipeline = new CompileToMemory();
            compiler.Parameters.Ducky = true;
            compiler.Parameters.WarnAsError = false;

            compiler.Parameters.AddAssembly(coreAssembly);
            compiler.Parameters.AddAssembly(typeof(log4net.ILog).Assembly);
            var t1 = typeof(Boo.Lang.Parser.BooParser);//WORKAROUND

            foreach (var source in sourceFiles)
            {
                compiler.Parameters.Input.Add(new FileInput(source));
            }

            CompilerContext context = compiler.Run();

            LogWarnings(context);

            //编译失败
            if (context.GeneratedAssembly == null)
            {
                Console.WriteLine("Failed to compile");
                LogErrors(context);
                throw new Exception("Failed to compile module");
            }

            return context.GeneratedAssembly;
        }

        private static void LogWarnings(CompilerContext context)
        {

            if (Log.IsWarnEnabled)
            {
                foreach (var w in context.Warnings)
                {
                    Log.WarnFormat("WARNING: {0}", w.Message);
                }
            }
        }

        private static void LogErrors(CompilerContext context)
        {
            foreach (CompilerError error in context.Errors)
            {
                Log.ErrorFormat(error.ToString());
            }
        }

        #endregion
    }
}
