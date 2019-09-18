using Evotec.KRATA.ReductorTAS.Lib.SYS;
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
    public class ImageHelper
    {
        /// <summary>
        /// Comprime una imagen
        /// </summary>
        public static string Resize(string sourcePath)
        {
            /*
            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                destinationPath = sourcePath;
            }
            */
            string destinationPath = System.IO.Path.Combine(Globals.GetTempPath, Guid.NewGuid() + System.IO.Path.GetExtension(sourcePath));

            using (Image imgOriginal = Image.FromFile(sourcePath))
            using (Image imgTratada = Globals.ResizeImage(imgOriginal))
            {
                if (imgOriginal.Width != imgTratada.Width || imgOriginal.Height != imgTratada.Height)
                {
                    imgOriginal.Dispose();

                    string ext = Path.GetExtension(sourcePath).ToLower();

                    ImageCodecInfo imgEncoder = null;
                    EncoderParameters myEncoderParameters = null;
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    myEncoderParameters = new EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 90L);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    if (ext == ".png")
                    {
                        imgEncoder = GetEncoder(ImageFormat.Png);
                    }
                    else if (ext == ".tiff")
                    {
                        imgEncoder = GetEncoder(ImageFormat.Tiff);
                    }
                    else //if (ext == ".jpg" || ext == ".jpeg")
                    {
                        imgEncoder = GetEncoder(ImageFormat.Jpeg);

                    }

                    //Prueba para guardar solo si menor
                    //long jpegByteSize;
                    //using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                    //using (var ms = new MemoryStream())
                    //{
                    //    if (imgEncoder != null)
                    //    {
                    //        imgTratada.Save(ms, imgEncoder, myEncoderParameters);
                    //    }
                    //    else
                    //    {
                    //        imgTratada.Save(ms, ImageFormat.Jpeg);
                    //    }
                    //    jpegByteSize = ms.Length;
                    //    ms.CopyTo(fileStream);
                    //}

                    if (imgEncoder != null)
                    {
                        imgTratada.Save(destinationPath, imgEncoder, myEncoderParameters);
                    }
                    else
                    {
                        imgTratada.Save(destinationPath);
                    }

                }
            }
            return destinationPath;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
