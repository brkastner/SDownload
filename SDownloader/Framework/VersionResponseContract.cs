using System;
using System.Runtime.Serialization;

namespace SDownload.Framework
{
    #pragma warning disable
    /// <summary>
    /// JSON Response from the SDownload servers containing information for
    /// updating to the newest information if needed
    /// </summary>
    [DataContract]
    public class VersionResponseContract
    {
        /// <summary>
        /// Whether or not the runnning version is up to date
        /// </summary>
        [DataMember(Name = "is_newest_version")]
        internal bool UpToDate;

        /// <summary>
        /// The newest version of SDownload
        /// (Only sent when out of date)
        /// </summary>
        [DataMember(Name = "version")]
        internal String NewestVersion;

        /// <summary>
        /// String representation of changes since the running version
        /// </summary>
        [DataMember(Name = "changelog")]
        internal String Changelog;

        /// <summary>
        /// The URL of the newest version for download
        /// </summary>
        [DataMember(Name = "update_url")]
        internal String UpdateUrl;

        /// <summary>
        /// The URL of the ad to be shown 
        /// (Link to SDownload servers)
        /// </summary>
        [DataMember(Name = "ad_url")]
        internal String AdUrl;
    }
}
