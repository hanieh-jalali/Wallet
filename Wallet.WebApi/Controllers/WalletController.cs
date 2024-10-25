using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Wallet.Domain.Contract.Repositories;

namespace Wallet.WebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly string _walletFilePath;

        public WalletController(IWalletRepository walletRepository, IConfiguration configuration)
        {
            _walletRepository = walletRepository;
            _walletFilePath = configuration["WalletFilePath"] ?? "default_wallet_path/";
        }

        [HttpPost("create")]
        public IActionResult CreateWallet(string walletName, string password)
        {
            var mnemonic = _walletRepository.CreateWallet(walletName, password);
            return Ok(new { walletName, mnemonic = mnemonic.ToString() });
        }

        [HttpPost("recover")]
        public IActionResult RecoverWallet(string walletName, string password, string mnemonicPhrase)
        {
            try
            {
                var mnemonic = new Mnemonic(mnemonicPhrase);
                var creationDateTime = DateTime.UtcNow;

                string walletFilePath = Path.Combine(_walletFilePath, $"{walletName}.json");

                if (!System.IO.File.Exists(walletFilePath))
                {
                    return NotFound(new { message = "Wallet not found. Please check the wallet name." });
                }

                var safe = _walletRepository.RecoverWallet(password, mnemonic, creationDateTime, walletName);

                return Ok(new
                {
                    message = "Wallet recovered successfully",
                    walletFilePath = walletFilePath
                });
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(new { message = "Wallet recovery failed", error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred during wallet recovery", error = ex.Message });
            }
        }

        [HttpGet("load")]
        public IActionResult LoadWallet(string walletName, string password)
        {
            try
            {
                var wallet = _walletRepository.LoadWallet(walletName, password);

                if (wallet == null)
                {
                    return NotFound(new { message = "Wallet not found or invalid password." });
                }

                string walletFilePath = Path.Combine(_walletFilePath, $"{walletName}.json");

                return Ok(new
                {
                    message = "Wallet loaded successfully",
                    wallet = new
                    {
                        wallet.Balance,
                        wallet.Mnemonic,
                        wallet.FilePath,
                        wallet.CreateDate,
                        wallet.LastUpdated,
                        Addresses = wallet.Addresses.Select(address => new
                        {
                            address.Id,
                            address.AddressValue,
                            address.IsUsed,
                            Transactions = address.Transactions.Select(t => new
                            {
                                t.Id,
                                     
                            }).ToList()
                        }).ToList()
                    },
                    walletFilePath
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while loading the wallet", error = ex.Message });
            }
        }

    }
}
