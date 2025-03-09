using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using eSaysay.Data;
using eSaysay.Models.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.RegularExpressions;
using eSaysay.Models;

namespace eSaysay.Controllers
{
    [Route("api/speechassessment")]
    [ApiController]
    [Authorize(Roles = "Student")] 
    public class SpeechAssessmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public SpeechAssessmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitSpeechAssessment([FromForm] IFormFile UserRecording, [FromForm] string ExpectedText)
        {
            if (UserRecording == null || UserRecording.Length == 0)
                return BadRequest(new { message = "No recording uploaded." });

            if (string.IsNullOrEmpty(ExpectedText))
                return BadRequest(new { message = "Expected text is required for assessment." });

            // Validate file type
            var allowedExtensions = new[] { ".mp3", ".wav", ".ogg" };
            var extension = Path.GetExtension(UserRecording.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Invalid file type. Only MP3, WAV, and OGG are allowed." });

            // Limit file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024;
            if (UserRecording.Length > maxFileSize)
                return BadRequest(new { message = "File size exceeds 5MB limit." });

            // Get user ID
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(new { message = "User not found." });

            // Save file
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await UserRecording.CopyToAsync(fileStream);
            }

            // Convert speech-to-text (mocked)
            string transcribedText = ConvertSpeechToText(filePath);

            // Calculate accuracy score
            double accuracyScore = CalculateSpeechAccuracy(ExpectedText, transcribedText);

            // Provide detailed feedback
            string feedback = GenerateFeedback(accuracyScore, transcribedText, ExpectedText);

            // Save assessment
            SpeechAssessment assessment = new SpeechAssessment
            {
                UserID = userId,
                UserRecording = $"/uploads/{uniqueFileName}",
                AccuracyScore = accuracyScore,
                Feedback = feedback,
                AttemptDate = DateTime.UtcNow
            };

            _context.SpeechAssessment.Add(assessment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                AccuracyScore = accuracyScore,
                Feedback = feedback,
                FilePath = assessment.UserRecording,
                TranscribedText = transcribedText
            });
        }

        [HttpGet("history")]
        public IActionResult GetUserAssessments()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(new { message = "User not found." });

            var assessments = _context.SpeechAssessment
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.AttemptDate)
                .Select(a => new
                {
                    a.AssessmentID,
                    a.UserRecording,
                    a.AccuracyScore,
                    a.Feedback,
                    AttemptDate = a.AttemptDate.ToString("yyyy-MM-dd HH:mm")
                })
                .ToList();

            return Ok(assessments);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteRecording(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(new { message = "User not found." });

            var assessment = await _context.SpeechAssessment.FindAsync(id);
            if (assessment == null || assessment.UserID != userId)
                return NotFound(new { message = "Recording not found or unauthorized." });

            // Delete file from storage
            string filePath = Path.Combine(_hostEnvironment.WebRootPath, assessment.UserRecording.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.SpeechAssessment.Remove(assessment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Recording deleted successfully." });
        }

        private string ConvertSpeechToText(string filePath)
        {
            // Simulate AI speech recognition (this should be replaced with an actual STT engine)
            string[] mockTexts = {
                "Hello, welcome to eSaysay! Practice speaking clearly and confidently.",
                "This is a sample speech-to-text transcription.",
                "Your pronunciation and fluency are being assessed."
            };

            Random random = new Random();
            return mockTexts[random.Next(mockTexts.Length)];
        }

        private double CalculateSpeechAccuracy(string expectedText, string spokenText)
        {
            // Normalize text
            expectedText = NormalizeText(expectedText);
            spokenText = NormalizeText(spokenText);

            string[] expectedWords = expectedText.Split(' ');
            string[] spokenWords = spokenText.Split(' ');

            int correctWords = expectedWords.Intersect(spokenWords).Count();
            int totalWords = expectedWords.Length;

            return Math.Round((double)correctWords / totalWords * 100, 2);
        }

        private string NormalizeText(string text)
        {
            return Regex.Replace(text.ToLower(), "[^a-zA-Z0-9 ]", "").Trim();
        }

        private string GenerateFeedback(double score, string spokenText, string expectedText)
        {
            if (score >= 90)
                return $"🌟 Excellent pronunciation! Your words closely match the expected text.";
            else if (score >= 80)
                return $"👍 Great job! Minor mispronunciations detected.";
            else if (score >= 70)
                return $"✅ Good effort! Try to improve pacing and pronunciation.";
            else if (score >= 60)
                return $"🛠 Needs improvement. Focus on clear enunciation.";
            else
                return $"⚠️ Keep practicing! You missed several key words.";
        }
    }
}
