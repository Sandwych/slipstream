using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public enum OnDeleteAction
    {
        NoAction,
        Cascade,
        Restrict,
        SetNull,
    }
}
