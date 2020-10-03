using ClassBookApplication.DataContext;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ClassBookApplication.Service
{
    public class NotificationService 
    {
        #region Variable
        private readonly ClassBookManagementContext _context;
        #endregion

        #region Ctor
        public NotificationService(ClassBookManagementContext context)
        {
            _context = context;
        }
        #endregion

        #region Method
        //public string SendNotificationFromFirebaseCloud(int userId, int statusId, int itemId, int quantity)
        //{
        //    string senderKey = string.Empty;
        //    string toKey = string.Empty;
        //    string severKey = "AAAA7dBWBD8:APA91bFUv8yE_DOeUXGdpdWSPtenFEtDf9-6sZJWVncsQC76bA4hsx2oRsC0g4rwSOOQTrhjwEsOTqCTV7FrDxgCiA73sgI7kVQeUUuzqwt9NBBdezJ1PVhrMybAzEtoW_AwIJU7iywa";
        //    var senderUser = _context.Users.Where(x => x.Id == userId).AsNoTracking().SingleOrDefault();
        //    var adminuser = _context.Users.Where(x => x.Type == "Admin").AsNoTracking().SingleOrDefault();
        //    var item = _context.Item.Where(x => x.Id == itemId).AsNoTracking().SingleOrDefault();
        //    if (senderUser != null)
        //        senderKey = senderUser.TokenKey;
        //    if (adminuser != null)
        //        toKey = adminuser.TokenKey;

        //    var bodyString = senderUser.Username.ToString() + " has  " + Enum.GetName(typeof(Status), statusId).ToString() + " " + quantity + " quantity for " + item.Name.ToString();
        //    var titlePart = Enum.GetName(typeof(Status), statusId).ToString() + " Quantity";
        //    //string senderkey = "dVvsprMj6_k:APA91bFl9xP6ST3zJv1UR7uQ4as5kJZfeGxyn61Q1l_tv4aNdWZyrSR48Z52zbn_NcDOrraoujIgEHq4pbhwnwb1oriNhEZnC-C7wUZnwwUMF6eS9qmTLyTbjtzcGV6NzER1hLluRlTk";
        //    //string toKey = "ei9HpkbBrec:APA91bHr9q1sOkGS3PAsvre0ZmMMxts1fMPpPnyyGbKYe0WVSr9AbjhY4TFerqkOUs_Xjw70qIBbuYiPXuJJ3kRRR4ksqKl227mg9IJMFIYsl6xH0a6AwCi_lcoImKy_RJqAls1Cozmh";
        //    WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
        //    tRequest.Method = "post";
        //    //serverKey - Key from Firebase cloud messaging server  
        //    tRequest.Headers.Add(string.Format("Authorization: key={0}", severKey.ToString()));
        //    //Sender Id - From firebase project setting  
        //    tRequest.Headers.Add(string.Format("Sender: id={0}", senderKey.ToString()));
        //    tRequest.ContentType = "application/json";
        //    var payload = new
        //    {
        //        to = toKey.ToString(),
        //        priority = "high",
        //        content_available = true,
        //        notification = new
        //        {
        //            body = bodyString,
        //            title = titlePart,
        //            badge = 1
        //        },
        //        data = new
        //        {
        //            key1 = "value1",
        //            key2 = "value2"
        //        }
        //    };

        //    string postbody = JsonConvert.SerializeObject(payload).ToString();
        //    Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
        //    tRequest.ContentLength = byteArray.Length;
        //    using (Stream dataStream = tRequest.GetRequestStream())
        //    {
        //        dataStream.Write(byteArray, 0, byteArray.Length);
        //        using (WebResponse tResponse = tRequest.GetResponse())
        //        {
        //            using (Stream dataStreamResponse = tResponse.GetResponseStream())
        //            {
        //                if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
        //                    {
        //                        String sResponseFromServer = tReader.ReadToEnd();
        //                    }
        //            }
        //        }
        //    }
        //    return string.Empty;
        //}

        #endregion
    }
}