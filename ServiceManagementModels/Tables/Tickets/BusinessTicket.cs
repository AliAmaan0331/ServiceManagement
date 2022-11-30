using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Tickets
{
    [Table("business_tickets")]
    public class BusinessTicket
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
        [Column("ticket_type_id")]
        public virtual long TicketTypeId { get; set; }
        [Column("work_flow_id")]
        public virtual long WorkFlowId { get; set; }
        [Column("name")]
        public virtual string? Name { get; set; }
        [Column("description")]
        public virtual string? Description { get; set; }
        [Column("requester_user_id")]
        public virtual long RequesterUserId { get; set; }
        [Column("asignee_user_id")]
        public virtual long AsigneeUserId { get; set; }
        [Column("asignee_group_id")]
        public virtual long AsigneeGroupId { get; set; }
        [Column("status_id")]
        public virtual long StatusId { get; set; }
        [Column("priority_id")]
        public virtual long PriorityId { get; set; }
        [Column("category_id")]
        public virtual long CategoryId { get; set; }
        [Column("resolution_id")]
        public virtual long ResolutionId { get; set; }
        [Column("pending_reason")]
        public virtual string? PendingReason { get; set; }
        [Column("initial_time_estimate")]
        public virtual string? InitialTimeEstimate { get; set; }
        [Column("is_cancelled")]
        public virtual bool IsCancelled { get; set; }
        [Column("cancelled_by")]
        public virtual long CancelledBy { get; set; }
        [Column("cancelled_on")]
        public virtual DateTime? CancelledOn { get; set; }
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
