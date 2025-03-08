using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using eSaysay.Models;
using System.Diagnostics;

namespace eSaysay.Controllers
{
    public class CaptchaController : Controller
    {
        private readonly Random _random = new Random();

        public IActionResult Generate()
        {
            // Generate random CAPTCHA text
            string captchaText = GenerateRandomText(5);

            // Store in session for later verification
            HttpContext.Session.SetString("CaptchaCode", captchaText);

            // Generate CAPTCHA image
            using var bitmap = new Bitmap(150, 50);
            using var g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            using Font font = new Font("Arial", 24, FontStyle.Bold);
            using Brush brush = new SolidBrush(Color.Black);
            g.DrawString(captchaText, font, brush, 20, 10);

            // Add some noise
            AddNoise(g, bitmap.Width, bitmap.Height);

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);

            return File(ms.ToArray(), "image/png");
        }

        private string GenerateRandomText(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] text = new char[length];
            byte[] randomBytes = new byte[length];
            RandomNumberGenerator.Fill(randomBytes);
            for (int i = 0; i < length; i++)
            {
                text[i] = chars[randomBytes[i] % chars.Length];
            }
            return new string(text);
        }

        private void AddNoise(Graphics g, int width, int height)
        {
            using var pen = new Pen(Color.Gray);
            for (int i = 0; i < 10; i++)
            {
                int x1 = _random.Next(width);
                int y1 = _random.Next(height);
                int x2 = _random.Next(width);
                int y2 = _random.Next(height);
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
