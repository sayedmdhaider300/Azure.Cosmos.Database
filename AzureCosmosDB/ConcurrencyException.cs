using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.CosmosDB
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException()
        {

        }

        public ConcurrencyException(string message) : base(message)
        {

        }

        public ConcurrencyException(string message, Exception exception) : base(message, exception)
        {

        }
    }
}
