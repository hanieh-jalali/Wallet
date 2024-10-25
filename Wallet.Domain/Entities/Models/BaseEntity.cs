using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Domain.Entities.Models
{
    public class BaseEntity
    {
        public string Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string CreateUserId { get; set; }

        public BaseEntity()
        {
            CreateDate = DateTime.UtcNow;
            Id = Guid.NewGuid().ToString();
        }
    }
}
