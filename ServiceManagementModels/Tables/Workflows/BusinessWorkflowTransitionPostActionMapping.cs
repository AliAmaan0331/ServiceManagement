using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Workflows
{
    [Table("business_workflow_transition_post_action_mapping")]
    public class BusinessWorkflowTransitionPostActionMapping
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("business_id")]
        public virtual long BusinessId { get; set; }
        [Column("business_workflow_transition_id")]
        public virtual long BusinessWorkflowTransitionId { get; set; }
        [Column("post_action_id")]
        public virtual long PostActionId { get; set; }
        [Column("active")]
        public bool Active { get; set; }
    }
}
