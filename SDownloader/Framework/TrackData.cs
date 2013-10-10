using System;
using System.Runtime.Serialization;

namespace SDownload
{
    #pragma warning disable
    /// <summary>
    /// Soundcloud API response
    /// http://developers.soundcloud.com/docs/api/reference#tracks
    /// </summary>
    [DataContract]
    public class TrackData
    {
        [DataMember(Name = "id")]
        internal int Id;

        [DataMember(Name = "created_at")]
        internal String CreatedAt;

        [DataMember(Name = "user_id")]
        internal int UserId;

        [DataMember(Name = "duration")]
        internal int Duration;

        [DataMember(Name = "commentable")]
        internal bool Commentable;

        [DataMember(Name = "state")]
        internal String State;

        [DataMember(Name = "sharing")]
        internal String Sharing;

        [DataMember(Name = "tag_list")]
        internal String TagList;

        [DataMember(Name = "permalink")]
        internal String Permalink;

        [DataMember(Name = "description")]
        internal String Description;

        [DataMember(Name = "streamable")]
        internal bool CanStream;

        [DataMember(Name = "downloadable")]
        internal bool CanDownload;

        [DataMember(Name = "genre")]
        internal String Genre;

        [DataMember(Name = "release")]
        internal String Release;

        [DataMember(Name = "purchase_url")]
        internal String PurchaseUrl;

        [DataMember(Name = "label_id")]
        internal String LabelId;

        [DataMember(Name = "label_name")]
        internal String LabelName;

        [DataMember(Name = "isrc")]
        internal String Isrc;

        [DataMember(Name = "video_url")]
        internal String VideoUrl;

        [DataMember(Name = "track_type")]
        internal String TrackType;

        [DataMember(Name = "key_signature")]
        internal String KeySignature;

        [DataMember(Name = "bpm")]
        internal String Bpm;

        [DataMember(Name = "title")]
        internal String Title;

        [DataMember(Name = "release_year")]
        internal String ReleaseYear;

        [DataMember(Name = "release_month")]
        internal String ReleaseMonth;

        [DataMember(Name = "release_day")]
        internal String ReleaseDay;

        [DataMember(Name = "original_format")]
        internal String OriginalFormat;

        [DataMember(Name = "original_content_size")]
        internal int OriginalContentSize;

        [DataMember(Name = "license")]
        internal String License;

        [DataMember(Name = "uri")]
        internal String Uri;

        [DataMember(Name = "permalink_url")]
        internal String PermalinkUrl;

        [DataMember(Name = "artwork_url")]
        internal String ArtworkUrl;

        [DataMember(Name = "waveform_url")]
        internal String WaveformUrl;

        [DataMember(Name = "user")]
        internal MiniUser User;

        [DataMember(Name = "stream_url")]
        internal String StreamUrl;

        [DataMember(Name = "download_url")]
        internal String DownloadUrl;

        [DataMember(Name = "playback_count")]
        internal int PlaybackCount;

        [DataMember(Name = "download_count")]
        internal int DownloadCount;

        [DataMember(Name = "favoritings_count")]
        internal int FavoritingsCount;

        [DataMember(Name = "comment_count")]
        internal int CommentCount;

        [DataMember(Name = "created_with")]
        internal CreationInfo CreatedWith;

        [DataMember(Name = "attachments_url")]
        internal String AttachmentsUrl;

        [DataContract]
        internal class CreationInfo
        {
            [DataMember(Name = "id")]
            internal int Id;

            [DataMember(Name = "name")]
            internal String Name;

            [DataMember(Name = "uri")]
            internal String Uri;

            [DataMember(Name = "permalink_url")]
            internal String PermalinkUrl;
        }

        [DataContract]
        internal class MiniUser
        {
            [DataMember(Name = "id")]
            internal int Id;

            [DataMember(Name = "permalink")]
            internal String Permalink;

            [DataMember(Name = "username")]
            internal String Username;

            [DataMember(Name = "uri")]
            internal String Uri;

            [DataMember(Name = "permalink_url")]
            internal String PermalinkUrl;

            [DataMember(Name = "avatar_url")]
            internal String AvatarUrl;
        }
    }
    #pragma warning enable
}
