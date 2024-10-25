using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Domain.Contract.Repositories
{
    public interface IBalanceRepository
    {
        decimal GetBalance(string walletAddress);
    }
}
