namespace CocoaAni.Files.M3U8.Shared;

public class DateRange
{
    /// <summary>
    /// ID：双引号包裹的唯一指明日期范围的标识。
    /// 该属性为必选参数。
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// CLASS：双引号包裹的由客户定义的一系列属性与与之对应的语意值。
    /// 所有拥有同一 CLASS 属性的日期范围必须遵守对应的语意。
    /// 该属性为可选参数。
    /// </summary>
    public string? Class { get; set; }

    /// <summary>
    /// START-DATE：双引号包裹的日期范围起始值。
    /// 该属性为必选参数。
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// END-DATE：双引号包裹的日期范围结束值。
    /// 该属性值必须大于或等于 START-DATE。
    /// 该属性为可选参数。
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// DURATION：日期范围的持续时间是一个十进制浮点型数值类型（单位：秒）。
    /// 该属性值不能为负数。
    /// 当表达立即时间时，将该属性值设为 0 即可。
    /// 该属性为可选参数。
    /// </summary>
    public float? Duration { get; set; }

    /// <summary>
    /// PLANNED-DURATION：该属性为日期范围的期望持续时长。
    /// 其值为一个十进制浮点数值类型（单位：秒）。
    /// 该属性值不能为负数。
    /// 在预先无法得知真实持续时长的情况下，可使用该属性作为日期范围的期望预估时长。
    /// 该属性为可选参数。
    /// </summary>
    public float? PlannedDuration { get; set; }
}