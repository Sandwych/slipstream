using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Threading;

namespace ObjectServer.Utility
{

    // This is to acheive Dynamic method invocation without the TargetInvocationException wrapping
    // Source http://timwi.blogspot.com/2010/03/dynamic-method-invocation-without.html

    public interface IInvokeDirect
    {
        object DoInvoke(object instance, object[] parameters, MethodInfo method);
    }

    public static class DirectInvoker
    {
        private static AssemblyBuilder asmBuilder;
        private static ModuleBuilder modBuilder;
        private static Dictionary<string, IInvokeDirect> invokers = new Dictionary<string, IInvokeDirect>();

        public static object InvokeDirect(MethodInfo method, object instance, params object[] parameters)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            if (!method.IsStatic && instance == null)
                throw new ArgumentException("Non-static method requires a non-null instance.");
            if (method.IsStatic && instance != null)
                throw new ArgumentException("Static method requires a null instance.");
            if (method.IsGenericMethodDefinition)
                throw new ArgumentException("Cannot invoke generic method definitions. Concretise the generic type first by supplying generic type parameters.");
            if (method.GetParameters().Length != parameters.Length)
                throw new ArgumentException(string.Format("Parameter count mismatch: {0} parameters given; {1} parameters expected.", parameters.Length, method.GetParameters().Length));

            // Create a dynamic assembly for the two types we need
            createAssembly();

            // Generate a string that identifies the method "signature" (the parameter types and return type, not the method name).
            // Different methods which have the same "signature" according to this criterion can re-use the same generated code.
            var sig = string.Join(" : ", method.GetParameters().Select(p => p.ParameterType.FullName).Concat(new[] { method.ReturnType.FullName }).ToArray());

            if (!invokers.ContainsKey(sig))
            {
                // Create a delegate type compatible with the method signature
                var delegateType = createDelegateType(method, sig);

                // Create a class that implements IInvokeDirect
                var classType = createClassType(method, sig, delegateType);

                // Instantiate the new class and remember the instance for additional future calls
                invokers[sig] = (IInvokeDirect)Activator.CreateInstance(classType);
            }

            // Invoke the interface method, passing it the necessary information to call the right target method.
            return invokers[sig].DoInvoke(instance, parameters, method);
        }

        private static Type createClassType(MethodInfo method, string sig, Type delegateType)
        {
            var typeBuilder = modBuilder.DefineType("invoke " + sig + ".GeneratedClass", TypeAttributes.Public,
                typeof(object), new Type[] { typeof(IInvokeDirect) });

            // Create a DoInvoke method that implements IInvokeDirect.DoInvoke
            var methodBuilder = typeBuilder.DefineMethod("DoInvoke", MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(object), new Type[] { typeof(object), typeof(object[]), typeof(MethodInfo) });
            typeBuilder.DefineMethodOverride(methodBuilder, typeof(IInvokeDirect).GetMethod("DoInvoke"));

            // Now generate IL which will create a delegate and invoke it.
            // The IL we will generate is basically equivalent to the following single line of C# code:
            // return ((GeneratedDelegate) Delegate.CreateDelegate(typeof(GeneratedDelegate), instance, method))((ParamType1) parameters[0], (ParamType2) parameters[1], ...);
            var il = methodBuilder.GetILGenerator();

            // We're going to call Delegate.CreateDelegate() with the following three parameters:
            // PARAMETER 1: The generated delegate type
            il.Emit(OpCodes.Ldtoken, delegateType.MetadataToken);
            il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));

            // PARAMETER 2: The instance on which we want to call a method
            il.Emit(OpCodes.Ldarg_1);

            // PARAMETER 3: The method we want to call
            il.Emit(OpCodes.Ldarg_3);

            // Call Delegate.CreateDelegate() with the above three parameters
            il.Emit(OpCodes.Call, typeof(Delegate).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object), typeof(MethodInfo) }));

            // The above method returns a "Delegate". We need to cast that to the real delegate type in order to call its actual Invoke method
            il.Emit(OpCodes.Castclass, delegateType.MetadataToken);

            // Feed the evaluation stack with the parameters to the delegate Invoke call
            var methodParameters = method.GetParameters();
            for (int paramIndex = 0; paramIndex < methodParameters.Length; paramIndex++)
            {
                // This is the IL equivalent for "parameters[paramIndex]", where "parameters" refers to the object[] parameter on DoInvoke
                il.Emit(OpCodes.Ldarg_2);
                EmitLdcI4(il, paramIndex);
                il.Emit(OpCodes.Ldelem_Ref);
                // Since "parameters" is object[], but the delegate wants the "real" type, cast it
                EmitObjectCast(il, methodParameters[paramIndex].ParameterType);
            }

            // Finally, call the Invoke method on the delegate. This will push the real returned object (if any) onto the stack
            il.Emit(OpCodes.Callvirt, delegateType.GetMethod("Invoke").MetadataToken);

            // Since DoInvoke returns an object, we need to return null if the Invoke method didn't return anything,
            // or box the returned object if the Invoke method returned a value type
            if (method.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else if (method.ReturnType.IsValueType)
                il.Emit(OpCodes.Box, method.ReturnType);

            // Return
            il.Emit(OpCodes.Ret);

            return typeBuilder.CreateType();
        }

        private static Type createDelegateType(MethodInfo method, string sig)
        {
            var delegateBuilder = modBuilder.DefineType("invoke " + sig + ".GeneratedDelegate",
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(MulticastDelegate));
            var delegateMethodBuilder = delegateBuilder.DefineMethod("Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray());
            delegateMethodBuilder.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
            var delegateConstructor = delegateBuilder.DefineConstructor(
                MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard, Type.EmptyTypes);
            delegateConstructor.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
            return delegateBuilder.CreateType();
        }

        private static void createAssembly()
        {
            if (asmBuilder == null)
            {
                asmBuilder = Thread.GetDomain().DefineDynamicAssembly(
                    new AssemblyName("direct_invoker_assembly"), AssemblyBuilderAccess.Run);
                modBuilder = asmBuilder.DefineDynamicModule("direct_invoker_module");
            }
        }

        private static void EmitLdcI4(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    else
                        il.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        private static void EmitObjectCast(ILGenerator il, Type toType)
        {
            if (toType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, toType);
            else
                il.Emit(OpCodes.Castclass, toType);
        }
    }
}
