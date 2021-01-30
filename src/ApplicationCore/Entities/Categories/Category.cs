using System.Collections.Generic;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Entities.PostCategories;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.Categories
{
    public class Category:BaseEntity,IAggregateRoot
    {
        public string DisplayName { get; set; }
        public string Slug { get; set; }
    }
}