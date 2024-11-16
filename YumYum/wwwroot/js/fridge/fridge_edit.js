$("#btn-return").on("click", function () {
    window.location.href = "/fridge/index";
});

// Add Ajax HERE in this function!
$('#Ifilter input[type="checkbox"]').on('change', function () {
    console.log($(this).prop('checked'));
    console.log($('#Ifilter input[type="checkbox"]:checked'));
    $(this).prev().toggleClass('selected');
});