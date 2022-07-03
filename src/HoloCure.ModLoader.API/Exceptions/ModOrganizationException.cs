using System;
using System.Collections.Generic;

namespace HoloCure.ModLoader.API.Exceptions
{
    internal class ModOrganizationException : Exception
    {
        public readonly ICollection<IMod> Errored;

        public ModOrganizationException(ICollection<IMod> errored, string? message = null) : base(message) {
            Errored = errored;
        }
    }
}