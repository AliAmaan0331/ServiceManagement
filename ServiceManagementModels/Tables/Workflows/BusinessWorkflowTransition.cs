using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Workflows
{
    [Table("business_workflow_transitions")]
    public class BusinessWorkflowTransition
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("business_id")]
        public virtual long BusinessId { get; set; }
        [Column("work_flow_id")]
        public virtual long WorkFlowId { get; set; }
        [Column("status_id")]
        public virtual long StatusId { get; set; }
        [Column("is_parent_status")]
        public virtual bool IsParentStatus { get; set; }
        [Column("child_status_id")]
        public virtual long ChildStatusId { get; set; }
        [Column("transition_name")]
        public virtual string? TransitionName { get; set; }
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
