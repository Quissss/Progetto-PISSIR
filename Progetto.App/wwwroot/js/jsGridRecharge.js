$(function () {
    let ajax = function (item, verb, json = true) {
        let requestData = json ? JSON.stringify(item) : item;

        return $.ajax({
            type: verb,
            url: "/api/CurrentlyCharging/recharges",
            data: requestData,
            headers: { "X-Connection-Id": connectionId },
            contentType: json ? "application/json" : "application/x-www-form-urlencoded; charset=UTF-8",
        });
    };

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/rechargeHub")
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

    connection.on("RechargeAdded", function (recharge) {
        var grid = $("#jsGridRecharge");
        var data = grid.jsGrid("option", "data");
        data.push(recharge);
        grid.jsGrid("refresh");
    });

    connection.on("RechargeUpdated", function (recharge) {
        var grid = $("#jsGridRecharge");
        var data = grid.jsGrid("option", "data");
        var itemIndex = data.findIndex(item => item.id === recharge.id);
        if (itemIndex > -1) {
            data[itemIndex] = recharge;
            grid.jsGrid("refresh");
        }
    });

    connection.on("RechargeDeleted", function (id) {
        var grid = $("#jsGridRecharge");
        var data = grid.jsGrid("option", "data");
        var itemIndex = data.findIndex(item => item.id === id);
        if (itemIndex > -1) {
            data.splice(itemIndex, 1);
            grid.jsGrid("refresh");
        }
    });

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
            { name: "currentChargePercentage", type: "number", title: "Current %", filtering: false, itemTemplate: (value) => value + " %" },
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
});