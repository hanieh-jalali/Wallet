using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Entities.ViewModel;

namespace Wallet.Domain.Contract.Repositories
{
    public interface ITransactionRepository
    {
        Task SendTransactionAsync(string walletName, string senderAddress, decimal amountToSend, string recipientAddress);
        IEnumerable<History> GetTransactionHistory(string walletAddress);
    }
}
