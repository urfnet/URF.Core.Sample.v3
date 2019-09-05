using Demo.UrfCore3.EF.Models;
using URF.Core.Abstractions;

namespace Demo.UrfCore3.EF.UnitsOfWork
{
    public interface IUrfDemoUnitOfWork : IUnitOfWork
    {
        public IRepository<Product> ProductsRepository { get; }
    }
}
