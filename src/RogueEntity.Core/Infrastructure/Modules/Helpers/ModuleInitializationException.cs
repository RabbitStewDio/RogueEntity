using System;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Infrastructure.Modules.Helpers
{
    public class ModuleInitializationException: Exception
    {
        public ModuleInitializationException()
        {
        }

        protected ModuleInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ModuleInitializationException(string message) : base(message)
        {
        }

        public ModuleInitializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}