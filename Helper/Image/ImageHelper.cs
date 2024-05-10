namespace Helper.Image
{
    public static class ImageHelper
    {
        public static string GetDisplayUrl(string? imageLink)
        {
            if (imageLink == null)
                return "";
            if (imageLink.IsHttpLink())
                return imageLink;
            return ImageFileToBase64(imageLink) ?? "";
        }

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