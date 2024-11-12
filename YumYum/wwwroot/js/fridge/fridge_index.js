$(document).ready(function () {
    $("#btn-edit").on("click", function () {
        window.location.href = '@Url.Action("Edit", "Fridge")';
    });
});