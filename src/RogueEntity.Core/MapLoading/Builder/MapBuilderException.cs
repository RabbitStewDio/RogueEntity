using System;
using System.Runtime.Serialization;

namespace RogueEntity.Core.MapLoading.Builder
{
    public class MapBuilderException: ApplicationException
    {
        public MapBuilderException()
        {
        }

        protected MapBuilderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MapBuilderException(string message) : base(message)
        {
        }

        public MapBuilderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
