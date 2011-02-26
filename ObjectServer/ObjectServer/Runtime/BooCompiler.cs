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
    internal sealed class BooCompiler : ICompiler
    {
        //因为我们框架里的 C# 代码没有实际用到 Boo 的其他 DLL，所以这里需要引用一下 Boo 各个 DLL 里的东西让构建程序/IDE
        //自动为我们复制和加载 Boo 的各个 DLL

        private static readonly object refLang = typeof(Boo.Lang.IQuackFu);
        private static readonly object refBooExtensions = typeof(Boo.Lang.Extensions.AssertMacro);
        private static readonly object refDepend = typeof(Boo.Lang.Parser.BooParser);
        private static readonly object refPatternMatching = typeof(Boo.Lang.PatternMatching.MatchMacro);
        private static readonly object refUserful = typeof(Boo.Lang.Useful.PlatformInformation);


        #region ICompiler 成员

        public Assembly CompileFromFile(IEnumerable<string> sourceFiles)
        {
            var compiler = new Boo.Lang.Compiler.BooCompiler();
            SetCompilerParameters(compiler);

            foreach (var source in sourceFiles)
            {
                compiler.Parameters.Input.Add(new FileInput(source));
            }

            CompilerContext context = compiler.Run();

            LogWarnings(context);

            if (context.GeneratedAssembly == null)
            {
                LogErrors(context);
                var errors = CreateClrCompilerErrorCollection(context);
                throw new CompileException("Failed to compile module", errors);
            }

            return context.GeneratedAssembly;
        }

        private System.CodeDom.Compiler.CompilerErrorCollection
            CreateClrCompilerErrorCollection(CompilerContext context)
        {
            var errors = new System.CodeDom.Compiler.CompilerErrorCollection();
            foreach (var booError in context.Errors)
            {
                errors.Add(CreateClrCompilerError(booError));
            }
            return errors;
        }

        private static void SetCompilerParameters(Boo.Lang.Compiler.BooCompiler compiler)
        {
            var coreAssembly = typeof(Model.ModelBase).Assembly;
            compiler.Parameters.Pipeline = new CompileToMemory();
            compiler.Parameters.Ducky = true;
            compiler.Parameters.WarnAsError = false;
            compiler.Parameters.Debug = ObjectServerStarter.Configuration.Debug;
            compiler.Parameters.AddAssembly(coreAssembly);
            compiler.Parameters.AddAssembly(typeof(log4net.ILog).Assembly);
        }

        private static void LogWarnings(CompilerContext context)
        {

            foreach (var w in context.Warnings)
            {
                var msg = string.Format("{0}({1},{2}): {3}",
                    w.LexicalInfo.FullPath,
                    w.LexicalInfo.Line,
                    w.LexicalInfo.Column,
                    w.Message);
                Logger.Warn(() => msg);
            }
        }

        private static void LogErrors(CompilerContext context)
        {
            foreach (CompilerError error in context.Errors)
            {
                var msg = string.Format("{0}({1},{2}): {3}",
                   error.LexicalInfo.FullPath,
                   error.LexicalInfo.Line,
                   error.LexicalInfo.Column,
                   error.Message);
                Logger.Error(() => msg);
            }
        }

        private System.CodeDom.Compiler.CompilerError
            CreateClrCompilerError(Boo.Lang.Compiler.CompilerError booError)
        {
            var err = new System.CodeDom.Compiler.CompilerError()
            {
                FileName = booError.LexicalInfo.FullPath,
                Line = booError.LexicalInfo.Line,
                Column = booError.LexicalInfo.Column,
                ErrorText = booError.Message,
                IsWarning = false,
            };
            return err;
        }

        #endregion
    }
}
