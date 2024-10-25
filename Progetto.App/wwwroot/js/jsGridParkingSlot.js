$(function () {
    let ajax = function (item, verb, json = true) {
        let requestData = json ? JSON.stringify(item) : item;

        return $.ajax({
            type: verb,
            url: "/api/ParkingSlot",
            data: requestData,
            headers: { "X-Connection-Id": connectionId },
            contentType: json ? "application/json" : "application/x-www-form-urlencoded; charset=UTF-8",
        });
    };

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/parkingSlotHub")
        .build();

    let connectionId = null;

    connection.start()
        .then(() => {
            console.log("SignalR connected");
            return connection.invoke("GetConnectionId");
        })
        .then(id => {
            connectionId = id;
            console.log("Connection ID:", connectionId);
        })
        .catch(err => console.error("Error connecting to SignalR:", err));

    connection.on("ParkingSlotAdded", function (parkingSlot) {
        var grid = $("#parkingSlotGrid");
        var data = grid.jsGrid("option", "data");
        data.push(parkingSlot);
        grid.jsGrid("refresh");
    });

    connection.on("ParkingSlotUpdated", function (parkingSlot) {
        var grid = $("#parkingSlotGrid");
        var data = grid.jsGrid("option", "data");
        var itemIndex = data.findIndex(item => item.id === parkingSlot.id);
        if (itemIndex > -1) {
            data[itemIndex] = parkingSlot;
            grid.jsGrid("refresh");
        }
    });

    connection.on("ParkingSlotDeleted", function (id) {
        var grid = $("#parkingSlotGrid");
        var data = grid.jsGrid("option", "data");
        var itemIndex = data.findIndex(item => item.id === id);
        if (itemIndex > -1) {
            data.splice(itemIndex, 1);
            grid.jsGrid("refresh");
        }
    });

    let parkingOptions = [];

    $.ajax({
        url: '/api/Parking',
        method: 'GET',
        async: false,
        success: (data) => parkingOptions = data.map(p => ({ value: p.id, text: `${p.name} [${p.city} - ${p.address}]` })),
        error: (error) => console.error("Errore nel caricamento dei dati dei parcheggi:", error)
    });

    $.getJSON("/api/ParkingSlot/statuses", function (statuses) {
        $("#parkingSlotGrid").jsGrid({
            width: "100%",
            autoload: true,
            filtering: true,
            inserting: true,
            editing: false,
            sorting: true,
            paging: true,
            pageSize: 10,

            controller: {
                loadData: filter => ajax(filter, "GET", false),
                insertItem: item => ajax(item, "POST"),
                deleteItem: item => ajax(item, "DELETE"),
            },

            fields: [
                { name: "id", visible: false, filtering: false },
                { name: "number", type: "number", width: 50, title: "Slot Number " },
                {
                    name: "parkingId",
                    type: "select",
                    width: 100,
                    title: "Location",
                    items: parkingOptions,
                    valueField: "value",
                    textField: "text",
                    itemTemplate: function (value, item) {
                        let parking = parkingOptions.find(p => p.value == value);
                        return parking ? parking.text : "";
                    }
                },
                { name: "status", type: "select", items: statuses, width: 100, title: "Status", valueField: "id", textField: "name" },
                { type: "control", deleteButton: true, editButton: false, sorting: false },
            ]
        });
    });
});
