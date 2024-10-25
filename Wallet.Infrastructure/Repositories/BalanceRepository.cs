using NBitcoin;
using QBitNinja.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Contract.Repositories;

namespace Wallet.Infrastructure.Repositories
{
    public class BalanceRepository : IBalanceRepository
    {
        private readonly QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);

        public decimal GetBalance(string walletAddress)
        {
            var balance = client.GetBalance(BitcoinAddress.Create(walletAddress, Network.TestNet), true).Result;
            decimal totalBalance = 0;

            foreach (var entry in balance.Operations)
            {
                foreach (var coin in entry.ReceivedCoins)
                {
                    Money amount = (Money)coin.Amount;
                    totalBalance += amount.ToDecimal(MoneyUnit.BTC);
                }
            }

            return totalBalance;
        }
    }
}