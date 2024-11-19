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
            url: 'FilterIngredients',
            type: 'POST',
            data: JSON.stringify(selectedId),
            contentType: 'application/json',
            success: function (response) {
                // Update tag section dynamically

                // Check selected ID
                console.log(response);

                // Clear section
                $('#ingredient-list').empty();

                // Append updated ingredients
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

$('#ingredientInput').on('input', function () {
    var keyword = $(this).val();

    $.ajax(
        {
            url: 'SearchIngredients',
            type: 'GET',
            data: { searchKeyword: keyword },
            success: function (data) {
                $('#ingredient-list').html(data);
            },
            error: function () {
                console.log('An error occurred while fetching the data.');
            }
        }
    );
});

$('#ingredient-list').on('click', '.tag-button', function () {
    let clickedButtonHtml = $(this).prop('outerHTML');
    console.log(clickedButtonHtml);

    let ingredientName = $(this).find('span').text().trim();
    console.log(ingredientName);

    $.ajax({
        url: 'GetUnitsByIngredientName', 
        type: 'GET', 
        data: { ingredientName: ingredientName }, 
        success: function (response) {
            console.log(response);

            let optionsHTML = '';
            response.forEach(function (unit) {
               optionsHTML += `<option value="${unit.UnitID}">${unit.UnitName}</option>`
            })

            let newRowHtml = `
                <div class="row align-items-center mb-1">
                    <div class="col-md-3 col-4">
                        ${clickedButtonHtml}
                    </div>
                    <div class="col-md-2 col-4 text-center">
                        <input type="number" class="form-control" value="1" min="1">
                    </div>
                    <div class="col-md-2 col-4">
                        <select class="form-select">
                            <option selected>Unit</option>
                        </select>
                    </div>
                    <div class="col-md-3 col-8 text-center">
                        <input type="date" class="form-control">
                    </div>
                    <div class="col-md-1 col-4 d-flex justify-content-end">
                        <button class="btn d-flex justify-content-center align-items-center ps-1 pe-1 btn-delete">
                            <img src="../img/icon/delete.svg" alt="" />
                        </button>
                    </div>
                </div>
            `;

            $('.item-list').prepend(newRowHtml);
        }, 
        error: function (error) {
            console.log('Error: ' + error);
        }
    });

    
});

$('.item-list').on('click', '.btn-delete', function () {
    $(this).closest('.row').remove();
});