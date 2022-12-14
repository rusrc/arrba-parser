// <auto-generated />
using System;
using Arrba.Parser.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Arrba.Parser.DbContext.Migrations
{
    [DbContext(typeof(ParserDbContext))]
    [Migration("20190408084904_InitialDatabase")]
    partial class InitialDatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Arrba.Parser.DbContext.Entities.Dealership", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("Name");

                    b.Property<string>("PhoneNumber");

                    b.Property<string>("ProviderName");

                    b.HasKey("Id");

                    b.ToTable("Dealerships");
                });

            modelBuilder.Entity("Arrba.Parser.DbContext.Entities.Url", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<string>("ErrorMessage");

                    b.Property<int>("ExternalId");

                    b.Property<int>("LinksRequestId");

                    b.Property<DateTime>("ModifiedDate");

                    b.Property<string>("StackTrace");

                    b.Property<int?>("Status");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("LinksRequestId");

                    b.HasIndex("Value")
                        .IsUnique();

                    b.ToTable("Urls");
                });

            modelBuilder.Entity("Arrba.Parser.DbContext.Entities.UrlInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<string>("ErrorMessage");

                    b.Property<string>("LastResponseStatus");

                    b.Property<int>("LinksCount");

                    b.Property<DateTime>("ModifiedDate");

                    b.Property<string>("ProviderName");

                    b.Property<string>("StackTrace");

                    b.HasKey("Id");

                    b.HasIndex("ProviderName")
                        .IsUnique();

                    b.ToTable("UrlInfos");
                });

            modelBuilder.Entity("Arrba.Parser.DbContext.Entities.Url", b =>
                {
                    b.HasOne("Arrba.Parser.DbContext.Entities.UrlInfo", "LinksRequest")
                        .WithMany("Link")
                        .HasForeignKey("LinksRequestId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
