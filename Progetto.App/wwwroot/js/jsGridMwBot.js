
const url = "/api/MwBot";

let ajax = function (item, verb, json = true) {
    return $.ajax({
        type: verb,
        url: url,
        data: json ? JSON.stringify(item) : item,
        dataType: "json",
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
        },
        error: function (xhr, status, error) {
            console.error("Error turning bot " + action + ": " + error);
        }
    });
}
$(function () {
    $("#mwBotGrid").jsGrid({
        width: "100%",
        height: "400px",
        editing: false,
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
            { name: "id", type: "number", width: 50, title: "ID", readOnly: true },
            { name: "batteryPercentage", type: "number", width: 150, title: "Battery Percentage" },
            {
                name: "status", type: "select", width: 150, title: "Status", items: [
                    { Name: "Offline", Id: 0 },
                    { Name: "Online", Id: -1 }
                ], valueField: "Id", textField: "Name",
                itemTemplate: function (value) {
                    switch (value) {
                        case 0: return "Offline";
                        case 1: return "Standby";
                        case 2: return "Charging Car";
                        case 3: return "Recharging";
                        case 4: return "Moving to Car";
                        case 5: return "Moving to Recharge";
                    }
                }
            },
            
            {
                type: "custom",
                name: "On/Off",
                width: 50,
                itemTemplate: function (value, item) {
                    if (item.status === 0) {
                        return $("<button>").text("Turn On")
                            .addClass("btn btn-success")
                            .on("click", function () {
                                turnBot(item, "on");
                            });
                    } else {
                        return $("<button>").text("Turn Off")
                            .addClass("btn btn-danger")
                            .on("click", function () {
                                turnBot(item, "off");
                            });
                    }
                }

            },
            {
                type: "control", editButton: false, deleteButton: true,
            }
        ]
    });
});