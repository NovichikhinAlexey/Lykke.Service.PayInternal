using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class SupervisorMembershipNotFoundException : Exception
    {
        public SupervisorMembershipNotFoundException()
        {
        }

        public SupervisorMembershipNotFoundException(string employeeId) : base("Supervisor membership not found")
        {
            EmployeeId = employeeId;
        }

        public SupervisorMembershipNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SupervisorMembershipNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        public string EmployeeId { get; set; }
    }
}
