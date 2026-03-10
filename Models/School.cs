namespace KyInfo.Api.Models;

public class School
{
    public int Id { get; set; }

    // 学校名称（如 “清华大学”）
    public string Name { get; set; } = default!;

    // 学校简称（如 “清华”），可选
    public string? ShortName { get; set; }

    // 所在省份 / 直辖市（如 “北京”）
    public string Province { get; set; } = default!;

    // 所在城市（如 “北京市”），可与 Province 合并，看你习惯
    public string City { get; set; } = default!;

    // 院校层次标签：985 / 211 / 双一流 / 普通
    public string LevelTag { get; set; } = default!;

    // 院校类型：综合 / 理工 / 师范 / 农林 / 医药 等
    public string Type { get; set; } = default!;

    // 性质：公办 / 民办 等
    public string Property { get; set; } = default!;

    // 官网链接
    public string? Website { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性：一个学校有多个专业
    public List<Major> Majors { get; set; } = new();
}