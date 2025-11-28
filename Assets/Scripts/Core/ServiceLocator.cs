using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Registro simples de serviços (IoC leve).
    /// Exemplo:
    ///   ServiceLocator.Register<ISolubilityService>(solubilityService);
    ///   var svc = ServiceLocator.Resolve<ISolubilityService>();
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services =
            new Dictionary<Type, object>();

        public static void Register<T>(T instance)
        {
            var type = typeof(T);

            if (instance == null)
            {
                Debug.LogError($"Tentando registrar serviço nulo para o tipo {type.Name}.");
                return;
            }

            if (_services.ContainsKey(type))
            {
                _services[type] = instance;
            }
            else
            {
                _services.Add(type, instance);
            }
        }

        public static T Resolve<T>()
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var instance))
            {
                return (T)instance;
            }

            Debug.LogError($"Serviço do tipo {type.Name} não encontrado no ServiceLocator.");
            return default;
        }

        public static void Clear()
        {
            _services.Clear();
        }
    }
}
