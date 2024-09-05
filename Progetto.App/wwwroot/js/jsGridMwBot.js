const url = "/api/MwBot";

let ajax = function (item, verb, json = true) {
    let requestData = json ? JSON.stringify(item) : item;

    return $.ajax({
        type: verb,
        url: url,
        data: requestData,
        contentType: json ? "application/json" : "text/plain",
    });
};

function turnBot(item, action) {
    var endpoint = action === "on" ? "on" : "off";
    $.ajax({
        type: "PUT",
        url: url + "/" + endpoint,
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

    $("#mwBotGrid").jsGrid({
        width: "100%",
        height: "400px",
        editing: true,
        autoload: true,
        filtering: true,
        inserting: true,
        sorting: true,
        paging: true,

        controller: {
            loadData: function (filter) {
                return ajax(filter, "GET", false);
            },
            updateItem: function (item) {
                return ajax(item, "PUT");
            },
            insertItem: function (item) {
                return ajax(item, "POST");
            },
            deleteItem: function (item) {
                return ajax(item, "DELETE");
            }
        },

        fields: [
            { name: "id", type: "number", title: "ID", filtering: false, editing: false },
            { name: "batteryPercentage", type: "number", title: "Battery Percentage", filtering: false, editing: false },
            {
                name: "parkingId", type: "select", width: 100, title: "Location", items: parkingOptions, valueField: "value", textField: "text",
                itemTemplate: function (value, item) {
                    let parking = parkingOptions.find(p => p.value == value);
                    return parking ? parking.text : "";
                }
            },
            {
                name: "status", type: "select", title: "Status", items: [
                    {},
                    { Name: "Offline", Id: 0 },
                    { Name: "Online", Id: 1 }
                ], valueField: "Id", textField: "Name", editing: false,
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
                            .on("click", function () {
                                turnBot(item, "on");
                            });
                    } else {
                        return $("<button>")
                            .addClass("btn btn-sm btn-outline-danger fa-solid fa-power-off")
                            .on("click", function () {
                                turnBot(item, "off");
                            });
                    }
                }
            },
            {
                type: "control", editButton: true, deleteButton: true,
            }
        ]
    });

    // TODO: implement SignalR
    setInterval(function () {
        let filter = $("#mwBotGrid").jsGrid("getFilter");
        $("#mwBotGrid").jsGrid("loadData", filter);
    }, 1000);
});
