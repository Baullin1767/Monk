using System;
using System.Collections.Generic;

namespace Monk.Common
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                return;
            }

            services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            if (services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }

            return null;
        }

        public static void Clear()
        {
            services.Clear();
        }
    }
}
