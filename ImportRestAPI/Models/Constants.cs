namespace ImportRestAPI.Models
{
    internal static class Constants
    {
        internal const string OPENING_TAG_NOT_FOUND = "OPENING_TAG_NOT_FOUND";
        internal const string CLOSING_TAG_NOT_FOUND = "CLOSING_TAG_NOT_FOUND";
        internal const string PROPERTY_NOT_FOUND = "PROPERTY_NOT_FOUND";
        internal const string PROPERTY_FOUND = "PROPERTY_FOUND";
        internal const string UNKNOWN = "UNKNOWN";

        internal const decimal GST_PERCENT = 15;        

        internal const string TOTAL = "total";
        internal const string PAYMENT_METHOD = "payment_method";
        internal const string VENDOR = "vendor";
        internal const string DESCRIPTION = "description";
        internal const string DATE = "date";
        internal const string COST_CENTRE = "cost_centre";
        internal const string FAILED = "Failed";
        internal const string SUCCESS = "Success";

        internal static string XML_CODES = $"{TOTAL},{PAYMENT_METHOD},{VENDOR},{DESCRIPTION},{DATE},{COST_CENTRE}";
    }
}