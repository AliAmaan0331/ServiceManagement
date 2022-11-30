using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Departments
{
    [Table("department_ticket_formats_mapping")]
    public class DepartmentTicketFormatsMapping
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("business_id")]
        public virtual long BusinessId { get; set; }
        [Column("department_id")]
        public virtual long DepartmentId { get; set; }
        [Column("ticket_format_id")]
        public virtual long TicketFormatId { get; set; }
        [Column("active")]
        public bool Active { get; set; }
    }
}
