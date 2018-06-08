using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class SupervisorMembershipAlreadyExistsException : Exception
    {
        public SupervisorMembershipAlreadyExistsException()
        {
        }

        public SupervisorMembershipAlreadyExistsException(string employeeId) : base("Supervisor membership already exists")
        {
            EmployeeId = employeeId;
        }

        public SupervisorMembershipAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SupervisorMembershipAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string EmployeeId { get; set; }
    }
}
