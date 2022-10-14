using System;
using System.Data;
using System.Data.Common;
using Arrba.Parser.DbContext.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;


namespace Arrba.Parser.DbContext
{
    public class ParserDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Url> Urls { get; set; }
        public DbSet<UrlInfo> UrlInfos { get; set; }
        public DbSet<Dealership> Dealerships { get; set; }
        public DbSet<Payload> Payloads { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // sudo - u postgres psql db_name
            // ALTER USER postgres WITH PASSWORD 'new_password';
            // sudo service postgresql restart

            //Server=84.201.155.72;Port=5432;User Id=postgres;Password=123456;Database=DbArrbaParser
            //Host=localhost;Port=5432;Username=postgres;Password=123456;Database=DbArrbaParser
            var connection = new NpgsqlConnection("Server=84.201.141.59;Port=5432;Database=DbArrbaParser;Username=postgres;Password=654321");
            connection.StateChange += (sender, args) =>
            {
                if (args.OriginalState == ConnectionState.Open)
                {
                    System.Diagnostics.Debug.WriteLine("ConnectionState.Open ....");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ConnectionState: " + args.OriginalState);
                }

                //var senderConnection = (DbConnection)sender;

                //using (var command = senderConnection.CreateCommand())
                //{
                //    command.Connection = senderConnection;
                //    command.CommandText = "-- TODO: Put little SQL command here.";

                //    command.ExecuteNonQuery();
                //}
            };
            optionsBuilder.UseNpgsql(connection, builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UrlInfoConfiguration());
            modelBuilder.ApplyConfiguration(new UrlConfiguration());
        }
    }
}
