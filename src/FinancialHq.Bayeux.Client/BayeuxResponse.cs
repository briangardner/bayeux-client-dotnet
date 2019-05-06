namespace FinancialHq.Bayeux.Client
{
#pragma warning disable 0649 // "Field is never assigned to". These fields will be assigned by JSON deserialization
    internal class BayeuxResponse
    {
        // ReSharper disable once InconsistentNaming
        public bool successful;
        // ReSharper disable once InconsistentNaming
        public string error;
    }
#pragma warning restore 0649
}
