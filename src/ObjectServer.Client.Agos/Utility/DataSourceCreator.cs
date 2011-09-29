using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;


/*
Blog entry:
http://blog.bodurov.com/How-to-Bind-Silverlight-DataGrid-From-IEnumerable-of-IDictionary
*/

namespace ObjectServer.Client.Agos
{
    public static class DataSourceCreator
    {
        private static readonly Regex PropertNameRegex =
                new Regex(@"^[A-Za-z_]+[A-Za-z0-9_]*$", RegexOptions.Singleline);

        private static readonly Dictionary<string, Type> _typeBySigniture =
                new Dictionary<string, Type>();

        public static IEnumerable ToDataSource(this IEnumerable<IDictionary> list)
        {
            IDictionary firstDict = null;
            var hasData = false;
            foreach (var currentDict in list)
            {
                hasData = true;
                firstDict = currentDict;
                break;
            }
            if (!hasData)
            {
                return new object[] { };
            }
            if (firstDict == null)
            {
                throw new ArgumentException("IDictionary entry cannot be null");
            }

            var typeSigniture = GetTypeSigniture(firstDict);
            var objectType = GetTypeByTypeSigniture(typeSigniture);

            if (objectType == null)
            {
                var tb = GetTypeBuilder(typeSigniture);

                var constructor = tb.DefineDefaultConstructor(MethodAttributes.Public |
                                        MethodAttributes.HideBySig |
                                        MethodAttributes.SpecialName |
                                        MethodAttributes.RTSpecialName);

                var onExecMethod = AddPropertyChangedEvent(tb);

                foreach (DictionaryEntry pair in firstDict)
                {
                    if (PropertNameRegex.IsMatch(Convert.ToString(pair.Key), 0))
                    {
                        CreateProperty(tb, Convert.ToString(pair.Key), GetValueType(pair.Value), onExecMethod);
                    }
                    else
                    {
                        throw new ArgumentException(
                                    @"Each key of IDictionary must be 
                                alphanumeric and start with character.");
                    }
                }
                objectType = tb.CreateType();

                _typeBySigniture.Add(typeSigniture, objectType);
            }

            return GenerateEnumerable(objectType, list, firstDict);
        }

        private static MethodBuilder AddPropertyChangedEvent(TypeBuilder tb)
        {
            var eventHandlerType = typeof(PropertyChangedEventHandler);
            var eventArgsConstrInfo = typeof(PropertyChangedEventArgs).GetConstructor(new[] { typeof(string) });
            var invokeDelegate = typeof(PropertyChangedEventHandler).GetMethod("Invoke");
            const string eventName = "PropertyChanged";
            const MethodAttributes eventMethodAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.SpecialName;
            const MethodImplAttributes eventMethodImpAtr = MethodImplAttributes.Managed | MethodImplAttributes.Synchronized;

            var fieldBuilder = tb.DefineField(eventName, eventHandlerType, FieldAttributes.Private);

            var eventBuilder = tb.DefineEvent(eventName, EventAttributes.None, eventHandlerType);

            var addMethodBuilder = tb.DefineMethod("add_" + eventName, eventMethodAttr, null, new[] { eventHandlerType });
            addMethodBuilder.SetImplementationFlags(eventMethodImpAtr);
            addMethodBuilder.DefineParameter(1, ParameterAttributes.None, "value");

            var combineInfo = typeof(Delegate).GetMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) });

            var addIL = addMethodBuilder.GetILGenerator();
            addIL.Emit(OpCodes.Ldarg_0);
            addIL.Emit(OpCodes.Ldarg_0);
            addIL.Emit(OpCodes.Ldfld, fieldBuilder);
            addIL.Emit(OpCodes.Ldarg_1);
            addIL.Emit(OpCodes.Call, combineInfo);
            addIL.Emit(OpCodes.Castclass, eventHandlerType);
            addIL.Emit(OpCodes.Stfld, fieldBuilder);
            addIL.Emit(OpCodes.Ret);

            var removeMethodBuilder = tb.DefineMethod("remove_" + eventName,
                eventMethodAttr, null, new[] { eventHandlerType });
            removeMethodBuilder.SetImplementationFlags(eventMethodImpAtr);
            removeMethodBuilder.DefineParameter(1, ParameterAttributes.None, "value");

            var removeInfo = typeof(Delegate).GetMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) });

            var remIL = removeMethodBuilder.GetILGenerator();
            remIL.Emit(OpCodes.Ldarg_0);
            remIL.Emit(OpCodes.Ldarg_0);
            remIL.Emit(OpCodes.Ldfld, fieldBuilder);
            remIL.Emit(OpCodes.Ldarg_1);
            remIL.Emit(OpCodes.Call, removeInfo);
            remIL.Emit(OpCodes.Castclass, eventHandlerType);
            remIL.Emit(OpCodes.Stfld, fieldBuilder);
            remIL.Emit(OpCodes.Ret);

            eventBuilder.SetAddOnMethod(addMethodBuilder);
            eventBuilder.SetRemoveOnMethod(removeMethodBuilder);

            var onExecute = tb.DefineMethod("On" + eventName,
                                        MethodAttributes.Private | MethodAttributes.HideBySig,
                                        null, new[] { typeof(string) });
            onExecute.DefineParameter(1, ParameterAttributes.None, "propName");

            var exeIL = onExecute.GetILGenerator();
            var lblDelegateOk = exeIL.DefineLabel();
            exeIL.DeclareLocal(typeof(PropertyChangedEventHandler));
            exeIL.Emit(OpCodes.Nop);
            exeIL.Emit(OpCodes.Ldarg_0);
            exeIL.Emit(OpCodes.Ldfld, fieldBuilder);
            exeIL.Emit(OpCodes.Stloc_0);
            exeIL.Emit(OpCodes.Ldloc_0);
            exeIL.Emit(OpCodes.Ldnull);
            exeIL.Emit(OpCodes.Ceq);
            exeIL.Emit(OpCodes.Brtrue, lblDelegateOk);
            exeIL.Emit(OpCodes.Ldloc_0);
            exeIL.Emit(OpCodes.Ldarg_0);
            exeIL.Emit(OpCodes.Ldarg_1);
            exeIL.Emit(OpCodes.Newobj, eventArgsConstrInfo);
            exeIL.Emit(OpCodes.Callvirt, invokeDelegate);
            exeIL.MarkLabel(lblDelegateOk);
            exeIL.Emit(OpCodes.Ret);

            return onExecute;
        }

        private static Type GetTypeByTypeSigniture(string typeSigniture)
        {
            Type type;
            return _typeBySigniture.TryGetValue(typeSigniture, out type) ? type : null;
        }

        private static Type GetValueType(object value)
        {
            return value == null ? typeof(object) : value.GetType();
        }

        private static string GetTypeSigniture(IDictionary firstDict)
        {
            var sb = new StringBuilder();
            foreach (DictionaryEntry pair in firstDict)
            {
                sb.AppendFormat("_{0}_{1}", pair.Key, GetValueType(pair.Value));
            }
            return sb.ToString().GetHashCode().ToString().Replace("-", "Minus");
        }

        private static IEnumerable GenerateEnumerable(
                 Type objectType, IEnumerable<IDictionary> list, IDictionary firstDict)
        {
            var listType = typeof(List<>).MakeGenericType(new[] { objectType });
            var listOfCustom = Activator.CreateInstance(listType);

            foreach (var currentDict in list)
            {
                if (currentDict == null)
                {
                    throw new ArgumentException("IDictionary entry cannot be null");
                }
                var row = Activator.CreateInstance(objectType);
                foreach (DictionaryEntry pair in firstDict)
                {
                    if (currentDict.Contains(pair.Key))
                    {
                        var property = objectType.GetProperty(Convert.ToString(pair.Key));
                        property.SetValue(
                            row,
                            currentDict[pair.Key],
                            null);
                    }
                }
                listType.GetMethod("Add").Invoke(listOfCustom, new[] { row });
            }
            return listOfCustom as IEnumerable;
        }

        private static TypeBuilder GetTypeBuilder(string typeSigniture)
        {
            var an = new AssemblyName
            {
                Name = "TempAssembly" + typeSigniture,
                Version = new Version(1, 0, 0, 0)
            };
            var assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    an, AssemblyBuilderAccess.Run);

            var name = "TempAssembly" + typeSigniture + ".dll";
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(name);

            var tb = moduleBuilder.DefineType("com.bodurov.TempType" + typeSigniture
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit
                                , typeof(object), new[] { typeof(INotifyPropertyChanged) });
            return tb;
        }

        private static void CreateProperty(
                        TypeBuilder tb, string propertyName, Type propertyType, MethodInfo onPropertyChangedMethod)
        {
            var fieldBuilder = tb.DefineField("_" + propertyName,
                                                        propertyType,
                                                        FieldAttributes.Private);


            var propertyBuilder =
                tb.DefineProperty(
                    propertyName, PropertyAttributes.HasDefault, propertyType, null);

            var getPropMthdBldr =
                tb.DefineMethod("get_" + propertyName,
                    MethodAttributes.Public |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig,
                    propertyType, Type.EmptyTypes);

            var getIL = getPropMthdBldr.GetILGenerator();
            getIL.DeclareLocal(propertyType);
            var getEnd = getIL.DefineLabel();

            getIL.Emit(OpCodes.Nop);
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Stloc_0);
            getIL.Emit(OpCodes.Br_S, getEnd);

            getIL.MarkLabel(getEnd);
            getIL.Emit(OpCodes.Ldloc_0);
            getIL.Emit(OpCodes.Ret);

            var setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });
            setPropMthdBldr.DefineParameter(1, ParameterAttributes.None, "value");

            var setIL = setPropMthdBldr.GetILGenerator();

            var isNotEqual = setIL.DefineLabel();
            setIL.DeclareLocal(typeof(bool));
            setIL.Emit(OpCodes.Nop);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldfld, fieldBuilder);

            var inequality = propertyType.GetMethod("op_Inequality");
            if (inequality != null)
            {
                setIL.Emit(OpCodes.Call, inequality);
                setIL.Emit(OpCodes.Ldc_I4_0);
            }
            setIL.Emit(OpCodes.Ceq);
            setIL.Emit(OpCodes.Stloc_0);
            setIL.Emit(OpCodes.Ldloc_0);
            setIL.Emit(OpCodes.Brtrue_S, isNotEqual);

            setIL.Emit(OpCodes.Nop);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldstr, propertyName);
            setIL.Emit(OpCodes.Call, onPropertyChangedMethod);
            setIL.Emit(OpCodes.Nop);
            setIL.Emit(OpCodes.Nop);
            setIL.MarkLabel(isNotEqual);
            setIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }

}
