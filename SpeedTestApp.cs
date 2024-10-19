using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Windows.Forms;

namespace SpeedTestApp
{
    public partial class MainForm : Form
    {
        private static readonly HttpClient client = CreateHttpClient();

        public MainForm()
        {
            InitializeComponent();
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (HttpRequestMessage req, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors) => true
            };
            return new HttpClient(handler);
        }


        private async Task<double> MeasureDownloadSpeed()
        {
            var stopwatch = Stopwatch.StartNew();

            var response = await client.GetAsync("https://github.com/3xx/SpeedTestApp/releases/download/SpeedTest/download.bin");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Download failed: " + response.StatusCode);
            }

            var content = await response.Content.ReadAsByteArrayAsync();
            stopwatch.Stop();

       
            double speedInMbps = (content.Length * 8) / (stopwatch.Elapsed.TotalSeconds * 1_000_000);
            return speedInMbps;
        }

        private async Task<double> MeasureUploadSpeed()
        {
            var stopwatch = Stopwatch.StartNew();
            byte[] data = new byte[25 * 1024 * 1024]; 
            var content = new ByteArrayContent(data);
            var response = await client.PostAsync("https://httpbin.org/post", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Upload failed: " + response.StatusCode);
            }

            stopwatch.Stop();

            // حساب سرعة الرفع
            double speedInMbps = (data.Length * 8) / (stopwatch.Elapsed.TotalSeconds * 1_000_000);
            return speedInMbps;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private async void btnTestSpeed_Click_1(object sender, EventArgs e)
        {
            lblDownloadSpeed.Text = "Measuring download speed...";
            lblUploadSpeed.Text = "Measuring upload speed...";

            try
            {
                var downloadSpeed = await MeasureDownloadSpeed();
                lblDownloadSpeed.Text = $"Download Speed: {downloadSpeed:F2} Mbps";

                var uploadSpeed = await MeasureUploadSpeed();
                lblUploadSpeed.Text = $"Upload Speed: {uploadSpeed:F2} Mbps";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error measuring speed: {ex.Message}");
            }
        }
    }
}
