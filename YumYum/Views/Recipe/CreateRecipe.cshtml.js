//字數計數
$('#step-text01').on('input', function () {
    $('#step-count01').text(`${$('#step-text01').val().length}/100`);
})

$('#step-text02').on('input', function () {
    $('#step-count02').text(`${$('#step-text02').val().length}/100`);
})

$('#recipe-introduction').on('input', function () {
    $('#recipe-introcount').text(`${$('#recipe-introduction').val().length}/100`);
})

$('#recipe-name').on('input', function () {
    $('#recipe-namecount').text(`${$('#recipe-name').val().length}/15`);
})



//接收步驟的變數
var step = 2;


$('#step-plus').on('click', function () {
    step += 1;
    //如果step是偶數的話就在right-steps下append新的步驟框
    if (step % 2 == 0) {

        var stepRight = $('<div>', {
            id: `step0${step}`,
            class: 'card step-right',    // 設定 class 屬性
            html: `<img b-mc3u0u1te1 src="/img/icon/AddPhoto.png" class="card-img-top img-load" alt="哈哈" style="height: 13rem;"><div b-mc3u0u1te1  class= "text-center m-0 step-bk"> <p b-mc3u0u1te1  class="m-0 mt-1" style="font-size: 1.5rem;">STEP ${step}</p></div ><div b-mc3u0u1te1  class="card-body" style="width:286px;height:152px"><textarea b-mc3u0u1te1 id=step-text0${step} class="card-text" style="width:100%;height:100%;border-color:transparent" placeholder="點我輸入內容"></textarea></div>`
        }).attr('b-mc3u0u1te1', '');
        //把前一個步驟的mb-200屬性移除
        $(`#step-countdiv0${step - 1}`).removeClass('mb-200')
        //中間連線
        $('#right-steps').append('<p b-mc3u0u1te1 style="position: absolute;margin-top: 50.2rem;margin-left:-10px;color:var(--yum-secondary-4)">●────────────</p>')
        //步驟本人
        $('#right-steps').append(stepRight);
        //計數
        $('#right-steps').append(`<div b-mc3u0u1te1 class="d-flex mb-200" id="step-countdiv0${step}" style="margin-right:146px"><label b-mc3u0u1te1 id ="step-count0${step}" class= "d-flex ms-auto">0/100</label></div>`);
        //新增計數邏輯
        $('#right-steps').append(`<script>$('#step-text0${step}').on('input', function () {$('#step-count0${step}').text(\`\${$('#step-text0${step}').val().length}/100\`);}) </script>`)
        //如果step是奇數的話就在left-steps下append新的步驟框
    } else {
        var stepLeft = $('<div>', {
            id: `step0${step}`,
            class: 'card step-left',    // 設定 class 屬性
            html: `<img b-mc3u0u1te1  src="/img/icon/AddPhoto.png" class="card-img-top img-load" alt="哈哈" style="height: 13rem;"><div b-mc3u0u1te1 class= "text-center m-0 step-bk"><p b-mc3u0u1te1  class="m-0 mt-1" style="font-size: 1.5rem;">STEP ${step}</p></div ><div b-mc3u0u1te1  class="card-body" style="width:286px;height:152px"><textarea b-mc3u0u1te1 id="step-text0${step}" class="card-text" style="width:100%;height:100%;border-color:transparent" placeholder="點我輸入內容"></textarea></div>`
        }).attr('b-mc3u0u1te1', '');
        //把前一個步驟的mb-200屬性移除
        $(`#step-countdiv0${step - 1}`).removeClass('mb-200')
        //中間連線
        $('#left-steps').append('<p b-mc3u0u1te1 style="position: absolute;margin-top: 50.2rem;margin-left: 24.52rem;color:var(--yum-secondary-4)">──────────────●</p>')
        //步驟本人
        $('#left-steps').append(stepLeft);
        //計數
        $('#left-steps').append(`<div id="step-countdiv0${step}" class="d-flex mb-200" style="margin-right: 260px"><label id ="step-count0${step}" class="d-flex ms-auto">0/100</label></div>`);
        //新增計數邏輯
        $('#left-steps').append(`<script>$('#step-text0${step}').on('input', function () {$('#step-count0${step}').text(\`\${$('#step-text0${step}').val().length}/100\`);}) </script>`)
    }


})
let selectedFile = null
//上傳圖片
$(document).on('click', '.img-load', function () {
    //將點擊的img元素賦值給一個變數
    const imgElement = $(this);
    //設定file-load的data將imgElement的元素裝到targerImg這個容器裡
    $('#file-load').data('targetImg', imgElement)
    $('#file-load').click();
});
$('#file-load').on('change', function (event) {
    //選取使用者選擇的第一個檔案
    selectedFile = event.target.files[0]
    const targetImg = $(this).data('targetImg')
    //將圖片載入到步驟的圖片
    if (selectedFile && targetImg) {
        //讀取檔案
        const reader = new FileReader();
        reader.onload = function (e) { targetImg.attr('src', e.target.result) }
        reader.readAsDataURL(selectedFile);
    }
})