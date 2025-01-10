using QRCoder;

namespace LibraryManagementSystem.API.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the file", ex);
            }

            return $"/{folderName}/{fileName}";
        }


        public static string GenerateQrCodeAsync(string data)
        {
            // Create a QR code generator instance
            using (QRCodeGenerator qrCodeGenerator = new QRCodeGenerator())
            {
                // Create QR code data from the input string
                using (QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q))
                {
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeImage = qrCode.GetGraphic(20);

                        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BookQRCodes");
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Generate a unique file name for the QR code
                        var qrCodeFileName = Guid.NewGuid().ToString() + ".png";
                        var qrCodePath = Path.Combine(directoryPath, qrCodeFileName);

                        System.IO.File.WriteAllBytesAsync(qrCodePath, qrCodeImage);

                        return $"/BookQRCodes/{qrCodeFileName}"; // Return the QR code file path to save in the database
                    }
                }
            }
        }


        public static void DeleteFile(string filePath)
        {
            if (filePath != null)
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }
    }
}
