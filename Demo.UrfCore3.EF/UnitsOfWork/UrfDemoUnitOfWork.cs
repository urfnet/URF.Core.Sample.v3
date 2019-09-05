using Demo.UrfCore3.EF.Models;
using Microsoft.EntityFrameworkCore;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace Demo.UrfCore3.EF.UnitsOfWork
{
    public class UrfDemoUnitOfWork : UnitOfWork, IUrfDemoUnitOfWork
    {
        public UrfDemoUnitOfWork(DbContext context, IRepository<Product> productsRepository) : base(context)
        {
            ProductsRepository = productsRepository;
        }

        public IRepository<Product> ProductsRepository { get; }
    }
}
