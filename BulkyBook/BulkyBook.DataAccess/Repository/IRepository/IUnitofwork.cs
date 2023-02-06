using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
   public interface IUnitofwork
    {
        ICategoryRepository category { get; }
        ICoverTypeRepository CoverType { get; }

        IProductRepository Product { get; }

        void save();
    }
}
