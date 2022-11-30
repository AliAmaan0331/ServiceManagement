using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Businesses
{
    [Table("businesses")]
    public class Business
    {
        [Column("Id")]
        public virtual long Id { get; set; }
        [Column("name")]
        public virtual string? Name { get; set; }
        [Column("country_id")]
        public virtual long CountryId { get; set; }
        [Column("state_id")]
        public virtual long StateId { get; set; }
        [Column("city_id")]
        public virtual long CityId { get; set; }
        [Column("time_zone")]
        public virtual string? TimeZone { get; set; }
        [Column("phone_number")]
        public virtual string? PhoneNumber { get; set; }
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
