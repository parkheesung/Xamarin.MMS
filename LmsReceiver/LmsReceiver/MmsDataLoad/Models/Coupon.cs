using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MmsDataLoad
{
    public class Coupon
    {
        protected Regex reg_coupon_num = new Regex("([0-9]{4})-([0-9]{4})-([0-9]{4})-([0-9]{4})", RegexOptions.Multiline);

        public string Number { get; set; } = string.Empty;

        public string ID { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string ReceiveType { get; set; } = string.Empty;

        public bool IsBinding { get; set; } = false;

        public Coupon()
        {
        }

        public void Bind()
        {
            if (!string.IsNullOrWhiteSpace(this.Body))
            {
                try
                {
                    MatchCollection numbers = reg_coupon_num.Matches(this.Body);
                    if (numbers != null && numbers.Count > 0)
                    {
                        foreach (Match mm in numbers)
                        {
                            this.Number = mm.Groups[1].Value;
                            break;
                        }
                    }
                }
                catch
                {
                }
            }

            if (!string.IsNullOrWhiteSpace(this.Phone))
            {
                this.IsBinding = true;
            }
        }
    }
}