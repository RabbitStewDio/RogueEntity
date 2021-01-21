using MessagePack;
using MessagePack.Formatters;
using System;

namespace RogueEntity.Api.Utils
{
    public class EitherResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new EitherResolver();

        EitherResolver()
        {
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> Formatter;

            // generic's static constructor should be minimized for reduce type generation size!
            // use outer helper method.
            static FormatterCache()
            {
                if (typeof(T).GetGenericTypeDefinition() != typeof(Either<,>))
                {
                    Formatter = null;
                    return;
                }

                var args = typeof(T).GetGenericArguments();
                var formatterType = typeof(EitherMessagePackFormatter<,>).MakeGenericType(args);
                Formatter = (IMessagePackFormatter<T>)Activator.CreateInstance(formatterType);
            }
        }
    }
}
