using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Runtime
{
    /// <summary>
    /// SINGLETON 
    /// TODO 线程安全
    /// </summary>
    public sealed class CompilerProvider
    {
        static readonly CompilerProvider s_instance = new CompilerProvider();

        Dictionary<string, ICompiler> compilers = new Dictionary<string, ICompiler>()
        {
            { "boo", new BooCompiler() },
            { "csharp", new CSharpCompiler() },
        };

        private CompilerProvider()
        {
        }

        public static ICompiler GetCompiler(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return s_instance.compilers["boo"];
            }

            if (!s_instance.compilers.ContainsKey(language.Trim().ToLowerInvariant()))
            {
                throw new NotSupportedException(
                    string.Format("Not supported language: '{0}'", language));
            }

            return s_instance.compilers[language];
        }
    }
}
