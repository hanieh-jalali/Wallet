using HBitcoin.KeyManagement;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Domain.Contract.Repositories
{
    public interface IWalletRepository
    {
        Mnemonic CreateWallet(string walletName, string password);
        Safe RecoverWallet(string password, Mnemonic mnemonic, DateTime creationTime, string WalletName);
        Entities.Models.Wallet LoadWallet(string walletName, string password);
    }
}
