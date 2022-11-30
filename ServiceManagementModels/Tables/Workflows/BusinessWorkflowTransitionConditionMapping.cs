using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Workflows
{
    [Table("business_workflow_transition_condition_mapping")]
    public class BusinessWorkflowTransitionConditionMapping
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("business_id")]
        public virtual long BusinessId { get; set; }
        [Column("business_workflow_transition_id")]
        public virtual long BusinessWorkflowTransitionId { get; set; }
        [Column("condition_id")]
        public virtual long ConditionId { get; set; }
        [Column("active")]
        public bool Active { get; set; }
    }
}
