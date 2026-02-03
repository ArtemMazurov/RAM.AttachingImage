using System.IO;
using System.Linq;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RAM.PdfPacker;

namespace Tests
{
    [TestClass]
    public class PdfPackerTests
    {
        [TestMethod]
        [DataRow("letter-img-pdf", PageSize.Letter, "IMG.jpg", "S100.pdf")]
        [DataRow("a1-img-pdf", PageSize.A1, "IMG.jpg", "S100.pdf")]
        public void OutputContainsSameAmountOfPagesAndOfTheTargetSize(string outFileName, PageSize size, params string[] files)
        {
            var pack = CreatePack(size, files);

            Assert.IsNotNull(pack.Pack);

            foreach (PdfPage page in pack.Pack.Pages)
            {
                Assert.AreEqual(size, page.Size);
            }

            Assert.AreEqual(SumPageCount(files), pack.Pack.PageCount);

            File.WriteAllBytes(Path.Combine(@"..\..\out", $"{outFileName}.pdf"), pack.SaveToMemory().ToArray());
        }

        private static PdfPacker CreatePack(PageSize size, params string[] files)
        {
            var packer = new PdfPacker(size);

            foreach (var filePath in files)
            {
                var file = ReadFile(filePath);
                if (Path.GetExtension(filePath) == ".pdf")
                {
                    packer.AddPdf(file);
                }
                else
                {
                    packer.AddImage(file);
                }
            }

            return packer;
        }

        private static int SumPageCount(params string[] files) => files
            .Select(file =>
            {
                if (Path.GetExtension(file) == ".pdf")
                {
                    using var pdf = PdfReader.Open(new MemoryStream(ReadFile(file)), PdfDocumentOpenMode.Import);
                    return pdf.PageCount;
                }

                return 1;
            })
            .Sum();

        private static byte[] ReadFile(string name) => File.ReadAllBytes(Path.Combine(@"..\..\data", name));
    }
}