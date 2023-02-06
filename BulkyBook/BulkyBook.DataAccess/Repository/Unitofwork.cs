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
        }
        public ICategoryRepository category { get; private set; }
        public ICoverTypeRepository CoverType{ get; private set; }

        public IProductRepository Product { get; private set; }

        public void save()
        {
           _db.SaveChanges();
        }
    }
}
