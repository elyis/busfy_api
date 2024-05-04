using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Infrastructure.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
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
            return account;
        }

        public async Task<UnconfirmedAccount?> Get(string email)
        {
            return await _context.UnconfirmedAccounts.FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<bool> Remove(string email)
        {
            var account = await Get(email);
            if (account == null)
                return true;

            _context.UnconfirmedAccounts.Remove(account);
            await _context.SaveChangesAsync();

            return true;
        }


    }
}