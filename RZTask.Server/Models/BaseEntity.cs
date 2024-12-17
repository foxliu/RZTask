using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RZTask.Server.Models
{
    public abstract class BaseEntity
    {
        [Required, Column("id")]
        public int Id { get; set; }

        [Required, Column("created_by"), DefaultValue("api")]
        public string CreatedBy { get; set; }

        [Required, Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_by"), DefaultValue("api")]
        public string? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
