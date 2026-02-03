using System.IO;
using PdfSharp;
using PdfSharp.Pdf;

namespace RAM.PdfPacker
{
    public class PdfPacker
    {
        public PdfPacker(PageSize targetPageSize)
        {
            TargetPageSize = targetPageSize;
        }

        public PdfDocument Pack { get; private set; }

        public PageSize TargetPageSize { get; }

        private ResizerCache _resizerCache;

        public void AddPdf(byte[] file)
        {
            var document = EnsurePack();
            PdfTools.AddPdf(document, file, _resizerCache ??= new(), TargetPageSize);
        }

        public void AddImage(byte[] file)
        {
            var page = EnsurePack().AddPage();
            page.Size = TargetPageSize;
            PdfTools.AddImage(page, file);
        }

        public MemoryStream SaveToMemory()
        {
            var blob = new MemoryStream();
            Pack.Save(blob);
            blob.Position = 0;

            return blob;
        }

        private PdfDocument EnsurePack()
        {
            if (Pack == null)
            {
                Pack = new PdfDocument();
                _resizerCache = new ResizerCache();
            }
            return Pack;
        }

    }
}
