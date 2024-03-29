using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Api.Utils
{
    public class EitherMessagePackFormatter<TLeft, TRight>: IMessagePackFormatter<Either<TLeft, TRight>>
    {
        public void Serialize(ref MessagePackWriter writer, Either<TLeft, TRight> value, MessagePackSerializerOptions options)
        {
            writer.Write(value.HasLeft);
            writer.Write(value.HasRight);
            if (value.HasLeft)
            {
                MessagePackSerializer.Serialize(ref writer, value.Left, options);
            }
            
            if (value.HasRight)
            {
                MessagePackSerializer.Serialize(ref writer, value.Right, options);
            }
        }

        public Either<TLeft, TRight> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var hasLeft = reader.ReadBoolean();
            var hasRight = reader.ReadBoolean();
            if (hasLeft)
            {
                var left = MessagePackSerializer.Deserialize<TLeft>(ref reader, options);
                return new Either<TLeft, TRight>(left);
            }

            if (hasRight)
            {
                var right = MessagePackSerializer.Deserialize<TRight>(ref reader, options);
                return new Either<TLeft, TRight>(right);
            }
            
            return default;
        }
    }
}
