using System.Text;

namespace ECommerceMVC.Helpers
{
    public class MyUtil
    {
        public static string UploadHinh(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return null;

            // Đường dẫn thư mục lưu hình
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Tên file gốc
            string fileName = Path.GetFileName(file.FileName);
            string fullPath = Path.Combine(path, fileName);

            // Nếu file tồn tại thì đổi tên
            string extension = Path.GetExtension(fileName);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            int i = 1;
            while (System.IO.File.Exists(fullPath))
            {
                fileName = $"{nameWithoutExt}_{i}{extension}";
                fullPath = Path.Combine(path, fileName);
                i++;
            }

            // Lưu file
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Trả về đường dẫn tương đối để lưu DB
            return Path.Combine("Hinh", folder, fileName).Replace("\\", "/");
        }

        public static string GenerateRamdomKey(int length = 5)
        {
            var pattern = @"qazwsxedcrfvtgbyhnujmiklopQAZWSXEDCRFVTGBYHNUJMIKLOP!";
            var sb = new StringBuilder();
            var rd = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(pattern[rd.Next(0, pattern.Length)]);
            }

            return sb.ToString();
        }
    }
}