using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class OrderController : Controller
	{
		private readonly IUnitofwork _unitofwork;

		public OrderController(IUnitofwork unitofwork)
		{
			_unitofwork = unitofwork;
		}
		public IActionResult Index()
		{
			return View();
		}


		#region API CALLS
		[HttpGet]
		public IActionResult GetAll(string status)
		
		{
			IEnumerable<OrderHeader> orderHeaders;
			orderHeaders = _unitofwork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

            switch (status)
            {
                case "pending":
					orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
				case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:                    
                    break;

            }

            return Json(new { data = orderHeaders });
		}
		#endregion
	}
}
