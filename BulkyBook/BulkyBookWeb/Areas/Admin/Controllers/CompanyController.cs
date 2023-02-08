using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.IO;

namespace BulkyBookWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitofwork _unitofwork;        

        public CompanyController(IUnitofwork unitofwork)
        {
            _unitofwork = unitofwork;            

        }
        public IActionResult Index()
        {
           
            return View();
        }
      

        //GET
        public IActionResult Upsert(int? id)
        {
            Company company = new();
            

            if (id == null || id == 0)
            {
                
                return View(company);
            }
            else
            {
                company = _unitofwork.Company.GetFirstOrDefault(u => u.Id == id);
                
                return View(company);

            }
            
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj, IFormFile? file)
        {
           
            if (ModelState.IsValid)
            {
                
                if (obj.Id == 0)
                {
                    _unitofwork.Company.Add(obj);
					TempData["success"] = "Company created successfully";

				}
                else
                {
                    _unitofwork.Company.Update(obj);
					TempData["success"] = "Company updated successfully";
				}

                _unitofwork.save();
                TempData["success"] = "Product Created  Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }


		#region API CALLS

		[HttpGet]
		public IActionResult GetAll()
        {
            var companyList =_unitofwork.Company.GetAll();
            return Json(new { data = companyList });
        }

        [HttpDelete]

        public IActionResult Delete(int? id)
        {

            var obj = _unitofwork.Company.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, Message = "Error while deleting" });
            }

            
            _unitofwork.Company.Remove(obj);
            _unitofwork.save();
            return Json(new { success = true, Message = "Delete Successful" });


        }

        #endregion

    }

}
