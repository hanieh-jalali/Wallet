using Microsoft.AspNetCore.Mvc;

namespace Wallet.WebApi.Controllers
{
    public class BalanceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
