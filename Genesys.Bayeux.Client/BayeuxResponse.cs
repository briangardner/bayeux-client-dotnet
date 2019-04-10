using System;
using System.Collections.Generic;
using System.Text;

namespace Genesys.Bayeux.Client
{
#pragma warning disable 0649 // "Field is never assigned to". These fields will be assigned by JSON deserialization
    internal class BayeuxResponse
    {
        public bool successful;
        public string error;
    }
#pragma warning restore 0649
}
