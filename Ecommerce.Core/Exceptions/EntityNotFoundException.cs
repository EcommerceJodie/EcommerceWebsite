using System;

namespace Ecommerce.Core.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, Guid id)
            : base($"Không tìm thấy {entityName} với ID {id}")
        {
            EntityName = entityName;
            EntityId = id;
        }

        public EntityNotFoundException(string entityName, string identifier, string value)
            : base($"Không tìm thấy {entityName} với {identifier} là {value}")
        {
            EntityName = entityName;
            Identifier = identifier;
            IdentifierValue = value;
        }

        public string EntityName { get; }
        public Guid? EntityId { get; }
        public string Identifier { get; }
        public string IdentifierValue { get; }
    }
} 
