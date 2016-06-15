using System.ComponentModel;
using BusinessRulesEngine.RulesEngine;
using Castle.DynamicProxy;

namespace BusinessRulesEngine.Interceptors
{
    /// <summary>
    ///     Intercept calls to non sealed classes that implement interfaces
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
            Target = Generator.CreateInterfaceProxyWithTarget(instance, new Interceptor(rules, this));
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
            private readonly MappingRules<T> _rules;

            public Interceptor(MappingRules<T> rules, InterfaceWrapper<T> parent)
            {
                _rules = rules;
                _parent = parent;
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
                            new Interceptor(_rules, _parent));

                        invocation.ReturnValue = proxy;
                    }
                }
                else if (methodName.StartsWith("set_"))
                {
                    var propertyName = methodName.Substring(4);

                    var modified = _rules.SetProperty(propertyName, invocation.InvocationTarget, invocation.Arguments[0]);

                    foreach (var property in modified)
                    {
                        _parent.OnPropertyChanged(property);
                    }
                }
            }
        }
    }
}