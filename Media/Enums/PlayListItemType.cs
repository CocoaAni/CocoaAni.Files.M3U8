namespace CocoaAni.Files.M3U8.Media.Enums;

public enum PlayListItemType
{
    //VOD：即 Video on Demand，表示该视屏流为点播源，因此服务器不能更改该 m3u8 文件；
    VOD,

    //EVENT：表示该视频流为直播源，因此服务器不能更改或删除该文件任意部分内容（但是可以在文件末尾添加新内容）。
    //注：VOD 文件通常带有 EXT-X-ENDLIST 标签，因为其为点播源，不会改变；而 EVEVT 文件初始化时一般不会有 EXT-X-ENDLIST 标签，暗示有新的文件会添加到播放列表末尾，因此也需要客户端定时获取该 m3u8 文件，以获取新的媒体片段资源，直到访问到 EXT-X-ENDLIST 标签才停止）。
    EVENT
}