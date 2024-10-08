using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace MonitorSimulator
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient();

        public Form1()
        {
            InitializeComponent();
            client.BaseAddress = new Uri("http://localhost:7237/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            LoadParkingStatuses();
            LoadParkings();
        }

        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            await LoadParkingSlots();
        }

        private async Task LoadParkingStatuses()
        {
            try
            {
                var response = await client.GetAsync("https://localhost:7237/api/ParkingSlot/statuses");
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();
                var option = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var statuses = System.Text.Json.JsonSerializer.Deserialize<List<ParkingSlotStatus>>(responseData, option);

                comboBoxParkingStatus.Items.Clear();

                comboBoxParkingStatus.Items.Add(new ComboBoxItem { Name = "Tutti gli stati", Id = "" });


                foreach (var status in statuses)
                {
                    comboBoxParkingStatus.Items.Add(new ComboBoxItem { Name = status.Name, Id = status.Id.ToString() });
                }

                comboBoxParkingStatus.DisplayMember = "Name";
                comboBoxParkingStatus.ValueMember = "Id";

                if (comboBoxParkingStatus.Items.Count > 0)
                {
                    comboBoxParkingStatus.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento degli stati dei parcheggi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private async Task LoadParkings()
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
                var parkings = System.Text.Json.JsonSerializer.Deserialize<List<Parking>>(responseData, option);

                comboBoxParking.Items.Clear();
                comboBoxParking.Items.Add(new ComboBoxItem { Name = "Tutti i parcheggi", Id = "" });

                foreach (var parking in parkings)
                {
                    comboBoxParking.Items.Add(new ComboBoxItem { Name = parking.Name, Id = parking.Id.ToString() });
                }

                comboBoxParking.DisplayMember = "Name";
                comboBoxParking.ValueMember = "Id";

                if (comboBoxParking.Items.Count > 0)
                {
                    comboBoxParking.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento dei parcheggi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async Task LoadParkingSlots()
        {
            try
            {
                string slotNumber = textBoxSlotNumber.Text.Trim();
                string statusId = (comboBoxParkingStatus.SelectedItem as ComboBoxItem)?.Id;
                string parkingId = (comboBoxParking.SelectedItem as ComboBoxItem)?.Id;


                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(slotNumber))
                {
                    queryParams.Add("number", slotNumber);
                }
                if (!string.IsNullOrEmpty(statusId))
                {
                    queryParams.Add("statusId", statusId);
                }
                if (!string.IsNullOrEmpty(parkingId))
                {
                    queryParams.Add("parkingId", parkingId);
                }

                var requestUrl = QueryHelpers.AddQueryString("https://localhost:7237/api/ParkingSlot", queryParams);

                var response = await client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();

                
                if (jsonString.Trim().StartsWith("[") || jsonString.Trim().StartsWith("{"))
                {
                    var parkingSlots = System.Text.Json.JsonSerializer.Deserialize<List<ParkingSlot>>(jsonString, new JsonSerializerOptions
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
                        DataPropertyName = "Parking.Name" 
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

    public class ComboBoxItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class ParkingSlot
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int ParkingId { get; set; }
        public ParkingSlotStatus Status { get; set; }
        public Parking Parking { get; set; }
    }

    public class Parking
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class ParkingSlotStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

