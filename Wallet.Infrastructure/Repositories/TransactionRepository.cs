using NBitcoin;
using QBitNinja.Client;
using Microsoft.EntityFrameworkCore;
using Wallet.Domain.Entities.ViewModel;
using Wallet.Infrastructure.Context;

namespace Wallet.Infrastructure.Repositories
{
    public class TransactionRepository : Domain.Contract.Repositories.ITransactionRepository
    {
        private readonly QBitNinjaClient _client;
        private readonly WalletDbContext _dbContext;

        public TransactionRepository(WalletDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _client = new QBitNinjaClient(Network.TestNet);
        }
        public async Task SendTransactionAsync(string walletName, string senderAddress, decimal amountToSend, string recipientAddress)
        {
            var outpointToSpend = await GetOutpointToSpend(walletName, senderAddress);
            if (outpointToSpend == null)
            {
                throw new InvalidOperationException("No available UTXO to spend.");
            }

            var senderPrivateKey = new BitcoinSecret("cRbhkRRTxk11nnAYhSyuQDr6kvqnFEtmZM7woaGzSSzKeT7WYmcb", Network.TestNet);
            var builder = Network.TestNet.CreateTransactionBuilder();
            var recipient = BitcoinAddress.Create(recipientAddress, Network.TestNet);

            var senderBalance = await GetSenderBalance(senderAddress);
            if (senderBalance < amountToSend + 0.0001m)
            {
                throw new InvalidOperationException("Insufficient funds to cover the transaction and fees.");
            }

           
            var tx = builder
                .AddCoins(new Coin(outpointToSpend, new TxOut(new Money(amountToSend, MoneyUnit.BTC), senderPrivateKey.PubKey.Hash.ScriptPubKey)))
                .AddKeys(senderPrivateKey)
                .Send(recipient, new Money(amountToSend, MoneyUnit.BTC))
                .SetChange(senderPrivateKey.PubKey.GetAddress(ScriptPubKeyType.Segwit, Network.TestNet))
                .SendFees(Money.Coins(0.0001m)) 
                .BuildTransaction(true);

            var broadcastResponse = await _client.Broadcast(tx);
            if (!broadcastResponse.Success)
            {
                throw new Exception($"Transaction failed to send: {broadcastResponse.Error.Reason}");
            }

            var transactionId = tx.GetHash().ToString();
            var transactionRecord = new Domain.Entities.Models.Transaction
            {
                Id = transactionId,
                Amount = amountToSend,
                SenderAddress = senderAddress,
                RecipientAddress = recipientAddress,
                CreateDate = DateTime.UtcNow,
                LastUpdated = DateTime.Now,
                CreateUserId = "UserId"
            };

            await _dbContext.Set<Domain.Entities.Models.Transaction>().AddAsync(transactionRecord);
            await _dbContext.SaveChangesAsync();

            await UpdateBalancesAsync(senderAddress, recipientAddress, amountToSend);
        }

        private async Task<decimal> GetSenderBalance(string senderAddress)
        {
            var senderAddressEntity = await _dbContext.Set<Domain.Entities.Models.Address>()
                .FirstOrDefaultAsync(a => a.AddressValue == senderAddress);

            if (senderAddressEntity == null)
            {
                throw new InvalidOperationException("Sender address not found.");
            }

            return senderAddressEntity.Balance;
        }

        private async Task UpdateBalancesAsync(string senderAddress, string recipientAddress, decimal amountToSend)
        {
            var senderAddressEntity = await _dbContext.Set<Domain.Entities.Models.Address>()
                .FirstOrDefaultAsync(a => a.AddressValue == senderAddress);

            if (senderAddressEntity != null)
            {
                senderAddressEntity.Balance -= amountToSend;
            }

            var recipientAddressEntity = await _dbContext.Set<Domain.Entities.Models.Address>()
                .FirstOrDefaultAsync(a => a.AddressValue == recipientAddress);

            if (recipientAddressEntity != null)
            {
                recipientAddressEntity.Balance += amountToSend;
            }

            await _dbContext.SaveChangesAsync();
        }

        public IEnumerable<History> GetTransactionHistory(string walletAddress)
        {
            var historyList = new List<History>();
            var balance = _client.GetBalance(BitcoinAddress.Create(walletAddress, Network.TestNet), false).Result;

            foreach (var entry in balance.Operations)
            {
                foreach (var coin in entry.ReceivedCoins)
                {
                    if (coin.Amount is Money money)
                    {
                        historyList.Add(new History
                        {
                            TransactionId = coin.Outpoint.Hash.ToString(),
                            Amount = money.ToDecimal(MoneyUnit.BTC)
                        });
                    }
                }
            }

            return historyList;
        }

        private async Task<OutPoint> GetOutpointToSpend(string walletName, string senderAddress)
        {
            var wallet = await _dbContext.Set<Domain.Entities.Models.Wallet>().FirstOrDefaultAsync(w => w.WalletName == walletName)
                           ?? throw new InvalidOperationException("Wallet not found.");

            var senderAddressEntity = await _dbContext.Set<Domain.Entities.Models.Address>()
                .FirstOrDefaultAsync(a => a.WalletId == wallet.Id && a.AddressValue == senderAddress)
                           ?? throw new InvalidOperationException("Sender address not found for this wallet.");

            var transactions = await _dbContext.Set<Domain.Entities.Models.Transaction>()
                .Where(t => t.AddressId == senderAddressEntity.Id).ToListAsync();

            if (!transactions.Any())
            {
               return new OutPoint(uint256.Zero, 0);
            }

            var latestTransaction = transactions.OrderByDescending(t => t.CreateDate).FirstOrDefault();
            return new OutPoint(uint256.Parse(latestTransaction.Id), 0);
        }
    }
}
