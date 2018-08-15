namespace Lykke.Service.PayInternal.Client.Models.File
{
    /// <summary>
    /// Represent a file details.
    /// </summary>
    public class FileInfoModel
    {
        /// <summary>
        /// Gets or sets a file id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a file name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a file mine type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets a file size in bytes.
        /// </summary>
        public int Size { get; set; }
    }
}
