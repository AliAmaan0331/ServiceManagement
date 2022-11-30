using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.TicketFormats
{
    [Table("business_ticket_formats")]
    public class BusinessTicketFormat
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("business_id")]
        public virtual long BusinessId { get; set; }
        [Column("name")]
        public virtual string? Name { get; set; }
        [Column("description")]
        public virtual string? Description { get; set; }
        [Column("logo_file_name")]
        public virtual string? LogoFileName { get; set; }
        [Column("ticket_type_id")]
        public virtual long TicketTypeId { get; set; }
        [Column("work_flow_id")]
        public virtual long WorkFlowId { get; set; }
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
