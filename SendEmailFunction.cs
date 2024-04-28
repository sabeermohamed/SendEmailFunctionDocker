using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Hosting.Server;
using System.Linq;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;
using SendEmailFunction.Model;
using Newtonsoft.Json.Linq;
using System.Text;

namespace SendEmailFunction
{
    public static class SendEmailFunction
    {
        [FunctionName("SendEmailFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            JObject jsonObject = JObject.Parse(requestBody);

            // Extract the productList array from the JSON
            JArray productList = (JArray)jsonObject["order"]["productList"];
            // Create a list to store the product names
            OrderDetails orderDetails = new OrderDetails();

            // Extract the product name and add it to the list
            IList<Product> producList = new List<Product>();
            // Iterate over each product in the productList
            foreach (JToken product in productList)
            {
                Product produc = new Product();   
                produc.productName = (string)product["product"]["productName"];
                produc.productPrice = (long)product["product"]["productPrice"];
                produc.productDescription = (string)product["product"]["productDescription"];
                produc.quantity = (long)product["quantity"];
                produc.price = (long)product["price"];
                orderDetails.totalPrice += produc.price;
                producList.Add(produc);
            }
            orderDetails.products = producList;
            orderDetails.email = (string)(JArray)jsonObject["order"]["emailId"];

            string to = "user_email@gmail.com";
            string subject = "Order Confirmation Email";

            if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(subject))
            {
                return new BadRequestObjectResult("Please provide 'to', 'subject' parameters.");
            }

            StringBuilder OrderDetailHTML = new StringBuilder();
            OrderDetailHTML.Append("<div class=\"order-item\">");
            foreach (var productListHtml in orderDetails.products) 
            {
                OrderDetailHTML.Append($"<p><strong>Product:</strong> {productListHtml.productName}</p>\r\n");
                OrderDetailHTML.Append($"<p><strong>Price: Rs </strong> {productListHtml.productPrice}</p>\r\n");
                OrderDetailHTML.Append($"<p><strong>Quantity:</strong> {productListHtml.quantity}</p>\r\n");
                OrderDetailHTML.Append($"<p><strong>Total: Rs </strong> {productListHtml.price}</p>\r\n");
                OrderDetailHTML.Append($"<br/>");
                OrderDetailHTML.Append($"<hr/>");
            }
            OrderDetailHTML.Append("</div>");
            OrderDetailHTML.Append($"<p><strong>Total Amount: Rs </strong> {orderDetails.totalPrice}</p>");
            // Replace these with your SMTP server details
            string smtpServer = "127.0.0.1";
            int smtpPort = 25;
            //string smtpUsername = "your-smtp-username";
            //string smtpPassword = "your-smtp-password";

            using (var message = new MailMessage())
            {
                message.From = new MailAddress("donot-reply@shopping.com");
                message.To.Add(new MailAddress(to));
                message.Subject = subject;
               

                using (AlternateView htmlview = AlternateView.CreateAlternateViewFromString(ConvertRequestDataToEmailTemplate(OrderDetailHTML),null,"text/html"))
                {
                    message.AlternateViews.Add(htmlview);
                    message.IsBodyHtml = true;
                    using (SmtpClient smtpClient = new SmtpClient())
                    {
                        smtpClient.Host = smtpServer;
                        smtpClient.Port = smtpPort;
                        smtpClient.Send(message);
                    }
                }
            }

            return new OkObjectResult("Email sent successfully!");
        }

        private static string ConvertRequestDataToEmailTemplate(StringBuilder orderDetails)
        {
            string result = string.Empty;
            string mailContent = string.Empty;

            //serverPath = serverPath.EndsWith("\\") == true ? serverPath : serverPath + "\\";
            string binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, "..\\EmailTemplate"));
            var fullFilePath = rootDirectory + "\\orderconfirmation.html";
            //string[] fullFilePath = Directory.GetFiles(binDirectory, filename,SearchOption.AllDirectories);
            using (FileStream stream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    mailContent = sr.ReadToEnd();
                }
            }

            string shoppingImg = rootDirectory + "\\shopping.jpg";

            string backGroundImg = rootDirectory + "\\background.png";
            mailContent = mailContent.Replace("[shoppingImage]", shoppingImg);
            mailContent = mailContent.Replace("[BackgroundImage]", backGroundImg);
            mailContent = mailContent.Replace("[OrderSummaryDetails]", orderDetails.ToString());

            //shopping.jpg
            return result;
        }
    }
}