using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementUtility.Common
{
    public class Enum
    {
        public enum BusinessTicketStatusCategories : short
        {
            [Description("Open")]
            Open = 1,
            [Description("In Progress")]
            InProgress = 2,
            [Description("On Hold")]
            OnHold = 3,
            [Description("Done")]
            Done = 4
        }

        public enum TicketPriorities : short
        {
            High = 1,
            Medium = 2,
            Low = 3
        }

        public enum Countries : short
        {
            Pakistan = 1,
            India = 2,
            Bangladesh = 3
        }

        public enum States : short
        {
            Sindh = 1,
            Punjab = 2,
            Balochistan = 3,
            KP = 4,
            Gilgit = 5,
            Kashmir = 6
        }

        public enum Cities : short
        {
            Karachi = 1,
            Hyderabad = 2,
            Sukkur = 3,
            Larkana = 4,
            Dadu = 5
        }

        public string? GetDescription(Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            if(fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enumValue.ToString();
        }
    }
}
