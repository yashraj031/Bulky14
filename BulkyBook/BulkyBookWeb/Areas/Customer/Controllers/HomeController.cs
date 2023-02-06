using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;

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

        public IActionResult Details(int id)
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                Product = _unitofwork.Product.GetFirstOrDefault(u => u.ID == id, includeProperties: "category,CoverType")
            };     
            return View(cartObj);
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