using System.Collections.Concurrent;

namespace smart_home_server.Mqtt.Authentication
{
    public class MqttAuthTypeActivatorCache : IMqttAuthTypeActivatorCache
    {
        private readonly Func<Type, ObjectFactory> _createFactory = (type) => ActivatorUtilities.CreateFactory(type, Type.EmptyTypes);

        private readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache = new ConcurrentDictionary<Type, ObjectFactory>();

        /// <inheritdoc/>
        public TInstance CreateInstance<TInstance>(IServiceProvider serviceProvider, Type implementationType)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            var createFactory = _typeActivatorCache.GetOrAdd(implementationType, _createFactory);

            return (TInstance)createFactory(serviceProvider, arguments: null);
        }
    }
}