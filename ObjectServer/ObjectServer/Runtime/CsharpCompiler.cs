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
        #region ICompiler 成员

        public Assembly CompileFromFile(IEnumerable<string> sourceFiles)
        {
            var selfAssembly = Assembly.GetExecutingAssembly();
            //声明C#或VB的CodeDOM 
            using (var provider = new CSharpCodeProvider())
            {

                //设置编译参数，加入所需的组件 
                var options = new CompilerParameters();
                options.ReferencedAssemblies.Add(selfAssembly.Location);
                options.GenerateInMemory = true;
                options.GenerateExecutable = false;
                options.OutputAssembly = "MyDemo";

                //开始编译
                var result = provider.CompileAssemblyFromFile(options, sourceFiles.ToArray());

                return result.CompiledAssembly;
            }
        }

        #endregion
    }
}
