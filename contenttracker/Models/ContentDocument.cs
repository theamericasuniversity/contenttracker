using System;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using Newtonsoft.Json;

namespace contenttracker.Models
{
    public partial class ContentDocument
    {
        [IsRetrievable(true)]
        [IsSearchable]
        public string content { get; set; }

        public string metadata_storage_content_type { get; set; }

        public int metadata_storage_size { get; set; }

        public DateTime metadata_storage_last_modified { get; set; }

        public string metadata_storage_content_md5 { get; set; }

        [IsRetrievable(true)]
        public string metadata_storage_name { get; set; }

        [IsRetrievable(true)]
        [System.ComponentModel.DataAnnotations.Key]
        public string metadata_storage_path { get; set; }

        [IsFilterable]
        public string metadata_content_type { get; set; }

        [IsFilterable]
        public string metadata_language { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSearchable]
        public string metadata_author { get; set; }

        [IsRetrievable(true)]
        public string metadata_title { get; set; }

        public string metadata_creation_date { get; set; }
    }
}
