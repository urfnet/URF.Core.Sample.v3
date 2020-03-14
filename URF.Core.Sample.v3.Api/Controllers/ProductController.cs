using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using URF.Core.Sample.v3.Abstractions;
using URF.Core.Sample.v3.Models;

namespace URF.Core.Sample.v3.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        public IUrfSampleUnitOfWork UnitOfWork { get; }

        public ProductController(IUrfSampleUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
        {
            var products = await UnitOfWork.ProductsRepository.Queryable().ToListAsync();
            return Ok(products);
        }

        // GET: api/Produc/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await UnitOfWork.ProductsRepository.FindAsync(id);
            if (product == null)
                return NotFound();
            return product;
        }

        // PUT: api/Produc/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            UnitOfWork.ProductsRepository.Update(product);

            try
            {
                await UnitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(id))
                    return NotFound();
                else
                    throw;
            }
            return Ok(product);
        }

        // POST: api/Product
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            UnitOfWork.ProductsRepository.Insert(product);
            await UnitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var result = await UnitOfWork.ProductsRepository.DeleteAsync(id);
            if (!result)
                return NotFound();
            await UnitOfWork.SaveChangesAsync();
            return NoContent();
        }

        private async Task<bool> ProductExists(int id)
        {
            return await UnitOfWork.ProductsRepository.ExistsAsync(id);
        }
    }
}