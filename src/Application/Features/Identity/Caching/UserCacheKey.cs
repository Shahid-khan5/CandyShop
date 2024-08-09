using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Application.Features.Identity.Caching;
public static class UserCacheKey
{
    // Defines the refresh interval for the cache expiration token
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(30);
    // Object used for locking to ensure thread safety
    private static readonly object TokenLock = new();

    // CancellationTokenSource used for managing cache expiration
    private static CancellationTokenSource _tokenSource = new(RefreshInterval);
    /// <summary>
    /// Gets the memory cache entry options with an expiration token.
    /// </summary>
    public static MemoryCacheEntryOptions MemoryCacheEntryOptions =>
        new MemoryCacheEntryOptions().AddExpirationToken(new CancellationChangeToken(GetOrCreateTokenSource().Token));
    public static string GetCampaignUsersCacheKey(string? userId) => $"GetCampaignUsersCacheKey{userId}";

    /// <summary>
    /// Gets or creates a new <see cref="CancellationTokenSource"/> with the specified refresh interval.
    /// </summary>
    /// <returns>The current or new <see cref="CancellationTokenSource"/>.</returns>
    public static CancellationTokenSource GetOrCreateTokenSource()
    {
        lock (TokenLock)
        {
            if (_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Dispose();
                _tokenSource = new CancellationTokenSource(RefreshInterval);
            }
            return _tokenSource;
        }
    }
    /// <summary>
    /// Refreshes the cache expiration token by cancelling and recreating the <see cref="CancellationTokenSource"/>.
    /// </summary>
    public static void Refresh()
    {
        lock (TokenLock)
        {
            if (!_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = new CancellationTokenSource(RefreshInterval);
            }
        }
    }
}
