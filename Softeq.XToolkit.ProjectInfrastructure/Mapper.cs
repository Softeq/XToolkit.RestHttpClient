using System;
using System.Collections.Generic;
using System.Linq;

namespace Softeq.XToolkit.CrossCutting
{
    public class Mapper
    {
        private readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> _fromTypeToTypeMappings;

        public Mapper()
        {
            _fromTypeToTypeMappings = new Dictionary<Type, Dictionary<Type, Func<object, object>>>();
        }

        public TTarget Map<TTarget>(object source)
        {
            if (source == null)
            {
                return default(TTarget);
            }

            var typeFrom = source.GetType();
            var typeTo = typeof(TTarget);

            EnsureMappingExists(typeFrom, typeTo);

            return (TTarget)_fromTypeToTypeMappings[typeFrom][typeTo].Invoke(source);
        }

        public IReadOnlyList<TTarget> MapCollection<TSource, TTarget>(IEnumerable<TSource> source)
        {
            if (source == null)
            {
                return Enumerable.Empty<TTarget>().ToList();
            }

            var typeFrom = typeof(TSource);
            var typeTo = typeof(TTarget);

            EnsureMappingExists(typeFrom, typeTo);

            return source.Select(item => (TTarget)_fromTypeToTypeMappings[typeFrom][typeTo].Invoke(item)).ToList();
        }

        public void RegisterMapping<TFrom, TTo>(Func<TFrom, TTo> mapper)
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

            _fromTypeToTypeMappings[fromType][toType] = data => mapper.Invoke((TFrom)data);
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
