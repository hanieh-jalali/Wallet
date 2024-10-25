using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Enums;

namespace Wallet.Domain.Entities.Models
{
    public class Transaction : BaseEntity
    {
        public string SenderAddress { get; set; }
        public string RecipientAddress { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public TransactionStatus Status { get; set; }
        public string AddressId { get; set; }
        public Transaction() { }

        public Transaction(string senderAddress, string recipientAddress, decimal amount, string addressId)
        {
            SenderAddress = senderAddress;
            RecipientAddress = recipientAddress;
            Amount = amount;
            AddressId = addressId;
            Timestamp = DateTime.UtcNow;
            Status = TransactionStatus.Pending;
        }
        public void CompleteTransaction()
        {
            Status = TransactionStatus.Success;
        }
    }
}
