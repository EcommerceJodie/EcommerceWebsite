using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Dữ liệu không hợp lệ")
        {
            Errors = errors;
        }

        public IDictionary<string, string[]> Errors { get; }
    }
} 