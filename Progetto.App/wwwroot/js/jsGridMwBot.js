
function turnBot(item, action) {
    var endpoint = action === "on" ? "on" : "off";
    $.ajax({
        type: "PUT",
        url: "/api/MwBot/" + endpoint,
        data: JSON.stringify(item),
        contentType: "application/json",
        success: function (response) {
            console.log("Bot turned " + action);
            $('#mwBotGrid').jsGrid('loadData');
        },
        error: function (xhr, status, error) {
            console.error("Error turning bot " + action + ": " + error);
        }
    });
}

$(function () {
    let ajax = function (item, verb, json = true) {
        let requestData = json ? JSON.stringify(item) : item;

        return $.ajax({
            type: verb,
            url: "/api/MwBot",
            data: requestData,
            headers: { "X-Connection-Id": connectionId },
            contentType: json ? "application/json" : "application/x-www-form-urlencoded; charset=UTF-8",
        });
    };

    const connection = new signalR.HubConnectionBuilder()
    .withUrl("/mwBotHub")
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

    connection.on("MwBotAdded", function (bot) {
        var grid = $("#mwBotGrid");
        var data = grid.jsGrid("option", "data");
        data.push(bot);
        grid.jsGrid("refresh");
    });

    connection.on("MwBotUpdated", function (bot) {
        var grid = $("#mwBotGrid");
        var data = grid.jsGrid("option", "data");
        var itemIndex = data.findIndex(item => item.id === bot.id);
        if (itemIndex > -1) {
            data[itemIndex] = bot;
            grid.jsGrid("refresh");
        }
    });

    connection.on("MwBotDeleted", function (id) {
        var grid = $("#mwBotGrid");
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
        error: (error) => console.error("Errore nel caricamento dei dati dei parcheggi:", error),
    });

    $("#mwBotGrid").jsGrid({
        width: "100%",
        editing: true,
        autoload: true,
        filtering: true,
        inserting: true,
        sorting: true,
        paging: true,
        pageSize: 10,

        controller: {
            loadData: filter => ajax(filter, "GET", false),
            updateItem: item => ajax(item, "PUT"),
            deleteItem: item => ajax(item, "DELETE"),
            insertItem: item => ajax(item, "POST"),
        },

        fields: [
            { name: "id", width: 25, type: "number", title: "ID", filtering: false, editing: false },
            { name: "batteryPercentage", width: 50, type: "number", title: "Battery %", filtering: false, editing: false, itemTemplate: (value) => value + " %" },
            {
                name: "parkingId", type: "select", width: 150, title: "Location", items: parkingOptions, valueField: "value", textField: "text",
                itemTemplate: function (value, item) {
                    let parking = parkingOptions.find(p => p.value == value);
                    return parking ? parking.text : "";
                }
            },
            {
                name: "status",
                type: "select",
                title: "Status",
                items: [
                    {},
                    { Name: "Offline", Id: 0 },
                    { Name: "Online", Id: 1 }
                ],
                valueField: "Id",
                textField: "Name",
                editing: false,
                inserting: false,
                itemTemplate: function (value) {
                    switch (value) {
                        case 0: return "Offline";
                        case 1: return "Standby";
                        case 2: return "Moving to Car";
                        case 3: return "Charging Car";
                        case 4: return "Moving to Recharge";
                        case 5: return "Recharging";
                    }
                }
            },
            {
                type: "custom",
                width: 18,
                itemTemplate: function (value, item) {
                    if (item.status === 0) {
                        return $("<button>")
                            .addClass("btn btn-sm btn-outline-success fa-solid fa-power-off")
                            .on("click", () => turnBot(item, "on"));
                    } else {
                        return $("<button>")
                            .addClass("btn btn-sm btn-outline-danger fa-solid fa-power-off")
                            .on("click", () => turnBot(item, "off"));
                    }
                },
                editing: false,
                inserting: false,
            },
            { type: "control", editButton: true, deleteButton: true, sorting: false }
        ],


        onItemInserting: (args) => {
            args.item["status"] = 0;
        },
    });
});
