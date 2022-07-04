using System;

namespace HoloCure.ModLoader.API.Exceptions
{
    public class ModLoadException : Exception
    {
        public ModLoadException(string? message, Exception? innerException = null) : base(message, innerException) { }
    }
}