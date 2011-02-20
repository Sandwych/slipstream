﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Transactions;
using System.Data;

using log4net;

using ObjectServer.Backend;
using ObjectServer.Model;
using ObjectServer.Utility;

namespace ObjectServer
{

    public sealed class ObjectPool
    {
        private static readonly ILog Log = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, IServiceObject> objects =
            new Dictionary<string, IServiceObject>();

        public ObjectPool(IDatabase db, string dbName)
        {
            this.Database = dbName;
            this.LoadAllObjects(db);

            this.InitializeAllObjects(db);
        }

        private void LoadAllObjects(IDatabase db)
        {
            this.RegisterAllCoreObjects();

            Module.UpdateModuleList(db);
            Module.LoadModules(db, this);
        }

        private void InitializeAllObjects(IDatabase db)
        {
            //一次性初始化所有对象
            //obj.Initialize(db, pool);
            //TODO: 初始化非 IModel 对象
            var models =
                (from o in this.objects.Values
                 where o is IModel
                 select o as IModel).ToArray();

            var sortedModels =
                DependencySorter<IModel, string>.DependencySort(
                    models,
                    m => m.Name,
                    m => m.GetDependedModels());

            foreach (var m in sortedModels)
            {
                m.Initialize(db, this);
            }
        }

        public string Database { get; private set; }

        private void RegisterAllCoreObjects()
        {
            var a = typeof(ObjectPool).Assembly;
            this.RegisterModelsInAssembly(a);
        }

        public void RegisterModelsInAssembly(Assembly assembly)
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Start to register all models for assembly [{0}]...", assembly.FullName);
            }

            var types = Model.TableModel.GetModelsFromAssembly(assembly);

            using (var db = Backend.DataProvider.OpenDatabase(this.Database))
            {
                db.Open();
                foreach (var t in types)
                {
                    var obj = CreateServiceObject(db, this, t);
                    this.objects[obj.Name] = obj;
                }
            }

            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Done");
            }
        }

        public IServiceObject LookupObject(string name)
        {
            return this.objects[name];
        }


        private static IServiceObject CreateServiceObject(IDatabase db, ObjectPool pool, Type t)
        {
            var obj = Activator.CreateInstance(t) as IServiceObject;
            if (obj == null)
            {
                var msg = string.Format("类型 '{0}' 没有实现 IServiceObject 接口", t.FullName);
                throw new InvalidCastException(msg);
            }
            return obj;
        }
    }
}
