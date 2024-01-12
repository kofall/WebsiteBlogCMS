using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBlogCMS.Models
{
    public class DropDownItem
    {
        public string label;
        public int value;
        public bool selected;

        public DropDownItem(string label, int value, bool selected)
        {
            this.label = label;
            this.value = value;
            this.selected = selected;
        }
    }
}