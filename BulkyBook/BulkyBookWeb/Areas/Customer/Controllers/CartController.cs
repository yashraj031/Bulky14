using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitofwork _unitOfWork;

        public ShopingCartVM ShopingCartVM { get; set; }
        public int OrderTotal { get; set; }
        public CartController(IUnitofwork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShopingCartVM = new ShopingCartVM()
            {
                ListCart = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product")
            };
            foreach(var cart in ShopingCartVM.ListCart) 
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShopingCartVM.CartTotal += (cart.Price * cart.Count);
            }
            return View(ShopingCartVM);
        }

		public IActionResult Summary()
		{
            return View();
		}

		public IActionResult plus(int cartId)
        {
            var cart =_unitOfWork.ShopingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShopingCart.IncrementCount(cart, 1);
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }

		public IActionResult minus(int cartId)
		{
			var cart = _unitOfWork.ShopingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShopingCart.Remove(cart);
            }
            else 
            {
				_unitOfWork.ShopingCart.DecrementCount(cart, 1);
			}
			
			_unitOfWork.save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult remove(int cartId)
		{
			var cart = _unitOfWork.ShopingCart.GetFirstOrDefault(u => u.Id == cartId);
			_unitOfWork.ShopingCart.Remove(cart);
			_unitOfWork.save();
			return RedirectToAction(nameof(Index));
		}
		private double GetPriceBasedOnQuantity(double quantity,double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else
            {
				if (quantity <= 100)

			    {
			        return price50;
				}
                return price100;
			} 
        }
    }
}
