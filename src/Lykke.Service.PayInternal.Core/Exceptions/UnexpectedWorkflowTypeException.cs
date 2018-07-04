using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class UnexpectedWorkflowTypeException : Exception
    {
        public UnexpectedWorkflowTypeException()
        {
        }

        public UnexpectedWorkflowTypeException(WorkflowType workflowType) : base("Unexpected workflow type value")
        {
            WorkflowType = workflowType;
        }

        public UnexpectedWorkflowTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnexpectedWorkflowTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public WorkflowType WorkflowType { get; set; }
    }
}
