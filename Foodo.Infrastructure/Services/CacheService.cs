using Foodo.Application.Abstraction.InfraRelated;
using ZiggyCreatures.Caching.Fusion;

namespace Foodo.Infrastructure.Services
{
	public class CacheService : ICacheService
	{
		private readonly IFusionCache _cache;

		public CacheService(IFusionCache cache)
		{
			_cache = cache;
		}

		public void Set (string key, object value)
		{
			_cache.Set(key, value);
		}

		public T Get<T>(string key)
		{
			return _cache.GetOrDefault<T>(key);
		}


		public void Remove(string key)
		{
			_cache.Remove(key);
		}

		public void RemoveByPrefix(string prefix)
		{
			_cache.RemoveByTag(prefix);
		}
	}
}
