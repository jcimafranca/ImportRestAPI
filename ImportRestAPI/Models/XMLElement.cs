namespace ImportRestAPI.Models
{
    public class XMLElement
    {
        public string Name { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool? Exist { get; set; } = null;
        public bool Start_Tag_Not_Found { get; set; } = false;
        public bool End_Tag_Not_Found { get; set; } = false;
    }
}