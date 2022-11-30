using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Users
{
    [Table("user_roles")]
    public class UserRole
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("name")]
        public virtual string? Name { get; set; }
        [Column("active")]
        public bool Active { get; set; }
    }
}
