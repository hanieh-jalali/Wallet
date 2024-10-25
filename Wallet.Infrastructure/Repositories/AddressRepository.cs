using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wallet.Domain.Contract.Repositories;
using Wallet.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using Wallet.Infrastructure.Context;

namespace Wallet.Infrastructure.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly WalletDbContext _dbContext; 

        public AddressRepository(WalletDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAddressAsync(string walletName)
        {
            var wallet = await _dbContext.Set<Wallet.Domain.Entities.Models.Wallet>()
                                          .FirstOrDefaultAsync(w => w.WalletName == walletName);

            if (wallet == null)
            {
                throw new ArgumentException("Wallet not found.");
            }
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Segwit, Network.TestNet);

            var addressInfo = new Address
            {
                WalletId = wallet.Id,
                AddressValue = address.ToString(),
                IsUsed = false,
                CreateUserId = "UserId",
                CreateDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Balance = 0
            };

            await _dbContext.Set<Address>().AddAsync(addressInfo);
            await _dbContext.SaveChangesAsync();
        }
        
        public async Task<Address> GetAddressAsync(string walletName, int index)
        {
            var wallet = await _dbContext.Set<Wallet.Domain.Entities.Models.Wallet>()
                                          .FirstOrDefaultAsync(w => w.WalletName == walletName);

            if (wallet == null)
            {
                throw new ArgumentException("Wallet not found.");
            }

            var addresses = await _dbContext.Set<Address>()
                                             .Where(a => a.WalletId == wallet.Id)
                                             .ToListAsync();

            if (index < 0 || index >= addresses.Count)
            {
                throw new IndexOutOfRangeException("Address index out of range");
            }

            return addresses[index];
        }
        public async Task<IEnumerable<Address>> GetAllAddressesAsync(string walletName)
        {
            var wallet = await _dbContext.Set<Wallet.Domain.Entities.Models.Wallet>()
                                         .FirstOrDefaultAsync(w => w.WalletName == walletName);

            return await _dbContext.Set<Address>()
                                   .Where(a => a.WalletId == wallet.Id)
                                   .ToListAsync();
        }
    }
}
