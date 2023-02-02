using CocoaAni.Files.M3U8.Exceptions;
using CocoaAni.Files.M3U8.Media;
using CocoaAni.Files.M3U8.Media.Enums;
using CocoaAni.Files.M3U8.Shared;
using Range = CocoaAni.Files.M3U8.Shared.Range;

namespace CocoaAni.Files.M3U8;

public class MediaM3U8 : IM3U8
{
    public static MediaM3U8 Parse(string content)
    {
        var result = new MediaM3U8();
        var lines = content.Split("\n");
        var handleCursor = 0;
        if (lines[handleCursor++] != "#EXTM3U")
        {
            throw new M3U8ParseException("M3u8 File First Line Must = #EXTM3U");
        }

        var playLists = result.PlayLists = new List<PlayListItem>();
        string line;
        var lastByteRange = default(Range?);
        var byteRange = default(Range?);
        var playListItem = new PlayListItem();
        PlayListItemMap? playListItemMap = null;
        PlayListItemKey? playListItemKey = null;
        while ((line = lines[handleCursor++]).StartsWith("#EXT"))
        {
            var tagEndIdx = line.IndexOf(':');
            if (tagEndIdx == -1)
            {
                switch (line)
                {
                    case "#EXT-X-DISCONTINUITY":
                        playListItem.IsDiscontinuity = true;
                        break;

                    case "#EXT-X-I-FRAMES-ONLY":
                        result.IsIframesOnly = true;
                        break;

                    case "#EXT-X-ENDLIST":
                        return result;

                    default:
                        throw new M3U8ParseException($"Error At Line {handleCursor + 1} => [{line}]");
                }
                continue;
            }
            var tag = line[..tagEndIdx];
            var tagValue = line[(tagEndIdx + 1)..];
            try
            {
                string[]? values;
                string[]? props;
                int count;
                int start;
                switch (tag)
                {
                    case "#EXT-X-VERSION":
                        result.Version = int.Parse(tagValue);
                        break;

                    case "#EXT-X-BYTERANGE":
                        count = 0;
                        start = lastByteRange?.Start + 1 ?? 0;
                        if (tagValue.Contains('@'))
                        {
                            values = tagValue.Split('@');
                            if (values.Length != 2)
                            {
                                throw new M3U8ParseException("Range Format Error Should Like <n>[@<o>]");
                            }

                            count = int.Parse(values[0]);
                            start = int.Parse(values[1]);
                        }
                        lastByteRange = byteRange;
                        byteRange = new Range(start, count);
                        break;

                    case "#EXT-X-KEY":
                        playListItemKey = new PlayListItemKey();
                        props = tagValue.Split(',');
                        foreach (var item in props)
                        {
                            var propKeyEndIdx = 0;
                            if ((propKeyEndIdx = item.IndexOf('=')) == 1)
                            {
                                throw new M3U8ParseException(
                                    $"#EXT-X-KEY Format Error At Line {handleCursor} => [{line}]");
                            }

                            var propKey = item[..propKeyEndIdx];
                            var propValue = item[(propKeyEndIdx + 1)..];
                            switch (propKey)
                            {
                                case "METHOD":
                                    playListItemKey.Method = propValue switch
                                    {
                                        "NONE" => PlayListItemKeyMethod.None,
                                        "AES-128" => PlayListItemKeyMethod.Aes128,
                                        "SAMPLE-AES" => PlayListItemKeyMethod.SampleAes,
                                        _ => throw new M3U8ParseException(
                                            $"#EXT-X-KEY.METHOD Format Error At Line {handleCursor} => [{line}]")
                                    };
                                    break;

                                case "URI":
                                    playListItemKey.Uri = propValue;
                                    break;

                                case "IV":
                                    if (propValue.Length != 16)
                                    {
                                        throw new M3U8ParseException(
                                            $"#EXT-X-KEY.URI Length != 16 Error At Line {handleCursor} => [{line}]");
                                    }
                                    playListItemKey.Iv = propValue;
                                    break;

                                case "KEYFORMAT":
                                    if (result.Version < 5)
                                    {
                                        throw new M3U8ParseException(
                                            $"#EXT-X-KEY.KEYFORMAT Version {result.Version} Not Support, Version Must > 5 At Line {handleCursor} => [{line}]");
                                    }
                                    if (!(propValue.StartsWith('"') && propValue.EndsWith('"')))
                                    {
                                        throw new M3U8ParseException(
                                            $"#EXT-X-KEY.KEYFORMAT Format Error At Line {handleCursor} => [{line}]");
                                    }
                                    playListItemKey.KeyFormat = propValue;
                                    break;

                                case "KEYFORMATVERSIONS":
                                    if (result.Version < 5)
                                    {
                                        throw new M3U8ParseException(
                                            $"#EXT-X-KEY.KEYFORMATVERSIONS Version {result.Version} Not Support, Version Must > 5 ! At Line {handleCursor} => [{line}]");
                                    }
                                    var isFindSplitTag = false;
                                    foreach (var c in propValue.ToCharArray())
                                    {
                                        if (!(c <= '0' && c >= 9))
                                        {
                                            isFindSplitTag = false;
                                            continue;
                                        }
                                        if (c == '/' && !isFindSplitTag)
                                        {
                                            isFindSplitTag = true;
                                        }
                                        throw new M3U8ParseException(
                                            $"#EXT-X-KEY.KEYFORMATVERSIONS Format Error! At Line {handleCursor} => [{line}]");
                                    }
                                    playListItemKey.KeyFormatVersions = propValue;
                                    break;
                            }
                        }
                        playListItemKey.KeyFormat ??= "identity";

                        break;

                    case "#EXT-X-MAP":
                        playListItemMap = new PlayListItemMap();
                        props = tagValue.Split(',');
                        foreach (var item in props)
                        {
                            var propKeyEndIdx = 0;
                            if ((propKeyEndIdx = item.IndexOf('=')) == 1)
                            {
                                throw new M3U8ParseException(
                                    $"#EXT-X-KEY Format Error At Line {handleCursor} => [{line}]");
                            }
                            var propKey = item[..propKeyEndIdx];
                            var propValue = item[(propKeyEndIdx + 1)..];
                            switch (propKey)
                            {
                                case "URI":
                                    if (!(propValue.StartsWith('"') && propValue.EndsWith('"')))
                                    {
                                        throw new M3U8ParseException(
                                            $"#EXT-X-MAP.URI Format Error At Line {handleCursor} => [{line}]");
                                    }
                                    playListItemMap.Uri = propValue;
                                    break;

                                case "BYTERANGE":
                                    if (!(propValue.StartsWith('"') && propValue.EndsWith('"')))
                                    {
                                        throw new M3U8ParseException(
                                            $"#EXT-X-MAP.BYTERANGE Format Error At Line {handleCursor} => [{line}]");
                                    }
                                    count = 0;
                                    start = 0;
                                    propValue = propValue[1..^1];
                                    if (propValue.Contains('@'))
                                    {
                                        values = tagValue.Split('@');
                                        if (values.Length != 2)
                                        {
                                            throw new M3U8ParseException("Range Format Error Should Like <n>[@<o>]");
                                        }

                                        count = int.Parse(values[0]);
                                        start = int.Parse(values[1]);
                                    }
                                    playListItemMap.ByteRange = new Range(start, count);
                                    break;
                            }
                        }

                        if (string.IsNullOrEmpty(playListItemMap.Uri))
                        {
                            throw new M3U8ParseException($"#EXT-X-MAP.URI Is Null! At Line {handleCursor} => [{line}]");
                        }
                        break;

                    case "#EXT-X-PROGRAM-DATE-TIME":
                        playListItem.ProgramDateTime = DateTime.Parse(tagValue);
                        break;

                    case "#EXT-X-DATERANGE":
                        playListItem.DateRange ??= new DateRange();
                        props = tagValue.Split(',');
                        foreach (var item in props)
                        {
                            var propKeyEndIdx = 0;
                            if ((propKeyEndIdx = item.IndexOf('=')) == 1)
                            {
                                throw new M3U8ParseException(
                                    $"#EXT-X-KEY Format Error At Line {handleCursor} => [{line}]");
                            }

                            var propKey = item[..propKeyEndIdx];
                            var propValue = item[(propKeyEndIdx + 1)..];
                            switch (propKey)
                            {
                                case "ID":
                                    playListItem.DateRange.Id = propValue;
                                    break;

                                case "CLASS":
                                    playListItem.DateRange.Class = propValue;
                                    break;

                                case "START-DATE":
                                    playListItem.DateRange.StartDate = DateTime.Parse(propValue);
                                    break;

                                case "END-DATE":
                                    playListItem.DateRange.EndDate = DateTime.Parse(propValue);
                                    break;

                                case "DURATION":
                                    playListItem.DateRange.Duration = float.Parse(propValue);
                                    if (playListItem.DateRange.Duration < 0)
                                    {
                                        throw new M3U8ParseException(
                                            $"#EXT-X-DATERANGE.DURATION < 0 ! At Line {handleCursor} => [{line}]");
                                    }
                                    break;

                                case "PLANNED-DURATION":
                                    playListItem.DateRange.PlannedDuration = float.Parse(propValue);
                                    if (playListItem.DateRange.Duration < 0)
                                    {
                                        throw new M3U8ParseException(
                                            $"#EXT-X-DATERANGE.PLANNED-DURATION < 0 ! At Line {handleCursor} => [{line}]");
                                    }
                                    break;
                            }
                        }

                        if (playListItem.DateRange.EndDate != null &&
                            playListItem.DateRange.EndDate < playListItem.DateRange.StartDate)
                        {
                            throw new M3U8ParseException(
                                $"#EXT-X-DATERANGE.END-DATE < START-DATE ! At Line {handleCursor} => [{line}]");
                        }
                        break;

                    case "#SCTE35-CMD":
                        playListItem.Scte35Cmd = tagValue;
                        break;

                    case "#SCTE35-OUT":
                        playListItem.Scte35Out = tagValue;
                        break;

                    case "#SCTE35-IN":
                        playListItem.Scte35In = tagValue;
                        break;

                    case "#END-ON-NEXT":
                        playListItem.IsEndOnNext = true;
                        break;

                    case "#EXTINF":
                        float duration;
                        string? title = null;
                        if (tagValue.EndsWith(','))
                        {
                            if (handleCursor >= lines.Length)
                            {
                                throw new M3U8ParseException(
                                    $"#EXTINF Not Has Title Error At Line {handleCursor} => [{line}]");
                            }

                            duration = float.Parse(tagValue[..^1]);
                            title = lines[handleCursor++];
                        }
                        else
                        {
                            values = tagValue.Split(',');
                            title = values.Length switch
                            {
                                1 => null,
                                2 => values[1],
                                _ => throw new M3U8ParseException(
                                    $"#EXTINF Format Error At Line {handleCursor} => [{line}]"),
                            };
                            duration = float.Parse(values[0]);
                        }
                        if (duration > result.TargetDuration)
                        {
                            throw new M3U8ParseException(
                                $"#EXTINF Duration={duration} > TargetDuration={result.TargetDuration} Error At Line {handleCursor} => [{line}]");
                        }
                        playListItem.Duration = duration;
                        playListItem.Title = title;
                        playListItem.Key = playListItemKey;
                        playListItem.Map = playListItemMap;
                        playLists.Add(playListItem);
                        playListItem = new PlayListItem();
                        break;

                    case "#EXT-X-TARGETDURATION":
                        result.TargetDuration = float.Parse(tagValue);
                        break;
                    //#EXT-X-MEDIA-SEQUENCE
                    case "#EXT-X-MEDIA-SEQUENCE":
                        result.MediaSequence = int.Parse(tagValue);
                        break;

                    case "#EXT-X-DISCONTINUITY-SEQUENCE":
                        result.DiscontinuitySequence = int.Parse(tagValue);
                        break;

                    case "#EXT-X-PLAYLIST-TYPE":
                        result.PlayListType = tagValue switch
                        {
                            "VOD" => PlayListItemType.VOD,
                            "EVENT" => PlayListItemType.EVENT,
                            _ => throw new M3U8ParseException($"#EXT-X-PLAYLIST-TYPE Format Error! At Line {handleCursor} => [{line}]")
                        };
                        break;

                    case "#EXT-X-ALLOW-CACHE":
                        result.AllowCache = tagValue switch
                        {
                            "YES" => true,
                            "NO" => false,
                            _ => throw new M3U8ParseException($"#EXT-X-ALLOW-CACHE Format Error! At Line {handleCursor} => [{line}]")
                        };
                        break;

                    default:
                        throw new M3U8ParseException($"Undefined Tag {tag}");
                }
            }
            catch (M3U8ParseException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new M3U8ParseException($"Error At Line {handleCursor} => [{line}]", e);
            }

            if (handleCursor >= lines.Length)
            {
                throw new M3U8ParseException($"Not Found #EXT-X-ENDLIST Error At Line {handleCursor} => [{line}]");
            }
        }

        return result;
    }

    public static MediaM3U8? TryParse(string content, out M3U8ParseException? ex)
    {
        ex = null;
        try
        {
            return Parse(content);
        }
        catch (M3U8ParseException e)
        {
            ex = e;
        }
        return null;
    }

    public static async Task<MediaM3U8> ParseAsync(Stream stream)
    {
        var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        return Parse(content);
    }

    public static async Task<(MediaM3U8? Value, Exception? Error)> TryParseAsync(Stream stream)
    {
        try
        {
            var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            return (Parse(content), null);
        }
        catch (Exception e)
        {
            return (null, e);
        }
    }

    public MediaM3U8()
    {
    }

    /// <summary>
    /// M3U8文件版本
    /// </summary>
    public int Version { get; set; }

    public bool AllowCache { get; set; }

    /// <summary>
    /// EXT-X-TARGETDURATION：表示每个视频分段最大的时长（单位秒）。
    /// 该标签为必选标签。
    /// 其格式为：
    /// #EXT-X-TARGETDURATION:$TargetDuration
    /// </summary>
    public float TargetDuration { get; set; }

    /// <summary>
    /// EXT-X-MEDIA-SEQUENCE：表示播放列表第一个 URL 片段文件的序列号。
    /// 每个媒体片段 URL 都拥有一个唯一的整型序列号。
    /// 每个媒体片段序列号按出现顺序依次加 1。
    /// 如果该标签未指定，则默认序列号从 0 开始。
    /// 媒体片段序列号与片段文件名无关。
    /// #EXT-X-MEDIA-SEQUENCE:$MediaSequence
    /// </summary>
    public int MediaSequence { get; set; }

    /// <summary>
    /// EXT-X-DISCONTINUITY-SEQUENCE：该标签使能同步相同流的不同 Rendition 和 具备 EXT-X-DISCONTINUITY 标签的不同备份流。
    /// 其格式为：
    /// EXT-X-DISCONTINUITY-SEQUENCE:$DiscontinuitySequence
    /// 其中：参数number为一个十进制整型数值。
    /// 如果播放列表未设置 EXT-X-DISCONTINUITY-SEQUENCE 标签，那么对于第一个切片的中断序列号应当为 0。
    /// </summary>
    public int DiscontinuitySequence { get; set; }

    /// <summary>
    /// EXT-X-ENDLIST：表明 m3u8 文件的结束。
    /// 该标签可出现在 m3u8 文件任意位置，一般是结尾。
    /// 其格式为：
    /// #EXT-X-ENDLIST
    /// </summary>
    public bool IsFindEndList { get; set; }

    /// <summary>
    /// 表明流媒体类型。全局生效。
    /// 该标签为可选标签。
    /// 其格式为：#EXT-X-PLAYLIST-TYPE:$PlayListItemType
    /// </summary>
    public PlayListItemType? PlayListType { get; set; }

    public bool IsIframesOnly { get; set; }

    /// <summary>
    /// 播放列表
    /// </summary>
    public List<PlayListItem> PlayLists { get; set; } = null!;

    //public MediaM3U8(string content)
    //{
    //    var lines = content.Split("\n");
    //    var handleCursor = 0;
    //    if (lines[handleCursor++] != "#EXTM3U")
    //    {
    //        throw new M3U8ParseException("M3u8 File First Line Must = #EXTM3U");
    //    }

    //    var playLists = PlayLists = new List<PlayListItem>();
    //    string line;
    //    var lastByteRange = default(Range?);
    //    var byteRange= default(Range?);
    //    var playListItem = new PlayListItem();
    //    PlayListItemMap? playListItemMap=null;
    //    PlayListItemKey? playListItemKey=null;
    //    while ((line = lines[handleCursor++]).StartsWith("#EXT"))
    //    {
    //        var tagEndIdx = line.IndexOf(':');
    //        if (tagEndIdx == -1)
    //        {
    //            switch (line)
    //            {
    //                case "#EXT-X-DISCONTINUITY":
    //                    playListItem.IsDiscontinuity=true;
    //                    break;
    //                case "#EXT-X-I-FRAMES-ONLY":
    //                    IsIframesOnly = true;
    //                    break;
    //                case "#EXT-X-ENDLIST":
    //                    return;
    //                default:
    //                    throw new M3U8ParseException($"Error At Line {handleCursor + 1} => [{line}]");
    //            }
    //            continue;
    //        }
    //        var tag = line[..tagEndIdx];
    //        var tagValue = line[(tagEndIdx + 1)..];
    //        try
    //        {
    //            string[]? values;
    //            string[]? props;
    //            int count;
    //            int start;
    //            switch (tag)
    //            {
    //                case "#EXT-X-VERSION":
    //                    Version = int.Parse(tagValue);
    //                    break;
    //                case "#EXT-X-BYTERANGE":
    //                    count = 0;
    //                    start = lastByteRange?.Start + 1 ?? 0;
    //                    if (tagValue.Contains('@'))
    //                    {
    //                        values = tagValue.Split('@');
    //                        if (values.Length != 2)
    //                        {
    //                            throw new M3U8ParseException("Range Format Error Should Like <n>[@<o>]");
    //                        }

    //                        count = int.Parse(values[0]);
    //                        start = int.Parse(values[1]);
    //                    }
    //                    lastByteRange = byteRange;
    //                    byteRange = new Range(start, count);
    //                    break;
    //                case "#EXT-X-KEY":
    //                    playListItemKey = new PlayListItemKey();
    //                    props = tagValue.Split(',');
    //                    foreach (var item in props)
    //                    {
    //                        var propKeyEndIdx = 0;
    //                        if ((propKeyEndIdx = item.IndexOf('=')) == 1)
    //                        {
    //                            throw new M3U8ParseException(
    //                                $"#EXT-X-KEY Format Error At Line {handleCursor} => [{line}]");
    //                        }

    //                        var propKey = item[..propKeyEndIdx];
    //                        var propValue = item[(propKeyEndIdx + 1)..];
    //                        switch (propKey)
    //                        {
    //                            case "METHOD":
    //                                playListItemKey.Method = propValue switch
    //                                {
    //                                    "NONE" => PlayListItemKeyMethod.None,
    //                                    "AES-128" => PlayListItemKeyMethod.Aes128,
    //                                    "SAMPLE-AES" => PlayListItemKeyMethod.SampleAes,
    //                                    _ => throw new M3U8ParseException(
    //                                        $"#EXT-X-KEY.METHOD Format Error At Line {handleCursor} => [{line}]")
    //                                };
    //                                break;
    //                            case "URI":
    //                                playListItemKey.Uri=propValue;
    //                                break;
    //                            case "IV":
    //                                if (propValue.Length != 16)
    //                                {
    //                                    throw new M3U8ParseException(
    //                                        $"#EXT-X-KEY.URI Length != 16 Error At Line {handleCursor} => [{line}]");
    //                                }
    //                                playListItemKey.Iv=propValue;
    //                                break;
    //                            case "KEYFORMAT":
    //                                if (Version < 5)
    //                                {
    //                                    throw new M3U8ParseException(
    //                                        $"#EXT-X-KEY.KEYFORMAT Version {Version} Not Support, Version Must > 5 At Line {handleCursor} => [{line}]");
    //                                }
    //                                if (!(propValue.StartsWith('"') && propValue.EndsWith('"')))
    //                                {
    //                                    throw new M3U8ParseException(
    //                                        $"#EXT-X-KEY.KEYFORMAT Format Error At Line {handleCursor} => [{line}]");
    //                                }
    //                                playListItemKey.KeyFormat = propValue;
    //                                break;
    //                            case "KEYFORMATVERSIONS":
    //                                if (Version < 5)
    //                                {
    //                                    throw new M3U8ParseException(
    //                                        $"#EXT-X-KEY.KEYFORMATVERSIONS Version {Version} Not Support, Version Must > 5 ! At Line {handleCursor} => [{line}]");
    //                                }
    //                                var isFindSplitTag = false;
    //                                foreach (var c in propValue.ToCharArray())
    //                                {
    //                                    if (!(c <= '0' && c >= 9))
    //                                    {
    //                                        isFindSplitTag = false;
    //                                        continue;
    //                                    }
    //                                    if (c == '/' && !isFindSplitTag)
    //                                    {
    //                                        isFindSplitTag = true;
    //                                    }
    //                                    throw new M3U8ParseException(
    //                                        $"#EXT-X-KEY.KEYFORMATVERSIONS Format Error! At Line {handleCursor} => [{line}]");
    //                                }
    //                                playListItemKey.KeyFormatVersions = propValue;
    //                                break;
    //                        }
    //                    }
    //                    playListItemKey.KeyFormat ??= "identity";

    //                    break;
    //                case "#EXT-X-MAP":
    //                    playListItemMap = new PlayListItemMap();
    //                    props = tagValue.Split(',');
    //                    foreach (var item in props)
    //                    {
    //                        var propKeyEndIdx = 0;
    //                        if ((propKeyEndIdx = item.IndexOf('=')) == 1)
    //                        {
    //                            throw new M3U8ParseException(
    //                                $"#EXT-X-KEY Format Error At Line {handleCursor} => [{line}]");
    //                        }
    //                        var propKey = item[..propKeyEndIdx];
    //                        var propValue = item[(propKeyEndIdx + 1)..];
    //                        switch (propKey)
    //                        {
    //                            case "URI":
    //                                if (!(propValue.StartsWith('"') && propValue.EndsWith('"')))
    //                                {
    //                                    throw new M3U8ParseException(
    //                                        $"#EXT-X-MAP.URI Format Error At Line {handleCursor} => [{line}]");
    //                                }
    //                                playListItemMap.Uri = propValue;
    //                                break;
    //                            case "BYTERANGE":
    //                                if (!(propValue.StartsWith('"') && propValue.EndsWith('"')))
    //                                {
    //                                    throw new M3U8ParseException(
    //                                        $"#EXT-X-MAP.BYTERANGE Format Error At Line {handleCursor} => [{line}]");
    //                                }
    //                                count = 0;
    //                                start = 0;
    //                                propValue = propValue[1..^1];
    //                                if (propValue.Contains('@'))
    //                                {
    //                                    values = tagValue.Split('@');
    //                                    if (values.Length != 2)
    //                                    {
    //                                        throw new M3U8ParseException("Range Format Error Should Like <n>[@<o>]");
    //                                    }

    //                                    count = int.Parse(values[0]);
    //                                    start = int.Parse(values[1]);
    //                                }
    //                                playListItemMap.ByteRange = new Range(start,count);
    //                                break;
    //                        }
    //                    }

    //                    if (string.IsNullOrEmpty(playListItemMap.Uri))
    //                    {
    //                        throw new M3U8ParseException($"#EXT-X-MAP.URI Is Null! At Line {handleCursor} => [{line}]");
    //                    }
    //                    break;
    //                case "#EXT-X-PROGRAM-DATE-TIME":
    //                    playListItem.ProgramDateTime=DateTime.Parse(tagValue);
    //                    break;
    //                case "#EXT-X-DATERANGE":
    //                    playListItem.DateRange ??= new DateRange();
    //                    props = tagValue.Split(',');
    //                    foreach (var item in props)
    //                    {
    //                        var propKeyEndIdx = 0;
    //                        if ((propKeyEndIdx = item.IndexOf('=')) == 1)
    //                        {
    //                            throw new M3U8ParseException(
    //                                $"#EXT-X-KEY Format Error At Line {handleCursor} => [{line}]");
    //                        }

    //                        var propKey = item[..propKeyEndIdx];
    //                        var propValue = item[(propKeyEndIdx + 1)..];
    //                        switch (propKey)
    //                        {
    //                            case "ID":
    //                                playListItem.DateRange.Id = propValue;
    //                                break;
    //                            case "CLASS":
    //                                playListItem.DateRange.Class = propValue;
    //                                break;
    //                            case "START-DATE":
    //                                playListItem.DateRange.StartDate=DateTime.Parse(propValue);
    //                                break;
    //                            case "END-DATE":
    //                                playListItem.DateRange.EndDate = DateTime.Parse(propValue);
    //                                break;
    //                            case "DURATION":
    //                                playListItem.DateRange.Duration=float.Parse(propValue);
    //                                if (playListItem.DateRange.Duration < 0)
    //                                {
    //                                    throw new M3U8ParseException(
    //                                        $"#EXT-X-DATERANGE.DURATION < 0 ! At Line {handleCursor} => [{line}]");
    //                                }
    //                                break;
    //                            case "PLANNED-DURATION":
    //                                playListItem.DateRange.PlannedDuration = float.Parse(propValue);
    //                                if (playListItem.DateRange.Duration < 0)
    //                                {
    //                                    throw new M3U8ParseException(
    //                                        $"#EXT-X-DATERANGE.PLANNED-DURATION < 0 ! At Line {handleCursor} => [{line}]");
    //                                }
    //                                break;
    //                        }
    //                    }

    //                    if (playListItem.DateRange.EndDate != null &&
    //                        playListItem.DateRange.EndDate < playListItem.DateRange.StartDate)
    //                    {
    //                        throw new M3U8ParseException(
    //                            $"#EXT-X-DATERANGE.END-DATE < START-DATE ! At Line {handleCursor} => [{line}]");
    //                    }
    //                    break;
    //                case "#SCTE35-CMD":
    //                    playListItem.Scte35Cmd = tagValue;
    //                    break;
    //                case "#SCTE35-OUT":
    //                    playListItem.Scte35Out = tagValue;
    //                    break;
    //                case "#SCTE35-IN":
    //                    playListItem.Scte35In = tagValue;
    //                    break;
    //                case "#END-ON-NEXT":
    //                    playListItem.IsEndOnNext = true;
    //                    break;
    //                case "#EXTINF":
    //                    float duration;
    //                    string? title = null;
    //                    if (tagValue.EndsWith(','))
    //                    {
    //                        if (handleCursor >= lines.Length)
    //                        {
    //                            throw new M3U8ParseException(
    //                                $"#EXTINF Not Has Title Error At Line {handleCursor} => [{line}]");
    //                        }

    //                        duration = float.Parse(tagValue[..^1]);
    //                        title = lines[handleCursor++];
    //                    }
    //                    else
    //                    {
    //                        values = tagValue.Split(',');
    //                        title = values.Length switch
    //                        {
    //                            1 => null,
    //                            2 => values[1],
    //                            _ => throw new M3U8ParseException(
    //                                $"#EXTINF Format Error At Line {handleCursor} => [{line}]"),
    //                        };
    //                        duration = float.Parse(values[0]);
    //                    }
    //                    if (duration > TargetDuration)
    //                    {
    //                        throw new M3U8ParseException(
    //                            $"#EXTINF Duration={duration} > TargetDuration={TargetDuration} Error At Line {handleCursor} => [{line}]");
    //                    }
    //                    playListItem.Duration = duration;
    //                    playListItem.Title = title;
    //                    playListItem.Key = playListItemKey;
    //                    playListItem.Map = playListItemMap;
    //                    playLists.Add(playListItem);
    //                    playListItem = new PlayListItem();
    //                    break;
    //                case "#EXT-X-TARGETDURATION":
    //                    TargetDuration=float.Parse(tagValue);
    //                    break;
    //                //#EXT-X-MEDIA-SEQUENCE
    //                case "#EXT-X-MEDIA-SEQUENCE":
    //                    MediaSequence=int.Parse(tagValue);
    //                    break;
    //                case "#EXT-X-DISCONTINUITY-SEQUENCE":
    //                    DiscontinuitySequence=int.Parse(tagValue);
    //                    break;
    //                case "#EXT-X-PLAYLIST-TYPE":
    //                    PlayListType = tagValue switch
    //                    {
    //                        "VOD" => PlayListItemType.VOD,
    //                        "EVENT" => PlayListItemType.EVENT,
    //                        _ => throw new M3U8ParseException($"#EXT-X-PLAYLIST-TYPE Format Error! At Line {handleCursor} => [{line}]")
    //                    };
    //                    break;
    //                case "#EXT-X-ALLOW-CACHE":
    //                    AllowCache = tagValue switch
    //                    {
    //                        "YES" => true,
    //                        "NO" => false,
    //                        _ => throw new M3U8ParseException($"#EXT-X-ALLOW-CACHE Format Error! At Line {handleCursor} => [{line}]")
    //                    };
    //                    break;
    //                default:
    //                    throw new M3U8ParseException($"Undefined Tag {tag}");

    //            }
    //        }
    //        catch (M3U8ParseException ex)
    //        {
    //            throw;
    //        }
    //        catch (Exception e)
    //        {
    //            throw new M3U8ParseException($"Error At Line {handleCursor} => [{line}]",e);
    //        }

    //        if (handleCursor >= lines.Length)
    //        {
    //            throw new M3U8ParseException($"Not Found #EXT-X-ENDLIST Error At Line {handleCursor} => [{line}]");
    //        }
    //    }
    //}
}