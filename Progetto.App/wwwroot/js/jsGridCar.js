const url = "/api/Car"; 

let ajax = function (item, verb, json = true) {
    let requestData = json ? JSON.stringify(item) : item;

    return $.ajax({
        type: verb,
        url: url,
        data: requestData,
        contentType: json ? "application/json" : "application/x-www-form-urlencoded; charset=UTF-8",
    });
};

$(function () {
    $("#jsGridCar").jsGrid({
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

    // TODO: implement SignalR
    setInterval(function () {
        let grid = $("#jsGridCar");
        let sorting = grid.jsGrid("getSorting");
        let filter = grid.jsGrid("getFilter");

        sorting.field === undefined && sorting.order === undefined ?
            grid.jsGrid("loadData", filter).done(() => grid.jsGrid()) :
            grid.jsGrid("loadData", filter).done(() => grid.jsGrid("sort", sorting.field, sorting.order));
    }, 1000);
});
