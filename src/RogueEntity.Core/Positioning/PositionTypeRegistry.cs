using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning;

public class PositionTypeRegistry
{
    static readonly Lazy<PositionTypeRegistry> instance = new Lazy<PositionTypeRegistry>();
    readonly Dictionary<Type, object> registeredTypes;

    public PositionTypeRegistry()
    {
        registeredTypes = new Dictionary<Type, object>();
    }

    public void Register<TPosition>(IPositionTypeRegistration<TPosition> reg)
        where TPosition : struct, IPosition<TPosition>
    {
        lock (registeredTypes)
        {
            registeredTypes[typeof(TPosition)] = reg;
        }
    }

    public static PositionTypeRegistry Instance => instance.Value;

    public bool TryGet<TPosition>([MaybeNullWhen(false)] out IPositionTypeRegistration<TPosition> p)
        where TPosition : struct, IPosition<TPosition>
    {
        lock (registeredTypes)
        {
            if (registeredTypes.TryGetValue(typeof(TPosition), out var raw))
            {
                p = (IPositionTypeRegistration<TPosition>)raw;
                return true;
            }
        }

        p = default;
        return false;
    }
}

public interface IPositionTypeRegistration<TPosition>
    where TPosition : struct, IPosition<TPosition>
{
    public TPosition Convert<TPositionIn>(TPositionIn p) 
        where TPositionIn : struct, IPosition<TPositionIn>;
}