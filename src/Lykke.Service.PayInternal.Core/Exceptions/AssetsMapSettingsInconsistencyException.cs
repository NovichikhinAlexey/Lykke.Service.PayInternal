using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class AssetsMapSettingsInconsistencyException : Exception
    {
        public AssetsMapSettingsInconsistencyException()
        {
        }

        public AssetsMapSettingsInconsistencyException(IEnumerable<string> inconsistentValues) : base("Assets map contains the same value for multiple keys/assets")
        {
            InconsistentValues = inconsistentValues;
        }

        public AssetsMapSettingsInconsistencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AssetsMapSettingsInconsistencyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IEnumerable<string> InconsistentValues { get; set; }
    }
}
