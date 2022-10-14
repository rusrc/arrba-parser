using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrba.Parser.DbContext.Entities
{
    public class Url
    {
        public int Id { get; set; }
        public int? PayloadId { get; set; }
        public int UrlInfoId { get; set; }
        public string Value { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public int ExternalId { get; set; }
        public Status? Status { get; set; }

        public Payload Payload { get; set; }
        public UrlInfo UrlInfo { get; set; }
    }

    public class UrlConfiguration : IEntityTypeConfiguration<Url>
    {
        public void Configure(EntityTypeBuilder<Url> builder)
        {
            builder.HasIndex(e => e.Value).IsUnique();
        }
    }
}
