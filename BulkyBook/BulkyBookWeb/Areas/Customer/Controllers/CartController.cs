using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitofwork _unitOfWork;
        [BindProperty]
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
                includeProperties: "Product"),
                OrderHeader = new ()
            };
            foreach(var cart in ShopingCartVM.ListCart) 
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShopingCartVM.OrderHeader.OrderTotal   += (cart.Price * cart.Count);
            }
            return View(ShopingCartVM);
        }

		public IActionResult Summary()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShopingCartVM = new ShopingCartVM()
            {
                ListCart = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
			ShopingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(
				u => u.Id == claim.Value);

			ShopingCartVM.OrderHeader.Name = ShopingCartVM.OrderHeader.ApplicationUser.Name;
			ShopingCartVM.OrderHeader.PhoneNumber = ShopingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShopingCartVM.OrderHeader.StreetAddress = ShopingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShopingCartVM.OrderHeader.City = ShopingCartVM.OrderHeader.ApplicationUser.City;
			ShopingCartVM.OrderHeader.State = ShopingCartVM.OrderHeader.ApplicationUser.State;
			ShopingCartVM.OrderHeader.PostalCode = ShopingCartVM.OrderHeader.ApplicationUser.PostalCode;

			foreach (var cart in ShopingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
                    cart.Product.Price50, cart.Product.Price100);
                ShopingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShopingCartVM);
           
		}
        
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
		public IActionResult SummaryPost()
		{

			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShopingCartVM.ListCart = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product");
            ShopingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
			ShopingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			ShopingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
			ShopingCartVM.OrderHeader.ApplicationUserId = claim.Value;

			foreach (var cart in ShopingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price,
					cart.Product.Price50, cart.Product.Price100);
				ShopingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
            _unitOfWork.OrderHeader.Add(ShopingCartVM.OrderHeader);
            _unitOfWork.save();
			foreach (var cart in ShopingCartVM.ListCart)
			{
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShopingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.save();
			}

            //Stipe Setting
            var domain = "https://localhost:44303/";
			var options = new SessionCreateOptions
            {

                PaymentMethodTypes =new List<string>
                {
					"card"
				},
            	
				LineItems = new List<SessionLineItemOptions>()
		        ,
				Mode = "payment",
				SuccessUrl = domain+$"customer/cart/OrderConfirmation?id={ShopingCartVM.OrderHeader.Id}",
				CancelUrl = domain+$"customer/cart/index",
			};
            foreach(var item in ShopingCartVM.ListCart)
            {

                var sessinLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),//20.00 --> 2000
                        Currency = "INR",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        },
                    },
                    Quantity = item.Count,

                };
                options.LineItems.Add(sessinLineItem);
		    }

			var service = new SessionService();
			Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(ShopingCartVM.OrderHeader.Id,session.Id,session.PaymentIntentId);
            _unitOfWork.save(); 
			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);


			//_unitOfWork.ShopingCart.RemoveRange(ShopingCartVM.ListCart);
   //         _unitOfWork.save();
   //         return RedirectToAction("Index", "Home");			

		}

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id ==id);
			var service = new SessionService();
			Session session = service.Get(orderHeader.SessionId);
			//check the stripe status
			if (session.PaymentStatus.ToLower() == "paid")
			{
				_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                _unitOfWork.save();
			}
			List<ShoppingCart> shoppingCarts = _unitOfWork.ShopingCart.GetAll(u => u.ApplicationUserId ==
			orderHeader.ApplicationUserId).ToList();
			_unitOfWork.ShopingCart.RemoveRange(shoppingCarts);
			_unitOfWork.save();
			return View(id);
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
