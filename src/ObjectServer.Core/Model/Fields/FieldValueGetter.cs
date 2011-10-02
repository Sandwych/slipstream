using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    /// <summary>
    /// 用户获取字段值的用户回调函数
    /// </summary>
    /// <param name="scope">ResourceScope 服务上下文</param>
    /// <param name="ids">数据库记录 IDs</param>
    /// <returns>返回一个字典，对应 ids 参数给予的各个记录的字段值下。</returns>
    public delegate Dictionary<long, object>
        FieldValueGetter(ITransactionContext scope, long[] ids);
}
