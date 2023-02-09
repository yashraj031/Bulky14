using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofwork _unitofwork;

        public HomeController(ILogger<HomeController> logger,IUnitofwork unitofWork)
        {
            _logger = logger;
            _unitofwork = unitofWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productsList = _unitofwork.Product.GetAll(includeProperties: "category,CoverType");
            return View(productsList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                ProductId= productId,
                Product = _unitofwork.Product.GetFirstOrDefault(u => u.ID == productId, includeProperties: "category,CoverType")
            };     
            return View(cartObj);
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart cartFromDb = _unitofwork.ShopingCart.GetFirstOrDefault(
                u=>u.ApplicationUserId==claim.Value && u.ProductId ==shoppingCart.ProductId);
            if (cartFromDb == null)
            {
                _unitofwork.ShopingCart.Add(shoppingCart);
            }
            else
            {
                _unitofwork.ShopingCart.IncrementCount(cartFromDb, shoppingCart.Count);
            }
            
            //_unitofwork.ShopingCart.Add(shoppingCart);
            _unitofwork.save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}