using Conectify.Database.Interfaces;

namespace Conectify.Service.History.Models
{
    public class CacheItem<T> : List<T> where T : class
    {
        public CacheItem()
        {
            CreationTimeUtc = DateTime.UtcNow;
        }

        public CacheItem(T value)
        {
            CreationTimeUtc = DateTime.UtcNow;
            this.Add(value);
        }

        public DateTime CreationTimeUtc { get; }

        public void Reorder<TKey>(Func<T, TKey> orderSelector)
        {
            var orderedItems = this.OrderBy(orderSelector);
            this.Clear();
            this.AddRange(orderedItems);
        }
    }
}
