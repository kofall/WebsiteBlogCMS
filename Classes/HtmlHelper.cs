using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WebsiteBlogCMS.Classes
{
    public static class HtmlHelper
    {
        public static IHtmlString TruncateHtml(string html, int length)
        {
            if (string.IsNullOrEmpty(html))
                return new HtmlString(string.Empty);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var sb = new StringBuilder();

            foreach (var node in doc.DocumentNode.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element && node.Name.ToLower() == "p")
                {
                    var truncatedText = TruncateParagraph(node, length);
                    sb.Append(truncatedText);
                }
            }

            return new HtmlString(sb.ToString());
        }

        private static string TruncateParagraph(HtmlNode paragraphNode, int length)
        {
            var sb = new StringBuilder();
            var currentLength = 0;

            foreach (var node in paragraphNode.DescendantsAndSelf())
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    var text = node.InnerHtml;

                    if (currentLength + text.Length <= length)
                    {
                        sb.Append(text);
                        currentLength += text.Length;
                    }
                    else
                    {
                        var remainingLength = length - currentLength;
                        sb.Append(text.Substring(0, remainingLength));
                        break;
                    }
                }
                else if (node.NodeType == HtmlNodeType.Element && node.Name.ToLower() == "p")
                {
                    // Skip nested paragraphs
                    continue;
                }
                else
                {
                    sb.Append(node.OuterHtml);
                }
            }

            if (currentLength < length)
                sb.Append("...");

            return sb.ToString();
        }
    }
}