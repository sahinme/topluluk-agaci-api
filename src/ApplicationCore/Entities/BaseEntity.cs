using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Microsoft.Nnn.ApplicationCore.Entities
{
    // This can easily be modified to be BaseEntity<T> and public T Id to support different key types.
    // Using non-generic integer types for simplicity and to ease caching logic
    public class BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        private DateTime? _createdDate = null;
        
        public DateTime ModifiedDate { get; set; }
        
        public string CreatorUserId { get; set; }
        
        public string ModifiedBy { get; set; }

        public string IpAddress { get; set; }
        
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }    
        
        public DateTime CreatedDate
        {
            get
            {
                return _createdDate.HasValue
                    ? _createdDate.Value
                    : DateTime.Now;
            }

            set { _createdDate = value; }
        }
        
    }
}
