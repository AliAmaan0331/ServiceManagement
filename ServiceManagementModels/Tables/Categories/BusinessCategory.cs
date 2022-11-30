using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Categories
{
    [Table("business_categories")]
    public class BusinessCategory
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("business_id")]
        public virtual long BusinessId { get; set; }
        [Column("help_desk_id")]
        public virtual long HelpDeskId { get; set; }
        [Column("name")]
        public virtual string? Name { get; set; }
        [Column("deleted_on")]
        public virtual DateTime? DeletedOn { get; set; }
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
