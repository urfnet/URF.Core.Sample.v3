using URF.Core.Abstractions;
using URF.Core.Sample.v3.Models;

namespace URF.Core.Sample.v3.Abstractions
{
    public interface IUrfSampleUnitOfWork : IUnitOfWork
    {
        public IRepository<Product> ProductsRepository { get; }
    }
}
