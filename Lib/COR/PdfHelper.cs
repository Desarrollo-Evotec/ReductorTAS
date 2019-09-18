using Evotec.KRATA.ReductorTAS.Lib.SYS;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf.security;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evotec.KRATA.ReductorTAS.Lib.COR
{
    public class PdfHelper
    {
        /// <summary>
        /// Comprime las imagenes de un fichero PDF
        /// </summary>
        public static string Resize(string sourceFileName)
        {
            /*
            if (string.IsNullOrWhiteSpace(destinationFileName))
            {
                destinationFileName = sourceFileName;
            }
            */

            string destinationFileName = System.IO.Path.Combine(Globals.GetTempPath, Guid.NewGuid() + System.IO.Path.GetExtension(sourceFileName));

            byte[] _array = null;

            if (!HasSignatures(sourceFileName))
            {
                using (FileStream pdfStream = File.Open(sourceFileName, FileMode.Open))
                using (MemoryStream ms = PDFCompress2(pdfStream))
                {
                    long OriginalSize = pdfStream.Length;

                    if (ms != null)
                    {
                        long ResizeSize = ms.Length;
                        if (ResizeSize < OriginalSize)
                        {
                            //Solo actualizamos si el tamaño final es más pequeño
                            _array = ms.ToArray();
                        }
                    }
                }
            }

            if (_array != null)
            {
                File.WriteAllBytes(destinationFileName, _array);
            }
            else
            {
                if (sourceFileName != destinationFileName)
                {
                    File.Copy(sourceFileName, destinationFileName);
                }
            }

            return destinationFileName;
        }

        /// <summary>
        /// Procesa un PDF
        /// </summary>
        private static MemoryStream PDFCompress2(Stream fileStream)
        {
            BinaryReader br = new BinaryReader(fileStream);
            byte[] byt = br.ReadBytes((int)fileStream.Length);
            MemoryStream ms = new MemoryStream();

            PdfReader pdf = new PdfReader(byt);
            if (pdf.IsOpenedWithFullPermissions)
            {

                PdfStamper stp = new PdfStamper(pdf, ms);
                PdfWriter writer = stp.Writer;
                //
                int page_count = pdf.NumberOfPages;
                for (int i = 1; i <= page_count; i++)
                {
                    PdfDictionary pg = pdf.GetPageN(i);
                    PdfDictionary res = (PdfDictionary)PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES));
                    PdfDictionary xobj = (PdfDictionary)PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT));
                    if (xobj != null)
                    {
                        foreach (PdfName name in xobj.Keys)
                        {
                            PdfObject obj = xobj.Get(name);
                            if (obj.IsIndirect())
                            {
                                PdfDictionary tg = (PdfDictionary)PdfReader.GetPdfObject(obj);
                                if (tg != null)//Veo que a veces que si se trata varias veces la misma imagen esto se vuelve null
                                {
                                    PdfName type = (PdfName)PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE));
                                    if (PdfName.IMAGE.Equals(type))
                                    {
                                        int xrefIdx = ((PRIndirectReference)obj).Number;
                                        PdfObject pdfObj = pdf.GetPdfObject(xrefIdx);
                                        PdfStream str = (PdfStream)pdfObj;
                                        
                                        

                                        string filter = string.Empty;
                                        if (tg.Get(PdfName.FILTER) != null)
                                        {
                                            filter = tg.Get(PdfName.FILTER).ToString();
                                        }
                                        else
                                        {

                                        }

                                        if (filter.Contains("/DCTDecode")) //Unas veces es "[/DCTDecode]" y otras "/DCTDecode"
                                        {
                                            try
                                            {
                                                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance((PRIndirectReference)obj);
                                                //byte[] bytes = PdfReader.GetStreamBytesRaw((PRStream)str);
                                                //System.Drawing.Image imgOriginal = System.Drawing.Image.FromStream(new MemoryStream(bytes));
                                                PdfImageObject pdfImage = new PdfImageObject((PRStream)str);

                                                using (System.Drawing.Image imgOriginal = pdfImage.GetDrawingImage())
                                                using (System.Drawing.Image img2 = Globals.ResizeImage(imgOriginal))
                                                {

                                                    if (img2.Width != imgOriginal.Width || img2.Height != imgOriginal.Height)
                                                    {
                                                        //img2 = Resize(img2, maxImageWidth, maxImageHeight);
                                                        var stream = new System.IO.MemoryStream();
                                                        img2.Save(stream, ImageFormat.Jpeg);
                                                        stream.Position = 0;
                                                        PdfReader.KillIndirect(obj);
                                                        img = iTextSharp.text.Image.GetInstance(stream);

                                                        writer.AddDirectImageSimple(img, (PRIndirectReference)obj);
                                                    }
                                                }
                                            }
                                            catch(Exception ex)
                                            {
                                                throw ex;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                stp.Writer.CloseStream = false;
                stp.FormFlattening = true;
                stp.Close();
                pdf.Close();
                //return ms;
            }
            else
            {
                ms = null;
            }
            return ms;
        }

        /// <summary>
        /// Nos dice si el pdf está firmado
        /// </summary>
        private static bool HasSignatures(String path)
        {
            using (PdfReader reader = new PdfReader(path))
            {
                AcroFields fields = reader.AcroFields;
                List<String> names = fields.GetSignatureNames();
                if (names != null && names.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
