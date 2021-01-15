using EnTTSharp.Entities;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Infrastructure.Actions.Schedule
{
    public class ScheduledActionPlanMessageFormatter<TActorId> : IMessagePackFormatter<ScheduledActionPlan<TActorId>>
        where TActorId : IEntityKey
    {
        public void Serialize(ref MessagePackWriter writer, ScheduledActionPlan<TActorId> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(value.Count);
            foreach (var a in value)
            {
                MessagePackSerializer.Serialize(ref writer, a, options);
            }
        }

        public ScheduledActionPlan<TActorId> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var elements = reader.ReadArrayHeader();
            var result = new ScheduledActionPlan<TActorId>();
            for (int x = 0; x < elements; x += 1)
            {
                result.Add(MessagePackSerializer.Deserialize<ScheduledAction<TActorId>>(ref reader, options));
            }

            return result;
        }
    }
}
