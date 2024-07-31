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
    let map = L.map('map').setView([45.4642, 9.1900], 13); // Default Milan, Italy
    let routingControl;
    let userPosition;
    
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
    }).addTo(map);

    // Get user location
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            userPosition = [position.coords.latitude, position.coords.longitude];
            map.setView(userPosition, 13);
            L.marker(userPosition).addTo(map)
                .bindPopup("You are here")
                .openPopup();
        }, function () {
            console.error("Geolocation is not supported by this browser or permission denied.");
        });
    } else {
        console.error("Geolocation is not supported by this browser.");
    }

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

    // TODO: implement SignalR
    setInterval(function () {
        let filter = $("#parkingGrid").jsGrid("getFilter");
        $("#parkingGrid").jsGrid("loadData", filter);
    }, 1000);

    function showMap(item) {
        if (!item || !item.address) {
            console.error("No address provided for mapping.");
            return;
        }
        
        if (routingControl) {
            map.removeControl(routingControl);
        }
        
        $.get(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(item.address + ' ' + item.city + ' ' + item.country)}`, function (data) {
            if (data && data.length > 0) {
                var lat = data[0].lat;
                var lon = data[0].lon;
                map.setView([lat, lon], 13);
                
                routingControl = L.Routing.control({
                    waypoints: [
                        L.latLng(userPosition),
                        L.latLng(lat, lon)
                    ],
                    createMarker: function (i, waypoint, n) {
                        var marker = L.marker(waypoint.latLng).bindPopup(`<b>${item.name}</b><br>${item.address}<br>${item.city}, ${item.province} ${item.postalCode}`);
                        return marker;
                    },
                    routeWhileDragging: true
                }).addTo(map);
            } else {
                console.error("Address not found: " + item.address);
            }
        });
    }
});
