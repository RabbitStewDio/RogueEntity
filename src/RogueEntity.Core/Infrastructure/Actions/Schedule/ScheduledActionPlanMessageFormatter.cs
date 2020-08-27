using EnTTSharp.Entities;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Infrastructure.Actions
{
    public class ScheduledActionPlanMessageFormatter<TContext, TActorId>: IMessagePackFormatter<ScheduledActionPlan<TContext, TActorId>> 
        where TActorId : IEntityKey
    {
        public void Serialize(ref MessagePackWriter writer, ScheduledActionPlan<TContext, TActorId> value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(value.Count);
            foreach (var a in value)
            {
                MessagePackSerializer.Serialize(ref writer, a, options);
            }
        }

        public ScheduledActionPlan<TContext, TActorId> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var elements = reader.ReadArrayHeader();
            var result = new ScheduledActionPlan<TContext, TActorId>();
            for (int x = 0; x < elements; x += 1)
            {
                result.Add(MessagePackSerializer.Deserialize<ScheduledAction<TContext, TActorId>>(ref reader, options));
            }

            return result;
        }
    }
}