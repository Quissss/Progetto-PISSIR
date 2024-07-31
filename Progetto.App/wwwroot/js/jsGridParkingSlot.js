const url = "/api/ParkingSlot";

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
    let parkingOptions = [];
    
    $.ajax({
        url: '/api/Parking',
        method: 'GET',
        async: false,
        success: function (data) {
            parkingOptions = data.map(p => ({ value: p.id, text: `${p.name} [${p.city} - ${p.address}]` }));
        },
        error: function (error) {
            console.error("Errore nel caricamento dei dati dei parcheggi:", error);
        }
    });

    $.getJSON(url + "/statuses", function (statuses) {
        $("#parkingSlotGrid").jsGrid({
            width: "100%",
            height: "400px",
            autoload: true,
            filtering: true,
            inserting: true,
            editing: false, 
            sorting: true,
            paging: true,
            controller: {
                loadData: filter => ajax(filter, "GET", false),
                insertItem: item => ajax(item, "POST"),
                deleteItem: item => ajax(item, "DELETE"),
            },
            fields: [
                { name: "id", visible: false, filtering: false },
                { name: "number", type: "number", width: 50, title: "Slot Number " },
                {
                    name: "parkingId", type: "select", width: 100, title: "Location", items: parkingOptions, valueField: "value", textField: "text",
                    itemTemplate: function (value, item) {
                        let parking = parkingOptions.find(p => p.value == value);
                        return parking ? parking.text : "";
                    }
                },
                { name: "status", type: "select", items: statuses, width: 100, title: "Status", valueField: "id", textField: "name" },
                {
                    type: "control",
                    deleteButton: true,
                    editButton: false,
                }
            ]
        });
    });

    // TODO: implement SignalR
    setInterval(function () {
        let filter = $("#parkingSlotGrid").jsGrid("getFilter");
        $("#parkingSlotGrid").jsGrid("loadData", filter);
    }, 1000);
});
