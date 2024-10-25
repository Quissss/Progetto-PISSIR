$(function () {
    let ajax = function (item, verb, json = true) {
        let requestData = json ? JSON.stringify(item) : item;
        let requestUrl = "/api/Car";

        if (verb === "GET") {
            requestUrl = "/api/Car/my-cars";
        }

        return $.ajax({
            type: verb,
            url: requestUrl,
            data: requestData,
            headers: { "X-Connection-Id": connectionId },
            contentType: json ? "application/json" : "application/x-www-form-urlencoded; charset=UTF-8",
        });
    };

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/carHub")
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

    connection.on("CarAdded", function (car) {
        var grid = $("#jsGridCar");
        var data = grid.jsGrid("option", "data");
        data.push(car);
        grid.jsGrid("refresh");
    });

    connection.on("CarUpdated", function (car) {
        var grid = $("#jsGridCar");
        var data = grid.jsGrid("option", "data");
        var itemIndex = data.findIndex(item => item.plate === car.plate);
        if (itemIndex > -1) {
            data[itemIndex] = car;
            grid.jsGrid("refresh");
        }
    });

    connection.on("CarDeleted", function (plate) {
        var grid = $("#jsGridCar");
        var data = grid.jsGrid("option", "data");
        var itemIndex = data.findIndex(item => item.plate === plate);
        if (itemIndex > -1) {
            data.splice(itemIndex, 1);
            grid.jsGrid("refresh");
        }
    });

    $("#jsGridCar").jsGrid({
        width: "100%",
        editing: true,
        autoload: true,
        filtering: true,
        inserting: true,
        sorting: true,
        paging: true,
        pageSize: 10,

        controller: {
            loadData: function (filter) {
                if (filter.status == -1) delete filter.status
                return ajax(filter, "GET", false);
            },
            updateItem: item => ajax(item, "PUT"),
            deleteItem: item => ajax(item, "DELETE"),
            insertItem: item => ajax(item, "POST"),
        },

        fields: [
            { name: "plate", type: "text", title: "Licence Plate", width: 100, validate: "required" },
            { name: "brand", type: "text", title: "Brand", width: 100, validate: "required", filtering: false },
            { name: "model", type: "text", title: "Model", width: 100, validate: "required", filtering: false },
            { name: "isElectric", type: "checkbox", title: "Is Electric", sorting: false, filtering: false },
            {
                name: "status",
                type: "select",
                title: "Status",
                width: 100,
                items: [
                    { Id: -1, Name: "" },
                    { Id: 0, Name: "Out Of Parking" },
                    { Id: 1, Name: "Waiting" },
                    { Id: 2, Name: "Waiting For Charge" },
                    { Id: 3, Name: "Waiting For ParkingSlot" },
                    { Id: 4, Name: "In Charge" },
                    { Id: 5, Name: "Charged" },
                    { Id: 6, Name: "Parked" },
                ],
                valueField: "Id",
                textField: "Name",
                editing: false,
                inserting: false,
            },
            { type: "control", editButton: true, deleteButton: true, sorting: false }
        ],

        onItemInserting: (args) => {
            args.item["ownerId"] = userId;
            args.item["status"] = 0;
        },
    });
});
