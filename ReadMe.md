# URF Core 3.0 Demo

Sample using Unit of Work and Repository Framework (URF) with ASP.NET Core 3.0

## Prerequisites

- .NET Core SDK [latest 3.0 version](https://dotnet.microsoft.com/download/dotnet-core)
- EF Core CLI

    ```
    dotnet tool install --global dotnet-ef --version 3.0.0-*
    ```

## EF Core Code First

1. Create a .NET Standard 2.1 class library

    ```
    dotnet new classlib -n Demo.UrfCore3.EF
    ```
   - Edit csproj file to set `TargetFramework` to `netstandard2.0`.

2. Add NuGet packages (v3.0+)
   - Microsoft.EntityFrameworkCore.SqlServer
   - Microsoft.EntityFrameworkCore.Design
3. Add `Product` class

    ```csharp
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
    }
    ```

4. Add `UrfDemoContext` class that extends `DbContext`

    ```csharp
    public class UrfDemoContext : DbContext
    {
        public UrfDemoContext(DbContextOptions<UrfDemoContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
    }
    ```

5. Add `UrfDemoContextFactory` class that implements `IDesignTimeDbContextFactory<UrfDemoContext>`

    ```csharp
    public class UrfDemoContextFactory : IDesignTimeDbContextFactory<UrfDemoContext>
    {
        public UrfDemoContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UrfDemoContext>();
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;initial catalog=UrfDemo;Integrated Security=True; MultipleActiveResultSets=True");
            return new UrfDemoContext(optionsBuilder.Options);
        }
    }
    ```

6. Add code to `UrfDemoContext` to seed data the database

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

7. Open command prompt at Data project directory and add an EF migration

    ```
    dotnet ef migrations add initial
    ```

8. Apply the EF migration to the database

    ```
    dotnet ef database update
    ```

## URF.Core Unit of Work

1. Add the `URF.Core.EF` package to the Demo.UrfCore3.EF class library.

2. Create a `IUrfDemoUnitOfWork` interface that extends `IUnitOfWork`.

    ```csharp
    public interface IUrfDemoUnitOfWork : IUnitOfWork
    {
        public IRepository<Product> ProductsRepository { get; }
    }
    ```

3. Create a `UrfDemoUnitOfWork` class that extends `UnitOfWork` and implements `IUrfDemoUnitOfWork`.

    ```csharp
    public class UrfDemoUnitOfWork : UnitOfWork, IUrfDemoUnitOfWork
    {
        public UrfDemoUnitOfWork(DbContext context, IRepository<Product> productsRepository) : base(context)
        {
            ProductsRepository = productsRepository;
        }

        public IRepository<Product> ProductsRepository { get; }
    }
    ```

## Web API with URF.Core

1. Scaffold a new ASP.NET Core Web API project

    ```
    dotnet new webapi -n Demo.UrfCore3.Api
    ```

2. Add NuGet packages  (v3.0+)
   - Microsoft.EntityFrameworkCore.SqlServer
   - Microsoft.EntityFrameworkCore.Design
   - URF.Core.EF

3. Reference the class library project.
   - Demo.UrfCore3.EF

4. Add a connection string to appsettings.json.

    ```json
    "ConnectionStrings": {
    "UrfDemoContext": "Data Source=(localdb)\\MSSQLLocalDB;initial catalog=UrfDemo;Integrated Security=True; MultipleActiveResultSets=True"
    }
    ```

5. Register services in `Startup.ConfigureServices`.

    ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        var connectionString = Configuration.GetConnectionString(nameof(UrfDemoContext));
        services.AddDbContext<UrfDemoContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<DbContext, UrfDemoContext>();
        services.AddScoped<IUrfDemoUnitOfWork, UrfDemoUnitOfWork>();
        services.AddScoped<IRepository<Product>, Repository<Product>>();
    }
    ```

6. Add a `ProductsController` controller.

    ```csharp
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
    ```

6. Start the Web API project and test with Postman.

    ```
    GET: https://localhost:5001/api/products
    GET: https://localhost:5001/api/products/1
    ```
    ```
    POST: https://localhost:5001/api/products
    ```
    ```json
    {
        "productName": "Chocolato",
        "unitPrice": 4.00
    }
    ```
    ```
    PUT: https://localhost:5001/api/products/4
    ```
    ```json
    {
        "id": 4,
        "productName": "Chocolato",
        "unitPrice": 5.00
    }
    ```
    ```
    DELETE: https://localhost:5001/api/products/4
    ```
