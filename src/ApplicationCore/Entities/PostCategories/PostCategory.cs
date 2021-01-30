using System;
using Microsoft.Nnn.ApplicationCore.Entities.Categories;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.PostCategories
{
    public class PostCategory:BaseEntity,IAggregateRoot
    {
        public Guid PostId { get; set; }
        public Guid CategoryId { get; set; }
        
        public Post Post { get; set; }
        public Category Category { get; set; }
    }
}