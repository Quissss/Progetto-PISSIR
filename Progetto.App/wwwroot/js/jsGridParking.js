const url = "/api/Parking";

let ajax = function (item, verb, json = true) {
    return $.ajax({
        type: verb,
        url: url,
        data: json ? JSON.stringify(item) : item,
        dataType: "json",
        contentType: json ? "application/json" : "text/plain",
    });
};

$(function () {
    let map = L.map('map').setView([45.4642, 9.1900], 13); // Default to Milan, Italy

    // Set up the OSM layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
    }).addTo(map);

    $("#parkingGrid").jsGrid({
        width: "100%",
        height: "400px",
        autoload: true,
        filtering: true,
        inserting: true,
        editing: true,
        sorting: true,
        paging: true,
        controller: {
            loadData: filter => ajax(filter, "GET", false),
            updateItem: item => ajax(item, "PUT"),
            deleteItem: item => ajax(item, "DELETE"),
            insertItem: item => ajax(item, "POST"),
        },

        data: parkings,
        fields: [
            { name: "id", visible: false },
            { name: "name", type: "text", width: 150, title: "Name" },
            { name: "address", type: "text", width: 200, title: "Address" },
            { name: "city", type: "text", width: 100, title: "City" },
            { name: "province", type: "text", width: 100, title: "Province", filtering: false },
            { name: "postalCode", type: "text", width: 100, title: "Postal Code", filtering: false },
            { name: "country", type: "text", width: 100, title: "Country", filtering: false },
            { name: "energyCostPerKw", type: "number", width: 100, title: "Energy Cost/Min", filtering: false },
            { name: "stopCostPerMinute", type: "number", width: 100, title: "Stop Cost Per Minute", filtering: false },
            {
                type: "control",
                editButton: true,
                deleteButton: true,
            }
        ],

        rowClick: function (args) {
            showMap(args.item);
        }
    });

    setInterval(function () {
        let filter = $("#parkingGrid").jsGrid("getFilter");
        $("#parkingGrid").jsGrid("loadData", filter);
    }, 1000);

    function showMap(item) {
        if (!item || !item.address) {
            console.error("No address provided for mapping.");
            return;
        }

        // Clear the map
        map.eachLayer(function (layer) {
            if (layer instanceof L.Marker) {
                map.removeLayer(layer);
            }
        });

        // Use a geocoding service to convert the address to coordinates (e.g., OpenStreetMap's Nominatim)
        $.get(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(item.address + ' ' + item.city + ' ' + item.country)}`, function (data) {
            if (data && data.length > 0) {
                var lat = data[0].lat;
                var lon = data[0].lon;
                map.setView([lat, lon], 13);
                L.marker([lat, lon]).addTo(map)
                    .bindPopup(`<b>${item.name}</b><br>${item.address}<br>${item.city}, ${item.province} ${item.postalCode}`)
                    .openPopup();
            } else {
                console.error("Address not found: " + item.address);
            }
        });
    }
});
