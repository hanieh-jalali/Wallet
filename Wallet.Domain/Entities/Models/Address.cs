using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Domain.Entities.Models
{
    public class Address : BaseEntity
    {
        public string WalletId { get; set; }
        public string AddressValue { get; set; }
        public bool IsUsed { get; set; }
        public List<Transaction> Transactions { get; set; }
        public Address()
        {
            Transactions = new List<Transaction>();
        }
        public Address(string walletId, string addressValue)
        {
            WalletId = walletId;
            AddressValue = addressValue;
            IsUsed = false;
            Transactions = new List<Transaction>();
        }
    }
}
