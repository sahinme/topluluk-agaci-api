using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Microsoft.Nnn.ApplicationCore.Services.UserService
{
    public static class Slug
    {
        public static string GenerateSlug(this string phrase)
        {
            string str = phrase.RemoveDiacritics().ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        private static string RemoveDiacritics(this string text)
        {
            var s = new string(text.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
 
            return s.Normalize(NormalizationForm.FormC);
        }
        
        public static string FriendlyUrlTitle(string incomingText)
        {
         
         if (incomingText!=null)
         {
             incomingText = incomingText.Replace("ş", "s");
             incomingText = incomingText.Replace("Ş", "s");
             incomingText = incomingText.Replace("İ", "i");
             incomingText = incomingText.Replace("I", "i");
             incomingText = incomingText.Replace("ı", "i");
             incomingText = incomingText.Replace("ö", "o");
             incomingText = incomingText.Replace("Ö", "o");
             incomingText = incomingText.Replace("ü", "u");
             incomingText = incomingText.Replace("Ü", "u");
             incomingText = incomingText.Replace("Ç", "c");
             incomingText = incomingText.Replace("ç", "c");
             incomingText = incomingText.Replace("ğ", "g");
             incomingText = incomingText.Replace("Ğ", "g");
             incomingText = incomingText.Replace(" ", "-");
             incomingText = incomingText.Replace("---", "-");
             incomingText = incomingText.Replace("?", "");
             incomingText = incomingText.Replace("/", "");
             incomingText = incomingText.Replace(".", "");
             incomingText = incomingText.Replace("'", "");
             incomingText = incomingText.Replace("#", "");
             incomingText = incomingText.Replace("%", "");
             incomingText = incomingText.Replace("&", "");
             incomingText = incomingText.Replace("*", "");
             incomingText = incomingText.Replace("!", "");
             incomingText = incomingText.Replace("@", "");
             incomingText = incomingText.Replace("+", "");
             incomingText = incomingText.ToLower();
             incomingText = incomingText.Trim();
             
            // tüm harfleri küçült
             string encodedUrl = (incomingText ?? "").ToLower();
            // & ile " " yer değiştirme
             encodedUrl = Regex.Replace(encodedUrl, @"\&+", "and");
            // " " karakterlerini silme
             encodedUrl = encodedUrl.Replace("'", "");
            // geçersiz karakterleri sil
            encodedUrl = Regex.Replace(encodedUrl, @"[^a-z0-9]", "-");
            // tekrar edenleri sil
             encodedUrl = Regex.Replace(encodedUrl, @"-+", "-");
            // karakterlerin arasına tire koy
             encodedUrl = encodedUrl.Trim('-');

             encodedUrl = encodedUrl.Substring(0, encodedUrl.Length <= 60 ? encodedUrl.Length : 60).Trim();
             
            return encodedUrl;
         }
         else
         {
             return "";
         }
         
        }
        public static string HtmlToPlainText(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text); 
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }
        
    }
    
    
    
    
}