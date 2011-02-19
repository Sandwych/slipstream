using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public enum ReferentialAction
    {
        Restrict,
        NoAction,
        Cascade,
        SetNull,
        SetDefault
    }
}
