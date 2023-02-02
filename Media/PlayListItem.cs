using CocoaAni.Files.M3U8.Shared;
using Range = CocoaAni.Files.M3U8.Shared.Range;

namespace CocoaAni.Files.M3U8.Media;

public class PlayListItem
{
    /// <summary>
    /// 可以为十进制的整型或者浮点型，其值必须小于或等于 EXT-X-TARGETDURATION 指定的值。
    /// 注：建议始终使用浮点型指定时长，这可以让客户端在定位流时，减少四舍五入错误。但是如果兼容版本号 EXT-X-VERSION 小于 3，那么必须使用整型。
    /// </summary>
    public float Duration { get; set; }

    /// <summary>
    /// Url地址
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 该标签表示接下来的切片资源是其后 URI 指定的媒体片段资源的局部范围（即截取 URI 媒体资源部分内容作为下一个切片）。
    /// BYTERANGE 标签要求兼容版本号 EXT-X-VERSION 大于等于 4。
    /// </summary>
    public Range? ByteRange { get; set; }

    /// <summary>
    /// 当以下任一情况变化时，必须使用该标签：
    /// 文件格式（file format）
    /// 数字（number），类型（type），媒体标识符（identifiers of tracks）
    /// 时间戳序列（timestamp sequence）
    /// 当以下任一情况变化时，应当使用该标签：
    /// 编码参数（encoding parameters）
    /// 编码序列（encoding sequence）
    /// 注：EXT-X-DISCONTINUITY 的一个经典使用场景就是在视屏流中插入广告，由于视屏流与广告视屏流不是同一份资源，因此在这两种流切换时使用 EXT-X-DISCONTINUITY 进行指明，客户端看到该标签后，就会处理这种切换中断问题，让体验更佳。
    /// </summary>
    public bool IsDiscontinuity { get; set; }

    /// <summary>
    /// EXT-X-KEY：媒体片段可以进行加密，而该标签可以指定解密方法。
    /// 该标签对所有 媒体片段 和 由标签 EXT-X-MAP 声明的围绕其间的所有 媒体初始化块（Meida Initialization Section） 都起作用，直到遇到下一个 EXT-X-KEY（若 m3u8 文件只有一个 EXT-X-KEY 标签，则其作用于所有媒体片段）。
    /// 多个 EXT-X-KEY 标签如果最终生成的是同样的秘钥，则他们都可作用于同一个媒体片段。
    /// 该标签使用格式为：
    /// #EXT-X-KEY:$Key
    /// </summary>
    public PlayListItemKey? Key { get; set; }

    public PlayListItemMap? Map { get; set; }

    /// <summary>
    /// 该标签使用一个绝对日期/时间表明第一个样本片段的取样时间。
    /// </summary>
    public DateTime? ProgramDateTime { get; set; }

    /// <summary>
    /// 该标签定义了一系列由属性/值对组成的日期范围。
    /// 其格式为：
    /// #EXT-X-DATERANGE:$DateRange
    /// </summary>
    public DateRange? DateRange { get; set; }

    /// <summary>
    /// 用于携带 SCET-35 数据。
    /// 该属性为可选参数。
    /// </summary>
    public string? Scte35Cmd { get; set; }

    /// <summary>
    /// 用于携带 SCET-35 数据。
    /// 该属性为可选参数。
    /// </summary>
    public string? Scte35Out { get; set; }

    /// <summary>
    /// 用于携带 SCET-35 数据。
    /// 该属性为可选参数。
    /// </summary>
    public string? Scte35In { get; set; }

    public bool IsEndOnNext { get; set; }
}