using HBitcoin.KeyManagement;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Contract.Repositories;
using Wallet.Infrastructure.Context;

namespace Wallet.Infrastructure.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly WalletDbContext _dbContext;
        private readonly string WalletFilePath;

        public WalletRepository(string walletFilePath, WalletDbContext dbContext)
        {
            _dbContext = dbContext;
            WalletFilePath = walletFilePath;
        }

        public Mnemonic CreateWallet(string walletName, string password)
        {
            Safe safe = Safe.Create(out Mnemonic mnemonic, password, $"{WalletFilePath}{walletName}.json", Network.TestNet);
            return mnemonic;
        }
        public Safe RecoverWallet(string password, Mnemonic mnemonic, DateTime creationTime, string walletName)
        {

            string recoveredWalletFilePath = Path.Combine(WalletFilePath, $"{walletName}.json");

            if (File.Exists(recoveredWalletFilePath))
            {
                throw new NotSupportedException($"Wallet already exists at {recoveredWalletFilePath}. Please choose a different name or delete the existing wallet.");
            }

            creationTime = DateTime.UtcNow;

            var safe = Safe.Recover(mnemonic, password, recoveredWalletFilePath, Network.TestNet, creationTime);

            var wallet = new Domain.Entities.Models.Wallet
            {
                WalletName = walletName,
                FilePath = recoveredWalletFilePath,
                Balance = 0,
                CreateUserId = "UserId",
                Password = password,
                Mnemonic = mnemonic.ToString(),
                CreateDate = creationTime,
                LastUpdated = creationTime
            };

            _dbContext.Wallets.Add(wallet);
            _dbContext.SaveChanges();

            return safe;
        }

        public Domain.Entities.Models.Wallet LoadWallet(string walletName, string password)
        {
            var wallet = _dbContext.Wallets
                .Include(w => w.Addresses)
                .ThenInclude(a => a.Transactions)
                .FirstOrDefault(w => w.WalletName == walletName && w.Password == password);

            return wallet;
        }
    }
}
