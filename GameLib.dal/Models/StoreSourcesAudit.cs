using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace GameLib.dal.Models;

[Table("store_sources_audit")]
public class StoreSourcesAudit: BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    [Column("created_on")] 
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    [Column("store_sources_ref")] 
    public Guid StoreSourcesUuid { get; set; }
    [Column("created_by")]
    public Guid CreatedBy { get; set; }
    [Column("modified")] 
    public bool Modified { get; set; }
    [Column("deleted")] 
    public bool Deleted { get; set; }
    [Column("name")] 
    public string Name { get; set; } = "";
    [Column("icon")] 
    public string Icon { get; set; } = "";
}