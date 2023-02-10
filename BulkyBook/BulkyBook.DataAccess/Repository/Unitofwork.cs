using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Unitofwork : IUnitofwork
    {
        private readonly ApplicationDbContext _db;

        public Unitofwork(ApplicationDbContext db) 
        {
            _db = db;
            category = new CategoryRepository(_db);
            CoverType = new CoverTypeRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            ShopingCart = new ShopingCartRepository(_db);
            ApplicationUser= new ApplicationUserRepository(_db);
            OrderHeader= new OrderHeaderRepository(_db);
            OrderDetail= new OrderDetailRepository(_db);  

		}
        public ICategoryRepository category { get; private set; }
        public ICoverTypeRepository CoverType{ get; private set; }

        public IProductRepository Product { get; private set; }

		public ICompanyRepository Company {get; private set;}

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IShopingCartRepository ShopingCart { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }

        public void save()
        {
           _db.SaveChanges();
        }
    }
}
