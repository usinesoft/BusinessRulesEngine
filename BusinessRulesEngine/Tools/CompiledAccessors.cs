using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace BusinessRulesEngine.Tools
{
    /// <summary>
    ///     Caches a compiled version for the public accessors of a type
    ///     We have getters, setters and smart setters; the last one changes the value only if it os different and returns true
    ///     only if
    ///     the value was changed
    /// </summary>
    public static class CompiledAccessors
    {
        private static readonly Dictionary<string, Func<object, object>> _compiledGetters =
            new Dictionary<string, Func<object, object>>();

        private static readonly Dictionary<string, Action<object, object>> _compiledSetters =
            new Dictionary<string, Action<object, object>>();

        private static readonly Dictionary<string, Func<object, object, bool>> _compiledSmartSetters =
            new Dictionary<string, Func<object, object, bool>>();

        private static string MakeKey(Type declaringType, string propertyName)
        {
            return declaringType.FullName + "." + propertyName;
        }

        public static Func<object, object> CompiledGetter(Type declaringType, string propertyName)
        {
            var key = MakeKey(declaringType, propertyName);

            if (_compiledGetters.ContainsKey(key))
            {
                return _compiledGetters[key];
            }

            return _compiledGetters[key] = CompileGetter(declaringType.GetProperty(propertyName));
        }

        public static Action<object, object> CompiledSetter(Type declaringType, string propertyName)
        {
            var key = MakeKey(declaringType, propertyName);

            if (_compiledSetters.ContainsKey(key))
            {
                return _compiledSetters[key];
            }

            return _compiledSetters[key] = CompileSetter(declaringType.GetProperty(propertyName));
        }

        public static Func<object, object, bool> CompiledSmartSetter(Type declaringType, string propertyName)
        {
            var key = MakeKey(declaringType, propertyName);

            if (_compiledSmartSetters.ContainsKey(key))
            {
                return _compiledSmartSetters[key];
            }

            return _compiledSmartSetters[key] = CompileSmartSetter(declaringType.GetProperty(propertyName));
        }

        /// <summary>
        ///     Precompile a call to a property getter. It can be called to avoid reflexion base invokation
        /// </summary>
        /// <returns></returns>
        private static Func<object, object> CompileGetter(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof (object), "instance");

            Debug.Assert(propertyInfo.DeclaringType != null, "propertyInfo.DeclaringType != null");
            var instanceCast = propertyInfo.DeclaringType.IsValueType
                ? Expression.TypeAs(instance, propertyInfo.DeclaringType)
                : Expression.Convert(instance, propertyInfo.DeclaringType);

            return
                Expression.Lambda<Func<object, object>>(
                    Expression.TypeAs(Expression.Call(instanceCast, propertyInfo.GetGetMethod()), typeof (object)),
                    instance)
                    .Compile();
        }

        /// <summary>
        ///     Precompile a call to a preperty setter. It can be called to avoid reflexion based invokation
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        private static Action<object, object> CompileSetter(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof (object), "instance");

            var value = Expression.Parameter(typeof (object), "value");

            var instanceType = propertyInfo.DeclaringType;
            var propertyType = propertyInfo.PropertyType;

            Debug.Assert(instanceType != null, "instanceType != null");
            var instanceCast = Expression.Convert(instance, instanceType);

            var valueCast = Expression.Convert(value, propertyType);

            return
                Expression.Lambda<Action<object, object>>(
                    Expression.Call(instanceCast, propertyInfo.GetSetMethod(), valueCast), instance, value)
                    .Compile();
        }

        /// <summary>
        ///     Smart setter that changes a value only if it is different from the current one
        /// </summary>
        /// <returns>true if value changed, false if same</returns>
        private static Func<object, object, bool> CompileSmartSetter(PropertyInfo propertyInfo)
        {
            var getter = CompiledGetter(propertyInfo.DeclaringType, propertyInfo.Name);
            var setter = CompiledSetter(propertyInfo.DeclaringType, propertyInfo.Name);

            return (parent, val) =>
            {
                var previous = getter(parent);
                if (!Equals(previous, val))
                {
                    setter(parent, val);
                    return true;
                }

                return false;
            };
        }
    }
}