using FastHashes;
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
                string? dto = exif.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
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
    
    /// <summary>
    /// Checks if two files are the same (name independent)
    /// </summary>
    /// <param name="path1">The path of the first file</param>
    /// <param name="path2">The path of the second file</param>
    /// <returns><see langword="true"/> if the files are identical, <see langword="false"/> if the are not.</returns>
    public static bool IsSameFile(string path1, string path2)
    {
        try
        {
            IEnumerable<MetadataExtractor.Directory> metadata1 = MetadataExtractor.ImageMetadataReader.ReadMetadata(path1);
            IEnumerable<MetadataExtractor.Directory> metadata2 = MetadataExtractor.ImageMetadataReader.ReadMetadata(path2);
            foreach (var directories in metadata1.Zip(metadata2, Tuple.Create))
            {
                foreach (var tags in directories.Item1.Tags.Zip(directories.Item2.Tags, Tuple.Create))
                {
                    if (tags.Item1.Description != tags.Item2.Description && tags.Item1.Name != "File Name" && tags.Item1.Name != "File Modified Date")
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        catch (MetadataExtractor.ImageProcessingException)
        {
            FarmHash64 hashing = new FarmHash64();
            byte[]? hash1 = hashing.ComputeHash(File.ReadAllBytes(path1));
            byte[]? hash2 = hashing.ComputeHash(File.ReadAllBytes(path2));
            return hash1.SequenceEqual(hash2);
        }
    }

}