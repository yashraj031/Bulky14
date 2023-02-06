﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            this._db = db;
        }


        public void Update(Product obj)
        {
          var objFromDb = _db.Products.FirstOrDefault(u => u.ID== obj.ID);
            if (objFromDb != null)
            {
                objFromDb.Title= obj.Title;
                objFromDb.Description= obj.Description;
                objFromDb.Price= obj.Price;
                objFromDb.ISBN = obj.ISBN;
                objFromDb.Price50= obj.Price50;
                objFromDb.Price100= obj.Price100;
                objFromDb.ListPrice= obj.ListPrice;
                objFromDb.CategoryId= obj.CategoryId;
                objFromDb.Author = obj.Author;
                objFromDb.CoverTypeId= obj.CoverTypeId;
                if(objFromDb != null)
                {
                    objFromDb.ImageUrl= obj.ImageUrl;
                }
            }
        }
    }
}
