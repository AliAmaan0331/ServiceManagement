using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Tickets
{
    [Table("business_ticket_time_tracking")]
    public class BusinessTicketTimeTracking
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("business_id")]
        public virtual long BusinessId { get; set; }
        [Column("help_desk_id")]
        public virtual long HelpDeskId { get; set; }
        [Column("department_id")]
        public virtual long DepartmentId { get; set; }
        [Column("ticket_format_id")]
        public virtual long TicketFormatId { get; set; }
        [Column("ticket_id")]
        public virtual long TicketId { get; set; }
        [Column("work_description")]
        public virtual string? WorkDescription { get; set; }
        [Column("time_spent")]
        public virtual string? TimeSpent { get; set; }
        [Column("started_on")]
        public virtual DateTime? StartedOn { get; set; }
        [Column("logged_by")]
        public virtual long LoggedBy { get; set; }
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
