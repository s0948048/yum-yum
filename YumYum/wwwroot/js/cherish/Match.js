﻿
$('#reset-left').click(function () {
    $('#Ifilter,#Cfilter,#Dfilter').find('input[type="checkbox"]').prop('checked', false);
    $('#Ifilter input[type="checkbox"]').prev().removeClass('selected')
})


$(
    '#Ifilter input[type="checkbox"],#Cfilter input[type="checkbox"],#Dfilter input[type="checkbox"]'
).change(function () {
    var sortAttr = [];
    var sortCont = [];
    var sortDay = [];
    $('#Ifilter input[type="checkbox"]:checked').each(function (idx,ele) {
        sortAttr.push(Number($(ele).val()))
    })
    $('#Cfilter input[type="checkbox"]:checked').each(function (idx, ele) {
        sortCont.push(Number($(ele).val()))
    })
    $('#Dfilter input[type="checkbox"]:checked').each(function (idx, ele) {
        sortDay.push(Number($(ele).val()))
    })
    
    var sortList = { sortAttr: sortAttr, sortCont: sortCont, sortDay: sortDay };

    console.log(sortList);

    $(this).prev().toggleClass('selected')

    $.ajax({
        url:''
    })






});

$('#Ifilter-active,#Cfilter-active,#Dfilter-active').change(function () {
    $(this).parent().next().toggleClass('filter-hide')
})


// 查看更多 【 -> (1)彈出所有資訊的視窗】
$('.card-body').find('button').click(function () {
    $('#mask').show()
    disableScroll()

    var cherishID = Number($(this)[0].attributes[4].nodeValue);
    $.ajax({
        url: '/cherish/DetailOrder',
        method: 'GET',
        data: { cherishID: cherishID },
        success: function (data) {
            $('#popup-order-info').html(data)
        },
        error: function () {
            alert('伺服器錯誤')
        }
    })


    $('#popup-order-info').removeClass('visible-hide')
})





// 輸入資訊的那個表單  【 -> (3)彈出/關閉對方資訊的視窗】
//  1.要AJAX
$('#btn-match-info-post').click(function (e) {
    e.preventDefault();

    var fromD = $(this).parent();

    console.log(fromD);

    var cherishID = Number($('#CherishId').val());

    $.ajax({
        url: '/cherish/ApplyCherish',
        method: 'POST',
        data: fromD.serialize(), // 將表單序列化
        success: function (data) {
            alert(`執行結果: ${data.message}`)

            $("#popup-order-match").addClass('visible-hide');
            $.ajax({
                url: '/cherish/ContactOrder',
                method: 'POST',
                data: { cherishID: cherishID },
                success: function (data) {
                    $('#contact-info').html(data);
                    $('#contact-info').removeClass('visible-hide');
                }
            })
        },
        error: function (xhr) {
            alert(`執行結果: ${xhr.responseJSON.message}`)
        },
    });
})
$('#btn-match-off').click(function () {
    $(this).parent().parent().addClass('visible-hide');
    $('#mask').hide()
    enableScroll()
})




// 搜尋列
$('#CitySelect').on('change', function () {
    var cityKey = $(this).val();
    $('#RegionSelect').empty();
    $.ajax({
        url: '/cherish/GetRegions',
        method: 'GET',
        data: { CityKey: cityKey },
        success: function (data) {
            $('#RegionSelect').append('<option value="">--請選擇地區--</option>').append(data)
        }
    })
})

function disableScroll() {
    $('body').css('overflow-y', 'hidden');

    $(document).on('scroll touchmove mousewheel', function (e) {
        e.preventDefault();
        e.stopPropagation();
        return false;
    });
}

function enableScroll() {
    $('body').css('overflow', 'scroll');
    $(document).off('scroll touchmove mousewheel');
}
