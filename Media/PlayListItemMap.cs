using Range = CocoaAni.Files.M3U8.Shared.Range;

namespace CocoaAni.Files.M3U8.Media;

public class PlayListItemMap
{
    /// <summary>
    /// URI：由引号包裹的字符串，指定了包含媒体初始化块的资源的路径。该属性为必选参数。
    /// </summary>
    public string Uri { get; set; } = null!;

    /// <summary>
    /// BYTERANGE：由引号包裹的字符串，指定了媒体初始化块在 URI 指定的资源的位置（片段）。
    /// 该属性指定的范围应当只包含媒体初始化块。
    /// 该属性为可选参数，如果未指定，则表示 URI 指定的资源就是全部的媒体初始化块。
    /// </summary>
    public Range? ByteRange { get; set; } = null!;
}