using System.Collections.Generic;

namespace Mobile.Secrets.SecretCollectorStrategies
{
    public interface ICollectorStrategy<TValue> : ICollectorStrategy
    {
        public TValue CollectSecrets();

        public Dictionary<string, object> MapSecrets(TValue collectedSecrets);
    }

    public interface ICollectorStrategy
    {
        public Dictionary<string, object> CollectAndMapSecrets();
    }
}