
const url = "/api/CurrentlyCharging/recharges";

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
        sorting: true,
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
            { name: "carPlate", type: "text", title: "Car Plate", filtering: true },
            { name: "startChargingTime", type: "text", title: "Start Time", width: 150, filtering: false, itemTemplate: (value) => formatDate(value) },
            { name: "endChargingTime", type: "text", title: "End Time", width: 150, filtering: false, itemTemplate: (value) => formatDate(value) },
            { name: "startChargePercentage", type: "number", title: "Start %", filtering: false, itemTemplate: (value) => value + " %" },
            { name: "currentChargePercentage", type: "number", title: "Current %", filtering: false, itemTemplate: (value) => value +  " %" },
            { name: "targetChargePercentage", type: "number", title: "Target %", filtering: false, itemTemplate: (value) => value + " %" },
            { name: "energyConsumed", type: "number", title: "Energy Consumed", filtering: false, itemTemplate: (value) => value + " kw" },
            { name: "totalCost", type: "number", title: "Total Cost", filtering: false, itemTemplate: (value) => value + " &euro;" }
        ]
    });

    function formatDate(value) {
        if (!value) return "";
        let date = new Date(value);
        return date.toLocaleString();
    }

    // TODO: implement SignalR
    setInterval(function () {
        let grid = $("#jsGridRecharge");
        let sorting = grid.jsGrid("getSorting");
        let filter = grid.jsGrid("getFilter");

        sorting.field === undefined && sorting.order === undefined ?
            grid.jsGrid("loadData", filter).done(() => grid.jsGrid()) :
            grid.jsGrid("loadData", filter).done(() => grid.jsGrid("sort", sorting.field, sorting.order));
    }, 1000);
});