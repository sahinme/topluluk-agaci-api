using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.PostTags
{
    public class Tag:BaseEntity,IAggregateRoot
    {
        [MinLength(2)]
        [MaxLength(32)]
        public string Text { get; set; }
        
        public virtual ICollection<PostTag> Posts { get; set; }
    }
}