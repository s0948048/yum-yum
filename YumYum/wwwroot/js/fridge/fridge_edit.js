//const { type, error } = require("jquery");

$("#btn-return").on("click", function () {
    window.location.href = "/fridge/index";
});

$('.ingredient-attribute-checkbox').on('change', function () {
    //console.log($(this).prop('checked'));
    $(this).prev().toggleClass('selected');

    // Get all selected ID
    let selectedId = [];
    $('.ingredient-attribute-checkbox:checked').each(function () {
        selectedId.push($(this).val());
    });
    console.log(selectedId);

    // Ajax request to get filtered tags
    $.ajax(
        {
            url: '@Url.Action("FilterIngredients", "Fridge")',
            type: 'POST',
            data: JSON.stringify(selectedId),
            contentType: 'application/json',
            success: function (response) {
                // Update tag section dynamically

                // Clear section
                $('#ingredient-list').empty();

                // Update
                $.each(response, function (index, item) {
                    $('#ingredient-list').append(
                        `
                        <button class="tag-button d-inline-flex align-items-center btn btn-tags">
                                <img src="${item.IngredientIcon}" alt="" />
                                <span>${item.IngredientName}</span>
                        </button>
                        `
                    );
                });
            },
            error: function (xhr, status, error) {
                console.error('Error: ' + error);
            }
        }
    );
});