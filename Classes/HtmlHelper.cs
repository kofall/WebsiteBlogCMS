using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace WebsiteBlogCMS.Classes
{
    public static class HtmlHelper
    {
        public static string TruncateHtml(string htmlContent, int maxLength)
        {
            if (string.IsNullOrEmpty(htmlContent) || maxLength <= 0)
                return string.Empty;

            // Remove HTML tags from the content
            string plainText = Regex.Replace(htmlContent, "<.*?>", "");

            // Truncate plain text to the specified length
            if (plainText.Length <= maxLength)
                return htmlContent;

            string truncatedPlainText = plainText.Substring(0, maxLength);

            // Find the last tag in the truncated plain text
            Match lastTagMatch = Regex.Match(truncatedPlainText, "<[^>]+>$");

            if (lastTagMatch.Success)
            {
                // Append the closing tag if necessary
                string closingTag = lastTagMatch.Value;
                truncatedPlainText += closingTag;
            }

            return truncatedPlainText;
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