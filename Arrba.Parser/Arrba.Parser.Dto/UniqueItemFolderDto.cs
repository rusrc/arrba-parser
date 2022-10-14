using Newtonsoft.Json;

namespace Arrba.Parser.Dto
{
    public class UniqueItemFolderDto
    {
        [JsonProperty("uniqueItemFolder")]
        public string UniqueItemFolder { get; set; }
    }
}
