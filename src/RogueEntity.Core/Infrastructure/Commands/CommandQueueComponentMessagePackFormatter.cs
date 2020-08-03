using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public class CommandQueueComponentMessagePackFormatter : IMessagePackFormatter<CommandQueueComponent>
    {
        public void Serialize(ref MessagePackWriter writer, CommandQueueComponent value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(value.Count);
            foreach (var a in value)
            {
                MessagePackSerializer.Serialize(ref writer, a, options);
            }
        }

        public CommandQueueComponent Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var elements = reader.ReadArrayHeader();
            var result = new CommandQueueComponent();
            for (int x = 0; x < elements; x += 1)
            {
                result.PerformLater(MessagePackSerializer.Deserialize<ICommand>(ref reader, options));
            }

            return result;
        }
    }
}