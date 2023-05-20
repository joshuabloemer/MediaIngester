using MetadataExtractor.Formats.Exif;

namespace MediaIngesterCore;

public static class Utils
{
    /// <summary>
    /// Gets the creation time of a file from embedded Metadata if possible, falls back to the system specific properties if not.  
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>The creation time as <see langword="DateTime"/> object.</returns>
    public static DateTime GetDateTaken(string path)
    {
        // Try to read Creation Time from metadata (only works for supported image formats)
        try
        {
            IEnumerable<MetadataExtractor.Directory> metadata = MetadataExtractor.ImageMetadataReader.ReadMetadata(path);
            // Search all exif subdirs for TagDateTimeOriginal 
            foreach (ExifSubIfdDirectory exif in metadata.OfType<ExifSubIfdDirectory>())
            {
                string? dto = exif?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
                if (dto != null)
                    return DateTime.ParseExact(dto, "yyyy:MM:dd HH:mm:ss", null);
            }
        }
        catch (Exception ex) when (ex is MetadataExtractor.ImageProcessingException || ex is FormatException) { }

        // Fallback to Creation Time and Modification Time for all other file types
        DateTime modifiedTime = File.GetLastWriteTime(path);
        DateTime creationTime = File.GetCreationTime(path);
        return creationTime <= modifiedTime ? creationTime : modifiedTime;
    }
}