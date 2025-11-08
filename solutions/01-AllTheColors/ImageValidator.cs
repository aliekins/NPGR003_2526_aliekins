namespace AllTheColors.Validator
{
    public struct ValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorMessage { get; private set; }

        private ValidationResult (bool ok, string msg)
        {
            IsValid = ok;
            ErrorMessage = msg;
        }

        public static ValidationResult Ok ()
        {
            return new ValidationResult(true, "");
        }

        public static ValidationResult Error (string msg)
        {
            return new ValidationResult(false, msg);
        }
    }

    public static class ImageRequestValidator
    {
        public const int AllColorsCount = 256 * 256 * 256;

        public static ValidationResult ValidateDimensions (int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return ValidationResult.Error("width and height must be positive.");
            }

            long pixelCount = (long)width * (long)height;
            if (pixelCount < AllColorsCount)
            {
                return ValidationResult.Error("image too small: needs at least 16777216 pixels (got " + pixelCount + ").");
            }

            return ValidationResult.Ok();
        }
    }
}