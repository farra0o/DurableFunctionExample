using DurableFunctionExample.DTO;
using DurableFunctionExample.Models;
using Microsoft.EntityFrameworkCore;

namespace DurableFunctionExample.DBContext
{
    public  class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           //ignore the DTO.
            modelBuilder.Ignore<ItemInOrderDto>();
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ItemOrder> ItemOrders { get; set; }
        public DbSet<User> Users { get; set; }

    }
   
}
