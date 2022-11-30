using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Users
{
    [Table("business_user_groups_mapping")]
    public class BusinessUserGroupsMapping
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("business_id")]
        public virtual long BusinessId { get; set; }
        [Column("user_id")]
        public virtual long UserId { get; set; }
        [Column("group_id")]
        public virtual long GroupId { get; set; }
        [Column("active")]
        public bool Active { get; set; }
    }
}
