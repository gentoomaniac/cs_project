using System;
namespace Exceptions
{
    class IllegalOpcodeException: Exception { public IllegalOpcodeException(string message): base(message) { } }
}