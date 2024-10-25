using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wallet.Domain.Entities.ViewModel;
using Wallet.Domain.Contract.Repositories;

namespace Wallet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendTransactionAsync([FromBody] SendTransactionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request.");
            }

            try
            {
                await _transactionRepository.SendTransactionAsync(request.WalletName, request.SenderAddress, request.AmountToSend, request.RecipientAddress);
                return Ok(new { message = "Transaction sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("history/{walletAddress}")]
        public ActionResult<IEnumerable<History>> GetTransactionHistory(string walletAddress)
        {
            try
            {
                var history = _transactionRepository.GetTransactionHistory(walletAddress);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
    public class SendTransactionRequest
    {
        public string WalletName { get; set; }
        public string SenderAddress { get; set; }
        public decimal AmountToSend { get; set; }
        public string RecipientAddress { get; set; }
    }

}
