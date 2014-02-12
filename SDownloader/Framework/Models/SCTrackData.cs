using System;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace SDownload.Framework.Models
{
    #pragma warning disable
    /// <summary>
    /// Soundcloud API response for a track
    /// <see cref="http://developers.soundcloud.com/docs/api/reference#tracks"/>
    /// </summary>
    [DataContract]
    public class SCTrackData
    {
        /// <summary>
        /// The API client ID for SDownload
        /// </summary>
        public const String ClientId = "4515286ec9d4ace678140c3f84357b35";

        public static SCTrackData LoadData(String url, bool Mock = false)
        {
            return LoadData(url, typeof(SCTrackData), Mock) as SCTrackData;
        }

        internal static object LoadData(String url, Type contractType, bool Mock = false)
        {
            object ret;
            if (!Mock)
            {
                const String resolveUrl = "http://api.soundcloud.com/resolve?url={0}&client_id={1}";
                var request = (HttpWebRequest)WebRequest.Create(String.Format(resolveUrl, url, ClientId));
                request.Method = WebRequestMethods.Http.Get;
                request.Accept = "application/json";
                var response = request.GetResponse().GetResponseStream();
                if (response == null)
                    throw new HandledException(
                        "Soundcloud API failed to respond! This could due to an issue with your connection.");

                ret = new DataContractJsonSerializer(contractType).ReadObject(response);
            }
            else
            {
                // TODO: Load mock data
                ret = new SCTrackData();
            }

            return ret;
        }

        [DataMember(Name = "genre")]
        internal String Genre;

        [DataMember(Name = "title")]
        internal String Title;

        [DataMember(Name = "uri")]
        internal String Uri;

        [DataMember(Name = "permalink_url")]
        internal String PermalinkUrl;

        [DataMember(Name = "artwork_url")]
        internal String ArtworkUrl;

        [DataMember(Name = "user")]
        internal MiniUser User;

        [DataMember(Name = "stream_url")]
        internal String StreamUrl;

        [DataMember(Name = "download_url")]
        internal String DownloadUrl;

        [DataContract]
        internal class MiniUser
        {
            [DataMember(Name = "username")]
            internal String Username;

            [DataMember(Name = "avatar_url")]
            internal String AvatarUrl;
        }
    }

    /// <summary>
    /// Soundcloud API response for a set, only the tracklisting is used
    /// </summary>
    [DataContract]
    public class SCPlaylistData
    {
        public static SCPlaylistData LoadData(String url, bool Mock = false)
        {
            return SCTrackData.LoadData(url, typeof(SCPlaylistData), Mock) as SCPlaylistData;
        }

        [DataMember(Name = "tracks")]
        internal SCTrackData[] Tracks;
    }
    #pragma warning enable
}
