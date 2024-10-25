using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Domain.Entities.Models
{
    public class Wallet : BaseEntity
    {
        public string WalletName { get; set; }
        public decimal Balance { get; set; }
        public string Password { get; set; }
        public string Mnemonic { get; set; }
        public string FilePath { get; set; }

        public List<Address> Addresses { get; set; }
        public Wallet() 
        {
            Addresses = new List<Address>(); 
        }
        public Wallet(string walletName, decimal initialBalance, string createUserId, string password, string mnemonic, string filePath)
        {
            WalletName = walletName;
            Balance = initialBalance;
            CreateUserId = createUserId;
            Password = password;
            Mnemonic = mnemonic;
            FilePath = filePath;
            Addresses = new List<Address>();
        }
        public void UpdateBalance(decimal amount)
        {
            Balance += amount;
        }
    }
}
