using System;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using Newtonsoft.Json;

namespace contenttracker.Models
{
    public partial class ActualContent
    {
        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        public string Department { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        public string HygienizedDiscipline { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        public string DisciplineId { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        public string DisciplineName { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        public string Unit { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        public string CompetencyAbilities { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        public string Classification { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        public string Content { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        public string DateProdAnt { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        public string DateProdNov { get; set; }

        [IsRetrievable(true)]
        [IsFilterable]
        [IsSortable]
        [IsSearchable]
        [System.ComponentModel.DataAnnotations.Key]
        public string ID { get; set; }
    }
}
