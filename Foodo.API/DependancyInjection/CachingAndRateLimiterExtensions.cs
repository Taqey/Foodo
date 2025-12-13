using Foodo.Application.Abstraction.InfraRelated;
using Foodo.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.RateLimiting;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Foodo.API.Extensions
{
	public static class CachingAndRateLimiterExtensions
	{
		public static IServiceCollection AddCachingAndRateLimiter(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddFusionCache()
				.WithDefaultEntryOptions(options =>
				{
					options.DistributedCacheDuration = TimeSpan.FromDays(7);
					options.Duration = TimeSpan.FromHours(2);
					options.FailSafeMaxDuration = TimeSpan.FromMinutes(10);
					options.FailSafeThrottleDuration = TimeSpan.FromSeconds(30);
					options.EagerRefreshThreshold = 0.9f;
					options.AllowBackgroundBackplaneOperations = true;
					options.LockTimeout = TimeSpan.FromSeconds(3);
				})
				.WithDistributedCache(new RedisCache(new RedisCacheOptions { Configuration = configuration["Redis:ConnectionString"] }))
				.WithSerializer(new FusionCacheSystemTextJsonSerializer());

			services.AddSingleton<ICacheService, CacheService>();

			services.AddRateLimiter(options =>
			{
				options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

				options.AddFixedWindowLimiter("FixedWindowPolicy", opt =>
				{
					opt.Window = TimeSpan.FromMinutes(1);
					opt.PermitLimit = 100;
					opt.QueueLimit = 2;
					opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
				});

				options.AddSlidingWindowLimiter("SlidingWindowPolicy", opt =>
				{
					opt.Window = TimeSpan.FromMinutes(15);
					opt.PermitLimit = 5;
					opt.QueueLimit = 0;
					opt.SegmentsPerWindow = 3;
				});

				options.AddTokenBucketLimiter("TokenBucketPolicy", opt =>
				{
					opt.TokenLimit = 30;
					opt.TokensPerPeriod = 10;
					opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
					opt.QueueLimit = 0;
				});

				options.AddConcurrencyLimiter("LeakyBucketPolicy", opt =>
				{
					opt.PermitLimit = 1;
					opt.QueueLimit = 5;
					opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
				});
			});

			return services;
		}
	}
}
