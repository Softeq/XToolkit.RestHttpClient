// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Softeq.HttpClient.Common
{
    public class Mapper
    {
        private readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> _fromTypeToTypeMappings;

        public Mapper()
        {
            _fromTypeToTypeMappings = new Dictionary<Type, Dictionary<Type, Func<object, object>>>();
        }

        public TTarget Map<TTarget>(object source) where TTarget : class
        {
            if (source == null)
            {
                return null;
            }

            var typeFrom = source.GetType();
            var typeTo = typeof(TTarget);

            EnsureMappingExists(typeFrom, typeTo);

            return _fromTypeToTypeMappings[typeFrom][typeTo].Invoke(source) as TTarget;
        }

        public IReadOnlyList<TTarget> MapCollection<TSource, TTarget>(IEnumerable<TSource> source)
            where TTarget : class
            where TSource : class
        {
            if (source == null)
            {
                return Enumerable.Empty<TTarget>().ToList();
            }

            var typeFrom = typeof(TSource);
            var typeTo = typeof(TTarget);

            EnsureMappingExists(typeFrom, typeTo);

            return source.Select(item => _fromTypeToTypeMappings[typeFrom][typeTo].Invoke(item) as TTarget).ToList();
        }

        public void RegisterMapping<TFrom, TTo>(Func<TFrom, TTo> mapper) where TFrom : class
        {
            var fromType = typeof(TFrom);
            var toType = typeof(TTo);

            if (!_fromTypeToTypeMappings.ContainsKey(fromType))
            {
                _fromTypeToTypeMappings.Add(fromType, new Dictionary<Type, Func<object, object>>());
            }

            if (!_fromTypeToTypeMappings[fromType].ContainsKey(toType))
            {
                _fromTypeToTypeMappings[fromType].Add(toType, null);
            }

            _fromTypeToTypeMappings[fromType][toType] = data => mapper.Invoke(data as TFrom);
        }

        private void EnsureMappingExists(Type typeFrom, Type typeTo)
        {
            if (!_fromTypeToTypeMappings.ContainsKey(typeFrom) || !_fromTypeToTypeMappings[typeFrom].ContainsKey(typeTo))
            {
                throw new ArgumentException(
                    $"Cannot map object. Mappings for {typeFrom.FullName} -> {typeTo.FullName} haven't been created.");
            }
        }
    }
}
