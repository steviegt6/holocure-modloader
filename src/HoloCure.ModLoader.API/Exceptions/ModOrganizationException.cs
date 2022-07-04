using System;
using System.Collections.Generic;

namespace HoloCure.ModLoader.API.Exceptions
{
    internal class ModOrganizationException : Exception
    {
        public readonly ICollection<ModMetadata> Errored;

        public ModOrganizationException(ICollection<ModMetadata> errored, string? message = null) : base(message) {
            Errored = errored;
        }
    }
}