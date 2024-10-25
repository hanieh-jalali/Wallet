using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Entities.Models;

namespace Wallet.Domain.Contract.Repositories
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetAllAddressesAsync(string walletName);
        Task<Address> GetAddressAsync(string walletName, int index);
        Task AddAddressAsync(string walletName);
    }
}
