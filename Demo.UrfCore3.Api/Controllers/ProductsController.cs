using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.UrfCore3.EF.Models;
using Demo.UrfCore3.EF.UnitsOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.UrfCore3.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        public IUrfDemoUnitOfWork UnitOfWork { get; }

        public ProductsController(IUrfDemoUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await UnitOfWork.ProductsRepository.Query().SelectAsync();
            return Ok(products);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await UnitOfWork.ProductsRepository.FindAsync(id);
            if (product == null)
                return NotFound();
            return product;
        }

        // PUT: api/Products/5
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

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            UnitOfWork.ProductsRepository.Insert(product);
            await UnitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
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
