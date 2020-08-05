using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class HelpersMan : MonoBehaviour
{
    public static void ShowPanel(GameObject panel)
    {
        if (panel)
        {
            panel.SetActive(true);
        }
    }
    public static void HidePanel(GameObject panel)
    {
        if (panel)
        {
            panel.SetActive(false);
        }
    }
    public static bool CheckEmpty(InputField inputField)
    {
        if (inputField)
        {
            if (inputField.text == "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    public static bool CheckEmpty(Dropdown dropdown)
    {
        if (dropdown)
        {
            if (dropdown.options[dropdown.value].text == "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    public static bool CheckNumeric(string str)
    {
        try
        {
            double.Parse(str);
            return true;
        }
        catch
        {
            Debug.Log("Please Enter a number ... ");
            return false;
        }
    }
    public static void HighlightPanel(GameObject panel)
    {
        if (panel)
        {
            Color color;
            color = panel.GetComponent<Image>().color;
            color.a = 0.5f;
            panel.GetComponent<Image>().color = color;
        }
    }
    public static void DeHighlightPanel(GameObject panel)
    {
        if (panel)
        {
            Color color;
            color = panel.GetComponent<Image>().color;
            color.a = 0.0f;
            panel.GetComponent<Image>().color = color;
        }
    }
    public static void ShuffleSymbolDirection(Image symbol)
    {
        if (symbol)
        {
            int ch = UnityEngine.Random.Range(1, 5);
            if (ch == 1)
            {
                // Left Direction Rotation 
                symbol.gameObject.GetComponent<RectTransform>().eulerAngles = new Vector3(
                    symbol.gameObject.GetComponent<RectTransform>().eulerAngles.x,
                    symbol.gameObject.GetComponent<RectTransform>().eulerAngles.y,
                    180
                    );
            }
            else if (ch == 2)
            {
                // Right Direction Rotation 
                symbol.gameObject.GetComponent<RectTransform>().eulerAngles = new Vector3(
                    symbol.gameObject.GetComponent<RectTransform>().eulerAngles.x,
                    symbol.gameObject.GetComponent<RectTransform>().eulerAngles.y,
                    0
                    );
            }
            else if (ch == 3)
            {
                // Up Direction Rotation 
                symbol.gameObject.GetComponent<RectTransform>().eulerAngles = new Vector3(
                    symbol.gameObject.GetComponent<RectTransform>().eulerAngles.x,
                    symbol.gameObject.GetComponent<RectTransform>().eulerAngles.y,
                    90
                    );
            }
            else if (ch == 4)
            {
                // Down Direction Rotation 
                symbol.gameObject.GetComponent<RectTransform>().eulerAngles = new Vector3(
                    symbol.gameObject.GetComponent<RectTransform>().eulerAngles.x,
                    symbol.gameObject.GetComponent<RectTransform>().eulerAngles.y,
                    270
                    );
            }
        }
    }
    public static void ScaleDownSymbol(Image symbol)
    {
        if (symbol)
        {
            symbol.gameObject.GetComponent<RectTransform>().localScale = new Vector3(
                symbol.gameObject.GetComponent<RectTransform>().localScale.x-0.1f,
                symbol.gameObject.GetComponent<RectTransform>().localScale.y-0.1f,
                symbol.gameObject.GetComponent<RectTransform>().localScale.z-0.1f
                );
        }
    }
    public static void ResetSymbolSizeAndRotation(Image symbol)
    {
        if (symbol)
        {
            symbol.gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            symbol.gameObject.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
        }
    }
    public static void SmallScaleDownSymbol(Image symbol)
    {
        if (symbol)
        {
            symbol.gameObject.GetComponent<RectTransform>().localScale = new Vector3(
                symbol.gameObject.GetComponent<RectTransform>().localScale.x - 0.05f,
                symbol.gameObject.GetComponent<RectTransform>().localScale.y - 0.05f,
                symbol.gameObject.GetComponent<RectTransform>().localScale.z - 0.05f
                );
        }
    }
    public static void DecreaseSymbolOpacity(Image symbol)
    {
        if (symbol)
        {
            Color temp = symbol.color;
            temp.a -= 0.05f;
            symbol.color = temp;
        }
    }
    public static void ResetSymbolOpacity(Image symbol)
    {
        if (symbol)
        {
            Color temp = symbol.color;
            temp.a = 1;
            symbol.color = temp;
        }
    }
    public static void SendEmail(string msg, string topic, string destinationMail)
    {
        MailMessage mail = new MailMessage();
        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
        SmtpServer.Timeout = 10000;
        SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
        SmtpServer.UseDefaultCredentials = false;
        SmtpServer.Port = 587;

        mail.From = new MailAddress("tmoprojectzc@gmail.com");
        mail.To.Add(new MailAddress(destinationMail));

        mail.Subject = topic;
        mail.Body = msg;


        SmtpServer.Credentials = new System.Net.NetworkCredential("tmoprojectzc@gmail.com", "tmovstzc") as ICredentialsByHost; SmtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        };

        mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
        SmtpServer.Send(mail);
        Debug.Log("Mail Sent ... ");
    }
    public static void SendEmail(string msg, string topic, string destinationMail, Attachment attachment)
    {
        MailMessage mail = new MailMessage();
        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
        SmtpServer.Timeout = 10000;
        SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
        SmtpServer.UseDefaultCredentials = false;
        SmtpServer.Port = 587;

        mail.From = new MailAddress("tmoprojectzc@gmail.com");
        mail.To.Add(new MailAddress(destinationMail));

        mail.Subject = topic;
        mail.Body = msg;
        mail.Attachments.Add(attachment);


        SmtpServer.Credentials = new System.Net.NetworkCredential("tmoprojectzc@gmail.com", "tmovstzc") as ICredentialsByHost; SmtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        };

        mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
        SmtpServer.Send(mail);
        Debug.Log("Mail Sent ... ");
    }
    public static bool CheckSuggestionInFriends(string userId)
    {
        bool res = false;
        for (int i = 0; i < Globals.currentUser.friends.Count; i++)
        {
            if (userId == Globals.currentUser.friends[i].userId)
            {
                res = true;
            }
        }
        return res;
    }
    public static void ClearInputField(InputField inputField)
    {
        if (inputField)
        {
            inputField.text = "";
        }
    }
    public static void ClearText(Text text)
    {
        if (text)
        {
            text.text = "";
        }
    }
    public static void ResetDropDown(Dropdown dd)
    {
        if (dd)
        {
            dd.value = 0;
        }
    }
    public static void ResetToggle(Toggle toggle)
    {
        if (toggle)
        {
            toggle.isOn = false;
        }
    }
    public static string GenerateRandomeID(int length)
    {
        string[] chars = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "a", "s", "d", "f", "g", "h", "j", "k", "l", "z", "x", "c", "v", "b", "n", "m", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        string randomId = "";
        for (int i = 0; i < length; i++)
        {
            randomId += chars[UnityEngine.Random.Range(0, chars.Length)];
        }
        return randomId;
    }
}
