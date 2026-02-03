using PX.SM;
using System.IO;

namespace RAM.PdfPacker.Helper
{
    public static class PdfPackerExtensions
    {
        public static void Combine(this PdfPacker packer,  UploadFile fileInfo, UploadFileRevision fileData)
        {
            string filename = PX.SM.FileInfo.GetShortName(fileInfo.Name);
            switch (Path.GetExtension(fileInfo.Name).ToLower())
            {
                case ".pdf":
                    packer.AddPdf(fileData.Data);
                    break;
                case ".png" or ".jpg" or ".jpeg" or "_tiff" or ".gif":
                    packer.AddImage(fileData.Data);
                    break;
            }
        }
    }
}
