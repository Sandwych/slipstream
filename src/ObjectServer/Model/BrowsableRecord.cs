﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Diagnostics;

namespace ObjectServer.Model
{
    //TODO 处理 lazy 的字段
    public sealed class BrowsableRecord : DynamicObject
    {
        private IDictionary<string, object> record;
        private IMetaModel metaModel;
        private IResourceScope scope;

        public BrowsableRecord(IResourceScope scope, IMetaModel metaModel, long id)
        {
            this.metaModel = metaModel;
            this.scope = scope;
            this.record = metaModel.ReadInternal(scope, new long[] { id }, null)[0];
        }

        public BrowsableRecord(IResourceScope scope, IMetaModel metaModel, IDictionary<string, object> record)
        {
            this.metaModel = metaModel;
            this.scope = scope;
            this.record = record;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            throw new NotSupportedException();
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            throw new NotSupportedException();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            throw new NotSupportedException();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Debug.Assert(this.record != null);
            Debug.Assert(this.metaModel != null);
            Debug.Assert(this.scope != null);

            result = null;
            if (!metaModel.Fields.ContainsKey(binder.Name))
            {
                return false;
            }

            var metaField = metaModel.Fields[binder.Name];

            result = metaField.BrowseField(this.scope, this.record);

            return true;
        }

    }
}