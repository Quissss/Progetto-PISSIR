using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Progetto.App.Core.Models;

namespace MonitorSimulator
{
    public partial class MonitorSimulatorForm : Form
    {
        private static readonly HttpClient client = new();

        public MonitorSimulatorForm()
        {
            InitializeComponent();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            LoadParkingStatuses();
            await LoadParkings();
            await LoadParkingSlots();
        }

        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            await LoadParkingSlots();
        }

        private void LoadParkingStatuses()
        {
            comboBoxParkingStatus.Items.Clear();
            comboBoxParkingStatus.Items.Add(new ComboBoxItem { Name = "Tutti gli stati", Id = 0 });

            foreach (var status in Enum.GetValues(typeof(ParkingSlotStatus)))
            {
                comboBoxParkingStatus.Items.Add(new ComboBoxItem { Name = status.ToString(), Id = (int)status });
            }

            comboBoxParkingStatus.DisplayMember = "Name";
            comboBoxParkingStatus.ValueMember = "Id";

            if (comboBoxParkingStatus.Items.Count > 0)
            {
                comboBoxParkingStatus.SelectedIndex = 0;
            }
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
            var parkings = JsonSerializer.Deserialize<List<Parking>>(responseData, option);

            comboBoxParking.Items.Clear();
            comboBoxParking.Items.Add(new ComboBoxItem { Name = "Tutti i parcheggi", Id = 0 });

            foreach (var parking in parkings)
            {
                comboBoxParking.Items.Add(new ComboBoxItem { Name = parking.Name, Id = parking.Id });
            }

            comboBoxParking.DisplayMember = "Name";
            comboBoxParking.ValueMember = "Id";

            if (comboBoxParking.Items.Count > 0)
            {
                comboBoxParking.SelectedIndex = 0;
            }
        }


        private async Task LoadParkingSlots()
        {
            try
            {
                var slotNumber = textBoxSlotNumber.Text.Trim();
                var statusId = (comboBoxParkingStatus.SelectedItem as ComboBoxItem).Id;
                var parkingId = (comboBoxParking.SelectedItem as ComboBoxItem).Id;

                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(slotNumber))
                {
                    queryParams.Add("number", slotNumber);
                }
                if (statusId > 0)
                {
                    queryParams.Add("status", statusId.ToString());
                }
                if (parkingId > 0)
                {
                    queryParams.Add("parkingId", parkingId.ToString());
                }

                var requestUrl = QueryHelpers.AddQueryString("https://localhost:7237/api/ParkingSlot", queryParams);

                var response = await client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();

                if (jsonString.Trim().StartsWith("[") || jsonString.Trim().StartsWith("{"))
                {
                    var parkingSlots = JsonSerializer.Deserialize<List<ParkingSlotWithName>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    dataGridViewParkingSlots.AutoGenerateColumns = false;
                    dataGridViewParkingSlots.Columns.Clear();

                    var slotNumberColumn = new DataGridViewTextBoxColumn
                    {
                        Name = "Slot Number",
                        DataPropertyName = "Number"
                    };
                    dataGridViewParkingSlots.Columns.Add(slotNumberColumn);

                    var parkingColumn = new DataGridViewTextBoxColumn
                    {
                        Name = "Parking",
                        DataPropertyName = "ParkingName"
                    };
                    dataGridViewParkingSlots.Columns.Add(parkingColumn);
                    dataGridViewParkingSlots.DataSource = parkingSlots;
                }
                else
                {
                    MessageBox.Show("Errore durante il caricamento degli slot di parcheggio: Risposta non valida dal server.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento degli slot di parcheggio: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class ParkingSlotWithName : ParkingSlot
    {
        public string ParkingName => Parking != null ? Parking.Name : string.Empty;
    }

    public class ComboBoxItem
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
}

