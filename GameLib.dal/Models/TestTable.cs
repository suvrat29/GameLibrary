using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace GameLib.dal.Models;

[Table("testtable")]
public class TestTable: BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = "";
    [Column("description")]
    public string Description { get; set; } = "";
    [Column("read_time")]
    public int ReadTime { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("created_by")]
    public Guid CreatedBy { get; set; }
}