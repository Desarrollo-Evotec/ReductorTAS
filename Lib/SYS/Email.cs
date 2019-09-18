using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Evotec.KRATA.ReductorTAS.Lib.SYS
{
    public class Email
    {
        private static SmtpClient InitializeSMTP()
        {
            SmtpClient _Smtp = new SmtpClient(Properties.Settings.Default.SMTP_Server, Properties.Settings.Default.SMTP_Port);
            string user = Properties.Settings.Default.SMTP_User;
            string pass = Properties.Settings.Default.SMTP_Pass;
            if (!String.IsNullOrEmpty(user))
            {
                _Smtp.Credentials = new System.Net.NetworkCredential(user, pass);
            }
            return _Smtp;
        }

        public static bool Send(string Subject, string Body, bool IsHTML, List<string> Destinatarios, List<string> DestinatariosEnCopia, List<string> DestinatariosEnCopiaOculta, List<Attachment> Adjuntos = null)
        {
            bool IsOk = false;

            if (Destinatarios == null || Destinatarios.Count == 0 || string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Body))
            {
                //Comprobación de campos obligatorios
                return IsOk;
            }

            SmtpClient _Smtp = InitializeSMTP();

            try
            {
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(Properties.Settings.Default.SMTP_From);



                MailAddress To = null;
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.SMTP_TestMail))
                {
                    To = new MailAddress(Properties.Settings.Default.SMTP_TestMail);
                    msg.To.Add(To);
                }
                else
                {
                    //Metemos destinatarios
                    foreach (var elem in Destinatarios)
                    {
                        To = new MailAddress(elem);
                        msg.To.Add(To);
                    }

                    //Metemos los destinatarios que van en copia
                    if (DestinatariosEnCopia != null)
                    {
                        foreach (var elem in DestinatariosEnCopia)
                        {
                            MailAddress CC = new MailAddress(elem);
                            msg.CC.Add(CC);
                        }
                    }

                    //Metemos los destinatarios que van en copia oculta
                    if (DestinatariosEnCopiaOculta != null)
                    {
                        foreach (var elem in DestinatariosEnCopiaOculta)
                        {
                            MailAddress Bcc = new MailAddress(elem);
                            msg.Bcc.Add(Bcc);
                        }
                    }
                }


                if (msg.To.Count == 0)
                {
                    return IsOk;
                }


                msg.Subject = Subject;
                msg.Body = Body;
                msg.IsBodyHtml = IsHTML;

                if (Adjuntos != null)
                {
                    foreach (Attachment adj in Adjuntos)
                    {
                        msg.Attachments.Add(adj);
                    }
                }


                if (Properties.Settings.Default.SMTP_Enable)
                {
                    _Smtp.Send(msg);
                }

            }
            catch (Exception ex)
            {
                Log.LogServer.WriteLog(ex);
            }

            return IsOk;
        }

        public static void SendOverSizedEmail(string FileName, int Size)
        {
            string Asunto = Properties.Resources.Asunto;
            string Cuerpo = Properties.Resources.Cuerpo;

            Cuerpo = Cuerpo.Replace("[#[NombreTasacion]#]", FileName);
            Cuerpo = Cuerpo.Replace("[#[TamañoActual]#]", Size.ToString());
            Cuerpo = Cuerpo.Replace("[#[TamañoMaximo]#]", Globals.MaxMBSize.ToString());

            Send(Asunto, Cuerpo, true, 
                GetListFromString(Config.Destinatarios), 
                GetListFromString(Config.DestinatariosCopia), 
                GetListFromString(Config.DestinatariosCopiaOculta));

        }

        private static List<string> GetListFromString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            List<string> Destinatarios = text.Split(new char[] { ',', ';' }).ToList();
            return Destinatarios;
        }
    }

}
