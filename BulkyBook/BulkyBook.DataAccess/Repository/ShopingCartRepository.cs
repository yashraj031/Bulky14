using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ShopingCartRepository : Repository<ShoppingCart>, IShopingCartRepository
    {
        private readonly ApplicationDbContext _db;

        public ShopingCartRepository(ApplicationDbContext db) : base(db)
        {
            this._db = db;
        }

        public int DecrementCount(ShoppingCart shopingCart, int count)
        {
            shopingCart.Count -= count;
            return shopingCart.Count;
        }

        public int IncrementCount(ShoppingCart shopingCart, int count)
        {
            shopingCart.Count += count;
            return shopingCart.Count;
        }
    }
}
