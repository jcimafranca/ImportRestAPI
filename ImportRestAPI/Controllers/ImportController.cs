using System;
using System.Web.Http;
using ImportRestAPI.Models;
using System.Collections.Generic;

namespace ImportRestAPI.Controllers
{
    public class ImportController : ApiController
    {        
        private static readonly string[] ELEMENTS = Constants.XML_CODES.Split(',');
        private SummaryOutput SummaryOutput = new SummaryOutput();

        // POST api/import
        [HttpPost]
        public SummaryOutput Post(DataInput raw)
        {
            IList<string> messages = new List<string>();
            try
            {
                if (raw != null && raw.value != null)
                {
                    IList<XMLElement> ListOfXMLElements = Extract(raw.value);
                    SummaryOutput = Validate(ListOfXMLElements);
                }
                else
                {
                    if (raw == null)
                        messages.Add("The data is not in JSON format.");
                    if (raw != null && raw.value == null)
                        messages.Add("The 'value' property is missing in the JSON string.");

                    SummaryOutput.results = Constants.FAILED;
                    SummaryOutput.message = messages;
                }
            }
            catch (Exception ex)
            {
                messages.Add(ex.Message);
                SummaryOutput.results = Constants.FAILED;
                SummaryOutput.message = messages;
                return SummaryOutput;
            }

            return SummaryOutput;
        }
        private IList<XMLElement> Extract(string raw_data)
        {
            IList<XMLElement> list = new List<XMLElement>();
            foreach (string name in ELEMENTS)
            {
                XMLElement element = new XMLElement { Name = name };
                element = Search(raw_data, element);

                if (element.Exist != null && element.Exist.Value)
                {
                    element.Value = GetValue(raw_data, element.Name);
                    if (string.IsNullOrEmpty(element.Value) && element.Name == Constants.COST_CENTRE)
                    {
                        element.Value = Constants.UNKNOWN;
                    }
                    if (string.IsNullOrEmpty(element.Value) && element.Name == Constants.TOTAL)
                    {
                        element.Value = "0";
                    }
                }
                if (element.Exist != null && !element.Exist.Value)
                    element.Value = Constants.UNKNOWN;
                if (element.Exist == null && element.Start_Tag_Not_Found)
                    element.Value = Constants.OPENING_TAG_NOT_FOUND;
                if (element.Exist == null && element.End_Tag_Not_Found)
                    element.Value = Constants.CLOSING_TAG_NOT_FOUND;

                list.Add(element);
            }

            return list;
        }

        private XMLElement Search(string raw_data, XMLElement element)
        {
            string start_tag = string.Concat("<", element.Name, ">");
            bool start_tag_found = raw_data.Contains(start_tag);

            string end_tag = string.Concat("</", element.Name, ">");
            bool end_tag_found = raw_data.Contains(end_tag);

            if (!start_tag_found && end_tag_found) element.Start_Tag_Not_Found = true;
            if (start_tag_found && !end_tag_found) element.End_Tag_Not_Found = true;
            if (!start_tag_found && !end_tag_found) element.Exist = false;
            if (start_tag_found && end_tag_found) element.Exist = true;

            return element;
        }

        private string GetValue(string raw_data, string xml_name)
        {
            string returnValue = string.Empty;
            int startIndex = raw_data.Trim().IndexOf("<" + xml_name + ">");
            int lastIndex = raw_data.Trim().LastIndexOf("</" + xml_name + ">");
            string value = raw_data.Trim().Substring(startIndex, (lastIndex - startIndex));
            returnValue = value.Replace("<" + xml_name + ">", string.Empty);
            return returnValue;
        }

        private SummaryOutput Validate(IList<XMLElement> list)
        {
            SummaryOutput output = new SummaryOutput();
            DataExtracted data = new DataExtracted();
            IList<string> messages = new List<string>();

            foreach (XMLElement xml in list)
            {
                if (xml.Name == Constants.TOTAL && xml.Value == Constants.UNKNOWN)
                {
                    output.results = Constants.FAILED;
                    messages.Add("The whole data import is rejected because of a missing <" + xml.Name + "> element.");
                    break;
                }
                else if (xml.Value == Constants.OPENING_TAG_NOT_FOUND)
                {
                    output.results = Constants.FAILED;
                    messages.Add("The whole data import is rejected because a closing tag </" + xml.Name + "> has no corresponding opening tag.");
                    break;
                }
                else if (xml.Value == Constants.CLOSING_TAG_NOT_FOUND)
                {
                    output.results = Constants.FAILED;
                    messages.Add("The whole data import is rejected because an opening tag <" + xml.Name + "> has no corresponding closing tag.");
                    break;
                }
                else if (xml.Name == Constants.COST_CENTRE && xml.Value == Constants.UNKNOWN)
                {
                    messages.Add("The whole data import is succesful eventhough <" + xml.Name + "> is missing. the value is set to unknown.");
                }

                if (string.IsNullOrEmpty(output.results))
                {
                    data = Save(xml.Name, (xml.Value == Constants.UNKNOWN && xml.Name != Constants.COST_CENTRE) ? null : xml.Value, data);
                }
            }

            if (output.results == Constants.FAILED)
            {
                if (messages.Count == 0)
                    messages.Add("The whole data import is rejected.");
            }
            else
            {
                output.data = new DataOutput
                {
                    extracted = data,
                    calculated = Calculate(data.total)
                };

                if (string.IsNullOrEmpty(output.results))
                    output.results = Constants.SUCCESS;

                if (messages.Count == 0)
                    messages.Add("The whole data import is succesful.");
            }

            output.message = messages;

            return output;
        }

        private DataExtracted Save(string xml_name, string value, DataExtracted current)
        {
            if (xml_name == Constants.TOTAL) current.total = value;
            if (xml_name == Constants.COST_CENTRE) current.cost_centre = value;
            if (xml_name == Constants.PAYMENT_METHOD) current.payment_method = value;
            if (xml_name == Constants.VENDOR) current.vendor = value;
            if (xml_name == Constants.DESCRIPTION) current.description = value;
            if (xml_name == Constants.DATE) current.date = value;
            return current;
        }

        private DataCalculated Calculate(string str_total)
        {
            DataCalculated calculate = new DataCalculated();

            decimal.TryParse(str_total, out decimal total);
            calculate.GST = Math.Round(total * (Constants.GST_PERCENT / 100), 2);
            calculate.totalexcludingGST = Math.Round(total - calculate.GST, 2);

            return calculate;
        }

    }
}
