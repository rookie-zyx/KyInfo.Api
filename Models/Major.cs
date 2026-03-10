namespace KyInfo.Api.Models;

public class Major
{
    public int Id { get; set; }

    // 专业名称（如 “计算机科学与技术”）
    public string Name { get; set; } = default!;

    // 专业代码（如 “081200”）
    public string Code { get; set; } = default!;

    // 学科门类（如 工学、理学、管理学）
    public string DisciplineCategory { get; set; } = default!;

    // 学位类型：Academic / Professional，或者直接用 “学硕/专硕”
    public string DegreeType { get; set; } = default!;  // "学硕" / "专硕"

    // 学习方式：全日制 / 非全日制
    public string StudyType { get; set; } = "全日制";

    // 学制（年）
    public int DurationYears { get; set; } = 3;

    // 学费（每年），可以先用 decimal?
    public decimal? TuitionPerYear { get; set; }

    // 所属学院/系（如 “计算机学院”）
    public string? SchoolDepartment { get; set; }

    // 培养方向简要说明
    public string? Description { get; set; }

    // 外键：所属院校
    public int SchoolId { get; set; }
    public School School { get; set; } = default!;
}