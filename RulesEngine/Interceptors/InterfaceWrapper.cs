using System.ComponentModel;
using Castle.DynamicProxy;
using RulesEngine.RulesEngine;

namespace RulesEngine.Interceptors
{
    /// <summary>
    ///     Intercept calls to non sealed classes that have all public properties virtual
    ///     Calls to setters are diverted to rule engine
    ///     Calls to getters are intercepted to wrap the returned value (if it is an interface)
    ///     Calls to methods are silently ignored
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class InterfaceWrapper<T> : INotifyPropertyChanged
        where T : class
    {
        // ReSharper disable once StaticMemberInGenericType
        private static ProxyGenerator Generator { get; } = new ProxyGenerator();

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public InterfaceWrapper(T instance, MappingRules<T> rules)
        {
            Target = typeof(T).IsInterface ? Generator.CreateInterfaceProxyWithTarget(instance, new Interceptor(rules, this, instance)) : Generator.CreateClassProxyWithTarget(instance, new Interceptor(rules, this, instance));
        }

        public T Target { get; }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class Interceptor : IInterceptor
        {
            private readonly InterfaceWrapper<T> _parent;
            private readonly T _instance;
            private readonly MappingRules<T> _rules;

            public Interceptor(MappingRules<T> rules, InterfaceWrapper<T> parent, T instance)
            {
                _rules = rules;
                _parent = parent;
                _instance = instance;
            }

            public void Intercept(IInvocation invocation)
            {
                var methodName = invocation.Method.Name;
                if (methodName.StartsWith("get_"))
                {
                    invocation.Proceed();

                    var getterReturnType = invocation.MethodInvocationTarget.ReturnType;

                    if (getterReturnType.IsInterface)
                    {
                        // wrap the result of the getter in a proxy
                        var proxy = Generator.CreateInterfaceProxyWithTarget(getterReturnType, invocation.ReturnValue,
                            new Interceptor(_rules, _parent, _instance));

                        invocation.ReturnValue = proxy;
                    }
                    else if(getterReturnType.IsClass && !getterReturnType.Namespace.StartsWith("System")) // avoid elementary types that are class
                    {
                        var proxy = Generator.CreateClassProxyWithTarget(getterReturnType, invocation.ReturnValue,
                            new Interceptor(_rules, _parent, _instance));

                        invocation.ReturnValue = proxy;
                    }
                }
                else if (methodName.StartsWith("set_"))
                {
                    var propertyName = methodName.Substring(4);

                    var modified = _rules.SetProperty(propertyName, _instance, invocation.InvocationTarget, invocation.Arguments[0]);

                    foreach (var property in modified)
                    {
                        _parent.OnPropertyChanged(property);
                    }
                }
            }
        }
    }
}