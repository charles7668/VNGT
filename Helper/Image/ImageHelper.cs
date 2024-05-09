namespace Helper.Image
{
    public static class ImageHelper
    {
        public static string? ImageFileToBase64(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            if (!File.Exists(filePath))
                return null;

            byte[] imageArray = File.ReadAllBytes(filePath);
            return "data:image/png;base64, " + Convert.ToBase64String(imageArray);
        }
    }
}