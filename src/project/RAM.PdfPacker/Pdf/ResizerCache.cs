using System.IO;
using PdfSharp.Pdf;

namespace RAM.PdfPacker
{
    public class ResizerCache
    {
        private PdfDocument _singlePagePdf;
        private MemoryStream _singlePageStream;

        public PdfDocument SinglePagePdf => _singlePagePdf ??= new();
        public MemoryStream SinglePageStream => _singlePageStream ??= new();
    }
}
