
const url = "/api/Parking";

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
    $("#parkingGrid").jsGrid({
        width: "100%",
        height: "400px",
        autoload: true,
        filtering: true,
        inserting: true,
        editing: true,
        sorting: true,
        paging: true,
        controller: {
            loadData: filter => ajax(filter, "GET", false),
            updateItem: item => ajax(item, "PUT"),
            deleteItem: item => ajax(item, "DELETE"),
            insertItem: item => ajax(item, "POST"),
        },

        data: parkings,
        fields: [
            { name: "id", visible: false },
            { name: "name", type: "text", width: 150, title: "Name",  },
            { name: "address", type: "text", width: 200, title: "Address" },
            { name: "city", type: "text", width: 100, title: "City" },
            { name: "province", type: "text", width: 100, title: "Province", filtering: false},
            { name: "postalCode", type: "text", width: 100, title: "Postal Code" ,filtering: false },
            { name: "country", type: "text", width: 100, title: "Country", filtering: false },
            { name: "energyCostPerKw", type: "number", width: 100, title: "Energy Cost/Min", filtering: false },
            { name: "stopCostPerMinute", type: "number", width: 100, title: "Stop Cost Per Minute", filtering: false },
            {
                type: "control",
                editButton: true,
                deleteButton: true,
            }
        ]
    });

    setInterval(function () {
        let filter = $("#parkingGrid").jsGrid("getFilter");
        $("#parkingGrid").jsGrid("loadData", filter);
    }, 1000);
});