using System;
using System.Runtime.Serialization;

namespace SDownload.Framework
{
    #pragma warning disable
    /// <summary>
    /// JSON Response from GitHub containing information regarding
    /// SDownload releases. The actual response from the API is an array
    /// of GithubReleaseItemContracts.
    /// </summary>
    [DataContract]
    public class GithubReleaseItemContract
    {
        /// <summary>
        /// The URL  of the release
        /// </summary>
        [DataMember(Name = "url")]
        internal String Url;

        /// <summary>
        /// ID of the release
        /// </summary>
        [DataMember(Name = "id")]
        internal int Id;

        /// <summary>
        /// The version tag name
        /// </summary>
        [DataMember(Name = "tag_name")]
        internal String TagName;

        /// <summary>
        /// The actual name of the release
        /// </summary>
        [DataMember(Name = "name")]
        internal String Name;

        /// <summary>
        /// The description of the release
        /// </summary>
        [DataMember(Name = "body")]
        internal String Body;

        /// <summary>
        /// If the release is still being drafted or is finished
        /// </summary>
        [DataMember(Name = "draft")]
        internal bool Draft;

        /// <summary>
        /// If the release is non-production ready (beta)
        /// </summary>
        [DataMember(Name = "prerelease")]
        internal bool PreRelease;

        /// <summary>
        /// Array containing the executable for installing the newer version
        /// </summary>
        [DataMember(Name = "assets")]
        internal GithubReleaseAssetContract[] Assets;
    }

    /// <summary>
    /// A release asset (always the installer file)
    /// </summary>
    [DataContract]
    public class GithubReleaseAssetContract
    {
        /// <summary>
        /// URL of the asset for download
        /// </summary>
        [DataMember(Name = "url")]
        internal String Url;

        /// <summary>
        /// URL of the asset
        /// </summary>
        [DataMember(Name = "id")]
        internal int Id;

        /// <summary>
        /// Size of the asset
        /// </summary>
        [DataMember(Name = "size")]
        internal int Size;

        /// <summary>
        /// The amount of times the asset has been downloaded
        /// </summary>
        [DataMember(Name = "download_count")]
        internal int DownloadCount;
    }
}
