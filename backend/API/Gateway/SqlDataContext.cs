﻿using LibraryApp.API.Authors;
using LibraryApp.API.Books;
using LibraryApp.API.Checkouts;
using LibraryApp.API.Publishers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LibraryApp.API.Gateway;

public class SqlDataContext(DbContextOptions<SqlDataContext> options) : DbContext(options)
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Checkout> Checkouts {get;set;}
    public DbSet<Publisher> Publishers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
