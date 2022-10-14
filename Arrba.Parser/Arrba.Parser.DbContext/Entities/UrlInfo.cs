using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrba.Parser.DbContext.Entities
{
    public class UrlInfo
    {
        public int Id { get; set; }
        public string ProviderName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string LastResponseStatus { get; set; }
        public int LinksCount { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }

        public List<Url> Urls { get; set; }
    }

    public class UrlInfoConfiguration : IEntityTypeConfiguration<UrlInfo>
    {
        public void Configure (EntityTypeBuilder<UrlInfo> builder)
        {
            builder.HasIndex(e => e.ProviderName).IsUnique();
        }
    }
}
