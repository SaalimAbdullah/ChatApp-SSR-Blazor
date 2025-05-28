using BlazorApp5.Data;

using BlazorApp5.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlazorApp5.Services
{
    public class UserService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private ApplicationUser? _cachedUser;

        public UserService(AuthenticationStateProvider authStateProvider, IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _authStateProvider = authStateProvider;
            _dbFactory = dbFactory;
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            if (_cachedUser != null)
                return _cachedUser;

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return null;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            await using var db = _dbFactory.CreateDbContext();
            _cachedUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return _cachedUser;
        }
    }

}
