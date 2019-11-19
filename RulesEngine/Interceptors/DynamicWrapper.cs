using System;
using System.ComponentModel;
using System.Dynamic;
using RulesEngine.RulesEngine;
using RulesEngine.Tools;

namespace RulesEngine.Interceptors
{
    /// <summary>
    ///     Wraps business object as a <see cref="DynamicObject" />. It can be used tu intercept accessor calls on types that
    ///     can not be wrapped in typed proxies (for example sealed classes)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DynamicWrapper<T> : DynamicObject, INotifyPropertyChanged
    {
        /// <summary>
        ///     The rules engine
        /// </summary>
        private readonly MappingRules<T> _businessRules;

        private readonly object _root;
        private readonly object _wrappedObject;


        internal DynamicWrapper(object root, object obj, MappingRules<T> businessRules)
        {
            _root = root;
            _wrappedObject = obj ?? throw new ArgumentNullException(nameof(obj));
            _businessRules = businessRules;
        }

        
        public DynamicWrapper(object root, MappingRules<T> businessRules)
        {
            
            _wrappedObject = root ?? throw new ArgumentNullException(nameof(root));
            _root = root;
            _businessRules = businessRules;
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Overrides of DynamicObject

        /// <summary>
        ///     For the moment let the method calls pass through.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            try
            {
                //call _wrappedObject object
                result = _wrappedObject.GetType()
                    .GetMethod(binder.Name)
                    ?.Invoke(_wrappedObject, args);

                return true;
            }

            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Intercept calls to the property setters and divert them to the rules engine
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            try
            {
                var modified = _businessRules.SetProperty(binder.Name, _root, _wrappedObject, value);

                foreach (var property in modified) OnPropertyChanged(property);

                return true;
            }
            catch
            {
                return false;
            }
        }


        private static bool IsComplexType(Type type)
        {
            if (type.IsValueType) return false;

            if (type == typeof(string)) return false;

            return type.IsClass;
        }

        /// <summary>
        ///     Intercept calls to property getters. Let the invocation being done normally for now
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                var getter = CompiledAccessors.CompiledGetter(_wrappedObject.GetType(), binder.Name);
                var getterResult = getter(_wrappedObject);

                result = IsComplexType(getterResult.GetType())
                    ? new DynamicWrapper<T>(_root, getterResult, _businessRules)
                    : getterResult;

                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        #endregion
    }
}