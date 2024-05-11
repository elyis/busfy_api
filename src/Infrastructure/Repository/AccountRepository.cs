using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace busfy_api.src.Infrastructure.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _distributedCache;
        private readonly string _prefix = "account:";
        private readonly DistributedCacheEntryOptions _options = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(3),
            AbsoluteExpiration = DateTime.UtcNow.AddMinutes(6)
        };

        public AccountRepository(
            AppDbContext context,
            IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        public async Task<UnconfirmedAccount?> CreateOrUpdateCode(SignUpBody body, string confirmationCode)
        {
            var account = await Get(body.Email);
            if (account != null)
            {
                account.ValidityPeriodCode = DateTime.UtcNow.AddMinutes(5);
                account.ConfirmationCode = confirmationCode;
            }
            else
            {
                account = new UnconfirmedAccount
                {
                    Email = body.Email,
                    ConfirmationCode = confirmationCode,
                    Nickname = body.Nickname,
                    Password = body.Password,
                    ValidityPeriodCode = DateTime.UtcNow.AddMinutes(5)
                };
                account = (await _context.UnconfirmedAccounts.AddAsync(account)).Entity;
            }

            await _context.SaveChangesAsync();
            // var resultString = SerializeObject(account);
            // await _distributedCache.SetStringAsync($"{_prefix}{account.Email}", resultString, _options);
            return account;
        }

        public async Task<UnconfirmedAccount?> Get(string email)
        {
            // var cachedString = await _distributedCache.GetStringAsync($"{_prefix}{email}");
            UnconfirmedAccount? account = null;

            // if (!string.IsNullOrEmpty(cachedString))
            // {
            //     account = DeserializeObject<UnconfirmedAccount>(cachedString);
            //     if (account != null)
            //     {
            //         _context.Attach(account);
            //         return account;
            //     }
            // }

            account = await _context.UnconfirmedAccounts.FirstOrDefaultAsync(e => e.Email == email);
            // if (account != null)
            // {
            //     var resultString = SerializeObject(account);
            //     await _distributedCache.SetStringAsync($"{_prefix}{account.Email}", resultString, _options);
            // }

            return account;
        }

        public async Task<bool> Remove(string email)
        {
            var account = await Get(email);
            if (account == null)
                return true;

            _context.UnconfirmedAccounts.Remove(account);
            await _context.SaveChangesAsync();
            // await _distributedCache.RemoveAsync($"{_prefix}{account.Email}");
            return true;
        }

        private static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private static T? DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}

