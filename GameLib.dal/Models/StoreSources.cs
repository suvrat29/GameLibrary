using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace GameLib.dal.Models;

[Table("store_sources")]
public class StoreSources: BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    [PrimaryKey("uuid", false)]
    public Guid Uuid { get; set; }
    [Column("name")] 
    public string Name { get; set; } = "";
    [Column("icon")] 
    public string Icon { get; set; } = "";
    [Column("created_by")]
    public Guid CreatedBy { get; set; }
    [Column("created_on")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    [Column("deleted")] 
    public bool Deleted { get; set; } = false;
}