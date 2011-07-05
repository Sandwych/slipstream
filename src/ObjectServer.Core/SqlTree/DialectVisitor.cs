using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    /// <summary>
    /// 方言调整器基类，用于修剪 SQL AST 树，按照各个数据库 SQL 方言进行节点的调整
    /// </summary>
    public abstract class DialectVisitor : StackedVisitor
    {
    }
}
