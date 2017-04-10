using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Entity
{
    public enum OnDeleteAction
    {
        NoAction,
        Cascade,
        Restrict,
        SetNull,
    }
}
