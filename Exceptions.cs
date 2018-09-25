using System;
namespace Exceptions
{
    class IllegalOpcodeException: Exception { public IllegalOpcodeException(string message): base(message) { } }
    class OpcodeNotImplementedException: Exception { public OpcodeNotImplementedException(string message): base(message) { } }
}