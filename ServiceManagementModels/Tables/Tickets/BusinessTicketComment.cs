using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Tickets
{
    [Table("business_ticket_comments")]
    public class BusinessTicketComment
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("ticket_id")]
        public virtual long TicketId { get; set; }
        [Column("comment")]
        public virtual string? Comment { get; set; }
        [Column("created_by")]
        public virtual long CreatedBy { get; set; }
        [Column("created_on")]
        public virtual DateTime CreatedOn { get; set; }
        [Column("last_modified_by")]
        public virtual long LastModifiedBy { get; set; }
        [Column("last_modified_on")]
        public virtual DateTime? LastModifiedOn { get; set; }
        [Column("deleted_on")]
        public virtual DateTime? DeletedOn { get; set; }
        [Column("active")]
        public bool Active { get; set; }
    }
}
