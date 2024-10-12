using Progetto.App.Core.Models;
using System.Text;
using System.Text.Json;

namespace CamSimulator
{
    public partial class CamSimulatorForm : Form
    {
        private static readonly HttpClient client = new();

        public CamSimulatorForm()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await LoadParkings();
        }

        private async Task LoadParkings()
        {
            HttpResponseMessage? response = null;
            do
            {
                try
                {
                    response = await client.GetAsync("https://localhost:7237/api/Parking");
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nel caricamento dei parcheggi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } while (response == null);

            var responseData = await response.Content.ReadAsStringAsync();
            var option = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var parkings = JsonSerializer.Deserialize<List<CamParking>>(responseData, option);

            cmbParkings.Items.Clear();
            cmbParkings.DataSource = (parkings);

            if (cmbParkings.Items.Count > 0)
            {
                cmbParkings.SelectedIndex = 0;
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string targa = txtLicensePlate.Text;
            CamParking? selectedParking = cmbParkings.SelectedItem as CamParking;

            if (string.IsNullOrWhiteSpace(targa))
            {
                MessageBox.Show("Inserisci una targa", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedParking == null)
            {
                MessageBox.Show("Seleziona un parcheggio", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var response = await InviaTarga(targa, selectedParking.Id);
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

        private async Task<HttpResponseMessage> InviaTarga(string targa, int parkingId)
        {
            var targaRequest = new { LicencePlate = targa, ParkingId = parkingId };
            var option = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonContent = JsonSerializer.Serialize(targaRequest, option);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            return await client.PostAsync("https://localhost:7237/api/CamSimulator/detect", content);
        }
    }

    public class CamParking : Parking
    {
        public override string ToString() => $"{Name}, {Address}, {City}";
    }
}
