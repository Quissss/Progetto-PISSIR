
const url = "/api/CurrentlyCharging";

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
    $("#jsGridRecharge").jsGrid({
        width: "100%",
        height: "400px",
        autoload: true,
        filtering: true,
        sorting: true,
        paging: true,
        controller: {
            loadData: filter => ajax(filter, "GET", false),
            updateItem: item => ajax(item, "PUT"),
            deleteItem: item => ajax(item, "DELETE"),
            insertItem: item => ajax(item, "POST"),
        },

        fields: [
            { name: "carPlate", type: "text", title: "Car Plate", width: 50 },
            { name: "startChargingTime", type: "text", title: "Start Charging Time", width: 75 },
            { name: "endChargingTime", type: "text", title: "End Charging Time", width: 75 },
            { name: "startChargePercentage", type: "number", title: "Start Charge %", width: 50 },
            { name: "currentChargePercentage", type: "number", title: "Current Charge %", width: 50 },
            { name: "targetChargePercentage", type: "number", title: "Target Charge %", width: 50 },
            { name: "energyConsumed", type: "number", title: "Energy Consumed", width: 50 },
            { name: "totalCost", type: "number", title: "Total Cost", width: 50 }
        ]
    });


    setInterval(function () {
        let filter = $("#jsGridRecharge").jsGrid("getFilter");
        $("#jsGridRecharge").jsGrid("loadData", filter);
    }, 1000);
});