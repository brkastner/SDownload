using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SDownload.Framework
{
    #pragma warning disable
    [DataContract]
    public class GithubReleaseItemContract
    {
        [DataMember(Name = "url")]
        internal String Url;

        [DataMember(Name = "html_url")] 
        internal String HtmlUrl;

        [DataMember(Name = "assets_url")]
        internal String AssetsUrl;

        [DataMember(Name = "upload_url")]
        internal String UploadUrl;

        [DataMember(Name = "id")]
        internal int Id;

        [DataMember(Name = "tag_name")]
        internal String TagName;

        [DataMember(Name = "target_commitish")]
        internal String TargetCommitish;

        [DataMember(Name = "name")]
        internal String Name;

        [DataMember(Name = "body")]
        internal String Body;

        [DataMember(Name = "draft")]
        internal bool Draft;

        [DataMember(Name = "prerelease")]
        internal bool PreRelease;

        [DataMember(Name = "created_at")]
        internal String CreatedAt;

        [DataMember(Name = "published_at")]
        internal String PublishedAt;

        [DataMember(Name = "assets")]
        internal GithubReleaseAssetContract[] Assets;
    }

    [DataContract]
    public class GithubReleaseAssetContract
    {
        [DataMember(Name = "url")]
        internal String Url;

        [DataMember(Name = "id")]
        internal int Id;

        [DataMember(Name = "size")]
        internal int Size;
    }
}
