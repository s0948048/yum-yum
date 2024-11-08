
$('#button-login').on('click', function () {
    if ($('#login-account').val() == "") {
        alert("帳號未輸入")
        return
    }
    else if ($('#login-password').val() == "") {
        alert("密碼未輸入")
        return
    }
    const data = {
        Email: $('#login-account').val(),
        Password: $('#login-password').val()
    }
    console.log("資料"+data["Email"])

    $.ajax({
        url: '/User/LogInPage', // 請求的 URL
        type: 'POST', // 請求類型
        contentType: 'application/json', // 設定資料格式
        data: JSON.stringify(data), // 傳遞的 JSON 資料
        success: function (result) {
            console.log('成功:', result);
            if (result["errorMessage"] == "帳號未註冊") {
                alert(`${result["errorMessage"]}`)
            }
            else {
                window.location.href = result["redirectUrl"]
            }
            
        },
        error: function (xhr, status, error) {
            console.error('錯誤:', error);
        }
    });
})
