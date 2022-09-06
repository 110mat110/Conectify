namespace Conectify.Shared.Library.ErrorHandling;
using System;

public class ConectifyException : Exception
{
    public ConectifyException(string message) : base(message)
    {
    }
}
