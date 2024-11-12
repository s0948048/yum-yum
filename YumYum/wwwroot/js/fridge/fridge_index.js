$(document).ready(function () {
    console.log("jQuery Loaded");
    $("#btn-edit").on("click", function () {
        window.location.href = '@Url.Action("Edit", "Fridge")';
    });
});