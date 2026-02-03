using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace RAM.PdfPacker
{
    public static class PdfTools
    {
        public static void AddPdf(PdfDocument target, byte[] blob, ResizerCache cache, PageSize pageSize)
        {
            using var importDoc = PdfReader.Open(new MemoryStream(blob), PdfDocumentOpenMode.Import);

            foreach (PdfPage page in importDoc.Pages)
            {
                if (page.Size != pageSize)
                {
                    Resize(page, pageSize, target, cache);
                }
                else
                {
                    target.AddPage(page);
                }
            }
        }

        public static void AddImage(PdfPage page, byte[] blob)
        {
            var image = XImage.FromStream(new MemoryStream(blob));
            var gfx = XGraphics.FromPdfPage(page);

            var (centerX, centerY) =
                ((page.Width / 2) - image.Size.Width / 2,
                (page.Height / 2) - image.Size.Height / 2);

            gfx.DrawImage(image, centerX, centerY);
        }

        public static void Resize(PdfPage page, PageSize size, PdfDocument target, ResizerCache cache)
        {
            var (pdf, stream) = (cache.SinglePagePdf, cache.SinglePageStream);

            var pageCopy = pdf.AddPage(page);
            pdf.Save(stream);
            stream.Position = 0;

            var scaledPage = target.AddPage();
            scaledPage.Size = size;

            if (page.Width > page.Height)
            {
                scaledPage.Orientation = PageOrientation.Landscape;
            }

            var form = XPdfForm.FromStream(stream);
            var gfx = XGraphics.FromPdfPage(scaledPage);

            gfx.DrawImage(form, new XRect(0, 0, scaledPage.Width, scaledPage.Height));

            pdf.Pages.Remove(pageCopy);
            stream.SetLength(0);
        }
    }
}
