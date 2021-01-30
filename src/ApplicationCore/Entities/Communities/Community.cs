using System;
using System.Collections.Generic;
using Microsoft.Nnn.ApplicationCore.Entities.Categories;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.Communities
{
    public class Community:BaseEntity,IAggregateRoot
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string LogoPath { get; set; }
        public string CoverImagePath { get; set; }
        public Guid CategoryId { get; set; }
        public Category  Category { get; set; }
        public virtual ICollection<CommunityUser> Users { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}