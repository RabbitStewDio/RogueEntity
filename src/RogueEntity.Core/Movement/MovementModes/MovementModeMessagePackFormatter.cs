using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Movement.MovementModes
{
    public class MovementModeMessagePackFormatter : IMessagePackFormatter<IMovementMode>
    {
        readonly Dictionary<string, IMovementMode> modes;
        
        public MovementModeMessagePackFormatter(params IMovementMode[] movementModes)
        {
            this.modes = new Dictionary<string, IMovementMode>();
            foreach (var m in movementModes)
            {
                this.modes.Add(m.GetType().Name, m);
            }
        }

        public void Serialize(ref MessagePackWriter writer, IMovementMode value, MessagePackSerializerOptions options)
        {
            if (value != null)
            {
                writer.Write(true);
                writer.Write(value.GetType().Name);
            }
            else
            {
                writer.Write(false);
            }
        }

        public IMovementMode? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (!reader.ReadBoolean())
            {
                return null;
            }
            
            if (modes.TryGetValue(reader.ReadString(), out var result))
            {
                return result;
            }
            
            throw new MessagePackSerializationException();
        }
    }
}