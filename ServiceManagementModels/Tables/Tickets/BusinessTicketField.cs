﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementModels.Tables.Tickets
{
    [Table("business_ticket_fields")]
    public class BusinessTicketField
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
        [Column("name")]
        public virtual string? Name { get; set; }
        [Column("value")]
        public virtual string? Value { get; set; }
        [Column("value_data_type")]
        public virtual string? ValueDataType { get; set; }
        [Column("is_value_drop_down")]
        public virtual bool IsValueDropDown { get; set; }
        [Column("drop_down_entity")]
        public virtual string? DropDownEntity { get; set; }
        [Column("is_hidden")]
        public virtual bool IsHidden { get; set; }
        [Column("is_required")]
        public virtual bool IsRequired { get; set; }
        [Column("is_editor")]
        public virtual bool IsEditor { get; set; }
        [Column("is_attachment")]
        public virtual bool IsAttachment { get; set; }
        [Column("field_order")]
        public virtual int FieldOrder { get; set; }
        [Column("show_in_create")]
        public virtual bool ShowInCreate { get; set; }
        [Column("show_in_edit")]
        public virtual bool ShowInEdit { get; set; }
        [Column("show_in_view")]
        public virtual bool ShowInView { get; set; }
        [Column("field_type")]
        public virtual string? FieldType { get; set; }
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
