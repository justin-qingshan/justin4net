namespace just4net.doc
{
    /// <summary>
    /// PDF Conversion
    /// </summary>
    public interface IPDFConversion
    {
        /// <summary>
        /// Convert document file to pdf file.
        /// </summary>
        /// <param name="sourceFile">File path of document.</param>
        /// <param name="pdfFile">Path of pdf file</param>
        /// <returns></returns>
        int ConvertToPDF(string sourceFile, string pdfFile);
    }
}
