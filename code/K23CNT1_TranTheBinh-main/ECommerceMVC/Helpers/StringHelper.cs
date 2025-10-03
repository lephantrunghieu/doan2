using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ECommerceMVC.Helpers
{
    public static class StringHelper
    {
        public static string ToAlias(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            // Bỏ dấu tiếng Việt
            string normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            string noDiacritics = builder.ToString().Normalize(NormalizationForm.FormC);

            // Chuyển về chữ thường
            noDiacritics = noDiacritics.ToLower();

            // Thay khoảng trắng bằng "-"
            noDiacritics = Regex.Replace(noDiacritics, @"\s+", "-");

            // Xóa ký tự đặc biệt
            noDiacritics = Regex.Replace(noDiacritics, @"[^a-z0-9\-]", "");

            return noDiacritics;
        }
    }
}
