using NBitcoin;
using QBitNinja.Client;
using Microsoft.EntityFrameworkCore;
using Wallet.Domain.Entities.ViewModel;
using Wallet.Infrastructure.Context;
using Transaction = NBitcoin.Transaction;

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
            var transaction = new Transaction();

            var addressToSend = BitcoinAddress.Create(recipientAddress, Network.TestNet);

            transaction.Outputs.Add(new TxOut
            {
                Value = new Money(amountToSend, MoneyUnit.BTC),
                ScriptPubKey = addressToSend.ScriptPubKey
            });

            var changeAmount = 0.0001m;
            var senderAddressObj = BitcoinAddress.Create(senderAddress, Network.TestNet);
            transaction.Outputs.Add(new TxOut
            {
                Value = new Money(changeAmount, MoneyUnit.BTC),
                ScriptPubKey = senderAddressObj.ScriptPubKey
            });

            var bitcoinPrivateKey = new BitcoinSecret("your_private_key_here", Network.TestNet);

          
            var prevOutHash = uint256.Parse("previous_transaction_hash_here");
            var prevOutIndex = 0; 
            var outpoint = new OutPoint(prevOutHash, prevOutIndex);

            // Add the input to the transaction
            transaction.Inputs.Add(new TxIn { PrevOut = outpoint });

            // Sign the transaction
            var coin = new Coin(outpoint, transaction.Outputs[0]); // Assuming you are spending from the first output
            transaction.Sign(bitcoinPrivateKey, new[] { coin });

            // Broadcast the transaction
            var broadcastResponse = await _client.Broadcast(transaction);

            // Check if the transaction was successfully broadcasted
            if (!broadcastResponse.Success)
            {
                throw new Exception($"Transaction failed to send: {broadcastResponse.Error.Reason}");
            }

            // Get the transaction ID
            var transactionId = transaction.GetHash().ToString();

            // Create and save a record of the transaction
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

            // Add the transaction record to the database
            await _dbContext.Set<Domain.Entities.Models.Transaction>().AddAsync(transactionRecord);
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
    }
}
