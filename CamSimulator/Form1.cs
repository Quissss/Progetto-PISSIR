using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamSimulator
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string targa = txtLicensePlate.Text;

            if (string.IsNullOrWhiteSpace(targa))
            {
                MessageBox.Show("Inserisci una targa", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var response = await InviaTarga(targa);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(responseData, "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Errore nella risposta del server: {response.StatusCode}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Errore di richiesta HTTP: {httpEx.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Si è verificato un errore: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<HttpResponseMessage> InviaTarga(string targa)
        {
            // Crea un oggetto con il dato da inviare
            var targaRequest = new { Targa = targa };

            // Serializza l'oggetto in JSON
            var jsonContent = JsonSerializer.Serialize(targaRequest);

            // Crea il contenuto della richiesta HTTP
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Invia la richiesta POST
            return await client.PostAsync("https://localhost:7237/api/CamSimulator/arrival", content);
        }

        private void txtLicensePlate_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
