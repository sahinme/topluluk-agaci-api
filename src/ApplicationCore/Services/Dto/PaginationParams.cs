using System;

namespace Microsoft.Nnn.ApplicationCore.Services.Dto
{
    public class PaginationParams
    {
        public Guid EntityId { get; set; }
        public Guid? UserId { get; set; }
        const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;
 
        private int _pageSize = 10;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
            }
        }
    }
}