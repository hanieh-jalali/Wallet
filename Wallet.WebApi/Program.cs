using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wallet.Domain.Contract.Repositories;
using Wallet.Infrastructure.Context;
using Wallet.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repository services later here
var configuration = builder.Configuration;
configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Register services with DI container
builder.Services.AddScoped<IWalletRepository>(provider =>
{
    var walletFilePath = configuration.GetValue<string>("WalletFilePath");
    var dbContext = provider.GetRequiredService<WalletDbContext>();
    return new WalletRepository(walletFilePath, dbContext);
});

builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
