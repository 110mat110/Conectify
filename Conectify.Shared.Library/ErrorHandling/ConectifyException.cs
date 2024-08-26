namespace Conectify.Shared.Library.ErrorHandling;
using System;

public class ConectifyException(string message) : Exception(message)
{
}
