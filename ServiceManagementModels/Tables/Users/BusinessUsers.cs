using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Users
{
    [Table("business_users")]
    public class BusinessUsers
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("name")]
        public virtual string? Name { get; set; }
        [Column("email")]
        public virtual string? Email { get; set; }
        [Column("password")]
        public virtual string? Password { get; set; }
        [Column("user_role_id")]
        public virtual int UserRoleId { get; set; }
        [Column("designation")]
        public virtual string? Designation { get; set; }
        [Column("line_manager_id")]
        public virtual long LineManagerId { get; set; }
        [Column("heading_1")]
        public virtual string? Heading1 { get; set; }
        [Column("heading_2")]
        public virtual string? Heading2 { get; set; }
        [Column("heading_3")]
        public virtual string? Heading3 { get; set; }
        [Column("heading_4")]
        public virtual string? Heading4 { get; set; }
        [Column("heading_5")]
        public virtual string? Heading5 { get; set; }
        [Column("column_1")]
        public virtual string? Column1 { get; set; }
        [Column("column_2")]
        public virtual string? Column2 { get; set; }
        [Column("column_3")]
        public virtual string? Column3 { get; set; }
        [Column("column_4")]
        public virtual string? Column4 { get; set; }
        [Column("column_5")]
        public virtual string? Column5 { get; set; }
        [Column("password_reset_on")]
        public virtual DateTime? PasswordResetOn { get; set; }
        [Column("created_by")]
        public virtual long CreatedBy { get; set; }
        [Column("created_on")]
        public virtual DateTime CreatedOn { get; set; }
        [Column("last_modified_by")]
        public virtual long LastModifiedBy { get; set; }
        [Column("last_modified_on")]
        public virtual DateTime? LastModifiedOn { get; set; }
        [Column("active")]
        public bool Active { get; set; }
    }
}
