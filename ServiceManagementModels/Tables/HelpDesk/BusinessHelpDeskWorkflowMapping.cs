using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.HelpDesk
{
    [Table("business_help_desk_workflow_mapping")]
    public class BusinessHelpDeskWorkflowMapping
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("business_id")]
        public virtual long BusinessId { get; set; }
        [Column("help_desk_id")]
        public virtual long HelpDeskId { get; set; }
        [Column("workflow_id")]
        public virtual long WorkflowId { get; set; }
        [Column("active")]
        public bool Active { get; set; }
    }
}
