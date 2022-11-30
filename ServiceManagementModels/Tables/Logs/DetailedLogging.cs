using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Logs
{
    [Table("detailed_logging")]
    public class DetailedLogging
    {
        [Column("id")]
        public virtual long Id { get; set; }
        [Column("business_id")]
        public virtual long BusinessId { get; set; }
        [Column("description")]
        public virtual string? Description { get; set; }
        [Column("created_by")]
        public virtual long CreatedBy { get; set; }
        [Column("created_on")]
        public virtual DateTime CreatedOn { get; set; }
        [Column("module")]
        public virtual string? Module { get; set; }
    }
}
