$(function () {
    const url = "/api/Car";
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/carHub")
        .build();

    connection.start().catch(err => console.error(err.toString()));

    connection.on("CarAdded", function (car) {
        $("#jsGridCar").jsGrid("insertItem", car);
    });

    connection.on("CarUpdated", function (car) {
        $("#jsGridCar").jsGrid("updateItem", car);
    });

    connection.on("CarDeleted", function (licencePlate) {
        $("#jsGridCar").jsGrid("deleteItem", { licencePlate: licencePlate });
    });

    let ajax = function (item, verb, json = true) {
        let requestData = json ? JSON.stringify(item) : item;

        return $.ajax({
            type: verb,
            url: url,
            data: requestData,
            contentType: json ? "application/json" : "application/x-www-form-urlencoded; charset=UTF-8",
        });
    };

    $("#jsGridCar").jsGrid({
        width: "100%",
        height: "400px",
        editing: true,
        autoload: false,
        filtering: true,
        inserting: true,
        sorting: true,
        paging: true,

        controller: {
            loadData: function (filter) {
                if (filter.status == -1)
                    delete filter.status
                return ajax(filter, "GET", false);
            },
            updateItem: item => ajax(item, "PUT"),
            deleteItem: item => ajax(item, "DELETE"),
            insertItem: item => ajax(item, "POST"),
        },

        fields: [
            { name: "licencePlate", type: "text", title: "Licence Plate", width: 100, validate: "required" },
            { name: "brand", type: "text", title: "Brand", width: 100, validate: "required", filtering: false },
            { name: "model", type: "text", title: "Model", width: 100, validate: "required", filtering: false },
            { name: "isElectric", type: "checkbox", title: "Is Electric", sorting: false, filtering: false },
            {
                name: "status", type: "select", title: "Status", width: 100, items: [
                    { Id: -1, Name: "" },
                    { Id: 0, Name: "Out Of Parking" },
                    { Id: 1, Name: "Waiting" },
                    { Id: 2, Name: "Waiting For Charge" },
                    { Id: 3, Name: "Waiting For ParkingSlot" },
                    { Id: 4, Name: "In Charge" },
                    { Id: 5, Name: "Charged" },
                    { Id: 6, Name: "Parked" },
                ], valueField: "Id", textField: "Name"
            },
            { type: "control", editButton: true, deleteButton: true, sorting: false }
        ],

        onItemInserting: (args) => args.item["ownerId"] = userId,
    });

    $("#jsGridCar").jsGrid("loadData");
});
