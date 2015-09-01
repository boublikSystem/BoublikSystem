using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace BoublikSystem.HtmlHelpers
{
    public static class PagingHelpers
    {
        public static MvcHtmlString PageLinks(this HtmlHelper helper, string url, int pageCount, int currentPage, string updateId)
        {
            StringBuilder result = new StringBuilder();

            if (pageCount != 1)
            {
                for (int i = 0; i < pageCount; i++)
                {
                    TagBuilder tag = new TagBuilder("a");
                    tag.MergeAttribute("href", string.Format("{0}{1}", url, i));
                    tag.MergeAttribute("data-ajax", "true");
                    tag.MergeAttribute("data-ajax-mode", "replace");
                    tag.MergeAttribute("data-ajax-update", updateId);
                    tag.InnerHtml = (i + 1).ToString();
                    if (i == currentPage)
                    {
                        tag.AddCssClass("selected");
                        tag.AddCssClass("btn-primary");
                    }
                    tag.AddCssClass("btn btn-default");
                    result.Append(tag.ToString());
                }
            }

            return MvcHtmlString.Create(result.ToString());
        }
    }
}

