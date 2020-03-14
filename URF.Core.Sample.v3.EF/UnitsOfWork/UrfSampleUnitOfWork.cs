using Microsoft.EntityFrameworkCore;
using URF.Core.Abstractions;
using URF.Core.EF;
using URF.Core.Sample.v3.Abstractions;
using URF.Core.Sample.v3.Models;

namespace URF.Core.Sample.v3.EF.UnitsOfWork
{
    public class UrfSampleUnitOfWork : UnitOfWork, IUrfSampleUnitOfWork
    {
        public UrfSampleUnitOfWork(DbContext context, IRepository<Product> productsRepository) : base(context)
        {
            ProductsRepository = productsRepository;
        }

        public IRepository<Product> ProductsRepository { get; }
    }
}
