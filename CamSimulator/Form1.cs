using System;
using System.Collections.Generic;
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
            LoadParkings();
        }

        private async void LoadParkings()
        {
            try
            {
                var response = await client.GetAsync("https://localhost:7237/api/Parking");
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();
                var option = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var parkings = JsonSerializer.Deserialize<List<Parking>>(responseData, option);

                cmbParkings.Items.Clear();

                cmbParkings.DataSource=(parkings);
                

                if (cmbParkings.Items.Count > 0)
                {
                    cmbParkings.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento dei parcheggi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string targa = txtLicensePlate.Text;
            Parking selectedParking = cmbParkings.SelectedItem as Parking;

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
            var targaRequest = new { LicensePlate = targa, ParkingId = parkingId };
            var option = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var jsonContent = JsonSerializer.Serialize(targaRequest, option);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            return await client.PostAsync("https://localhost:7237/api/CamSimulator/arrival", content);
        }

        private void txtLicensePlate_TextChanged(object sender, EventArgs e)
        {

        }
    }

    public class Parking
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public override string ToString()
        {
            return $"{Name}, {Address}, {City}";
        }
    }


}
