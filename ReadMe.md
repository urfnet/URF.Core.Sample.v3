# URF Core 3.x Demo

Sample using Unit of Work and Repository Framework (URF) with ASP.NET Core 3.x

## Prerequisites

- .NET Core SDK [latest 3.x version](https://dotnet.microsoft.com/download/dotnet-core)
- EF Core CLI (specify current version)
    ```
    dotnet tool uninstall --global dotnet-ef
    dotnet tool install --global dotnet-ef --version 3.1.2
    ```

## EF Core Code First

1. Create `Models` .NET Standard 2.1 class library.

2. Add `Product` class.
    ```csharp
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
    }
    ```

3. Create `EF` .NET Core class library.
   - Reference the Models project.
   - Add NuGet packages:
     - URF.Core.EF
     - Microsoft.EntityFrameworkCore.SqlServer
     - Microsoft.EntityFrameworkCore.Design

4. Add `UrfSampleContext` class that extends `DbContext`
   - Add Contexts folder.
  
    ```csharp
    public class UrfSampleContext : DbContext
    {
        public UrfSampleContext(DbContextOptions<UrfSampleContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
    }
    ```

5. Add `UrfSampleContextFactory` class that implements `IDesignTimeDbContextFactory<UrfSampleContext>`

    ```csharp
    public class UrfSampleContextFactory : IDesignTimeDbContextFactory<UrfSampleContext>
    {
        public UrfSampleContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UrfSampleContext>();
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;initial catalog=UrfSample;Integrated Security=True; MultipleActiveResultSets=True");
            return new UrfSampleContext(optionsBuilder.Options);
        }
    }
    ```

6. Add code to `UrfSampleContext` to seed data the database.

    ```csharp
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, ProductName = "Chai", UnitPrice = 1 },
            new Product { Id = 2, ProductName = "Chang", UnitPrice = 2},
            new Product { Id = 3, ProductName = "Cappuccino", UnitPrice = 3 }
        );
    }
    ```

7. Open command prompt at EF project directory and add an EF migration.

    ```
    dotnet ef migrations add initial
    ```

8.  Apply the EF migration to the database

    ```
    dotnet ef database update
    ```

## URF.Core Unit of Work

1. Create `Abstractions` .NET Standard 2.1 class library.
   - Reference the Models project.
   - Add URF.Core.Abstractions package.
   - Add `IUrfSampleUnitOfWork` interface that extends `IUnitOfWork` and adds `ProductsRepository` property.
    ```csharp
    public interface IUrfSampleUnitOfWork : IUnitOfWork
    {
        public IRepository<Product> ProductsRepository { get; }
    }
    ```

2. In the `EF` project add a reference to the `Abstractions` project.
   - Add `UnitsOfWork` folder.
   - Create a `UrfSampleUnitOfWork` class that extends `UnitOfWork` and implements `IUrfSampleUnitOfWork`.

    ```csharp
    public class UrfSampleUnitOfWork : UnitOfWork, IUrfSampleUnitOfWork
    {
        public UrfSampleUnitOfWork(DbContext context, IRepository<Product> productsRepository) : base(context)
        {
            ProductsRepository = productsRepository;
        }

        public IRepository<Product> ProductsRepository { get; }
    }
    ```

## Web API with URF.Core

1. Add a new ASP.NET Core Web API project
   - Remove `WeatherForecast` and `WeatherForecastController` classes.
   - Add NuGet packages:
     - Microsoft.EntityFrameworkCore.SqlServer
     - URF.Core.EF
   - Reference Models, Abstractions and EF projects.

2. Add a connection string to appsettings.json.

    ```json
    "ConnectionStrings": {
    "UrfSampleContext": "Data Source=(localdb)\\MSSQLLocalDB;initial catalog=UrfSample;Integrated Security=True; MultipleActiveResultSets=True"
    }
    ```

3. Register services in `Startup.ConfigureServices`.

    ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        var connectionString = Configuration.GetConnectionString(nameof(UrfSampleContext));
        services.AddDbContext<UrfSampleContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<DbContext, UrfSampleContext>();
        services.AddScoped<IUrfSampleUnitOfWork, UrfSampleUnitOfWork>();
        services.AddScoped<IRepository<Product>, Repository<Product>>();
    }
    ```

4. Add a `ProductController` controller.

    ```csharp
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
    ```

5. Update `launchSettings.json` in the Properties folder.
   - Replace `weatherforecast` with `api/product`.
   - Set the `Api` project as the startup project.
   - Select `URF.Core.Sample.v3.Api` for debugging and press F5.

6. Start the Web API project and test with Postman.
   - Turn off 'SSL certificate verification' in Settings > General

    ```
    GET: https://localhost:5001/api/product
    GET: https://localhost:5001/api/product/1
    ```
    ```
    POST: https://localhost:5001/api/product
    ```
    ```json
    {
        "productName": "Chocolato",
        "unitPrice": 4.00
    }
    ```
    - Should return 201 Created with correct Location response header.
    ```
    PUT: https://localhost:5001/api/product/4
    ```
    ```json
    {
        "id": 4,
        "productName": "Chocolato",
        "unitPrice": 5.00
    }
    ```
    ```
    DELETE: https://localhost:5001/api/product/4
    ```
    ```
    GET: https://localhost:5001/api/product/4
    ```
    - Should return 404 Not Found.