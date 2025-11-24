using Foodo.Application.Abstraction;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodo.Infrastructure.Services
{
	public class CacheService : ICacheService
	{
		private readonly IMemoryCache _cache;
		private static readonly HashSet<string> _keys = new();

		public CacheService(IMemoryCache cache)
		{
			_cache = cache;
		}

		public void Set(string key, object value)
		{
			var cacheOptions = new MemoryCacheEntryOptions()
	.SetSlidingExpiration(TimeSpan.FromMinutes(30))
	.SetAbsoluteExpiration(TimeSpan.FromHours(2));
			_cache.Set(key, value, cacheOptions);
			_keys.Add(key);
		}

		public T Get<T>(string key)
		{
			return _cache.TryGetValue(key, out T value) ? value : default;
		}

		public void Remove(string key)
		{
			_cache.Remove(key);
			_keys.Remove(key);
		}

		public void RemoveByPrefix(string prefix)
		{
			var keysToRemove = _keys.Where(k => k.StartsWith(prefix)).ToList();

			foreach (var key in keysToRemove)
			{
				_cache.Remove(key);
				_keys.Remove(key);
			}
		}
	}

}
