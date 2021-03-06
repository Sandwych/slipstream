﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Test
{

    [Resource]
    public sealed class FunctionalFieldEntities : AbstractSqlEntity
    {
        public const string EntityName = "test.functional_field_entities";

        public FunctionalFieldEntities()
            : base(EntityName)
        {
            Fields.Chars("name").WithLabel("Name").WithRequired().WithSize(64);
            Fields.ManyToOne("user", "core.user").WithValueGetter(GetUser);
            Fields.Integer("field1").WithRequired();
            Fields.Integer("field2").WithRequired();
            Fields.Integer("sum_field").WithValueGetter(GetSum).WithReadonly()
                .WithCriterionConverter(this.ConvertSumFieldCriterion);
        }

        private Dictionary<long, object> GetSum(IServiceContext ctx, IEnumerable<long> ids)
        {
            var fields = new string[] { "field1", "field2" };
            var records = this.ReadInternal(ids.ToArray(), fields);
            var result = new Dictionary<long, object>(ids.Count());
            foreach (var record in records)
            {
                var id = (long)record[AbstractEntity.IdFieldName];
                var field1 = (int)record["field1"];
                var field2 = (int)record["field2"];
                result[id] = field1 + field2;
            }

            return result;
        }

        private Criterion[] ConvertSumFieldCriterion(IServiceContext ctx, Criterion cr)
        {
            var ids = (long[])this.SearchInternal(null, null, 0, 0);
            var values = GetSum(ctx, ids);

            if (cr.Operator == "=")
            {
                var resultIds = values.Where(p => (int)p.Value == (int)cr.Value).Select(p => p.Key).ToArray();
                return new Criterion[] { new Criterion(IdFieldName, "in", resultIds) };
            }
            else if (cr.Operator == "!=")
            {
                var resultIds = values.Where(p => (int)p.Value != (int)cr.Value).Select(p => p.Key).ToArray();
                return new Criterion[] { new Criterion(IdFieldName, "in", resultIds) };
            }
            else
            {
                return new Criterion[] { Criterion.NegeativeCriterion };
            }
        }

        private Dictionary<long, object> GetUser(IServiceContext ctx, IEnumerable<long> ids)
        {
            var userEntity = (IEntity)this.DbDomain.GetResource("core.user");
            var constraints = new object[][] { new object[] { "login", "=", "root" } };
            var userIds = Search(userEntity, constraints, null, 0, 0);
            var rootId = userIds[0];
            var result = new Dictionary<long, object>();
            foreach (var id in ids)
            {
                result[id] = new object[2] { rootId, "root" };
            }

            return result;
        }
    }



}
